using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentalFeatures.Async
{
	public abstract class AsyncStateMachine<T> : TaskCompletionSource<T>, IDisposable
	{
		private readonly bool _catchContext;

		private readonly SynchronizationContext _context;
		private readonly CancellationToken _token;
		private IEnumerator<Task> _enumerator;

		private Task _lastTask;

		protected AsyncStateMachine(bool catchContext = false, CancellationToken token = default(CancellationToken))
			: base(token)
		{
			_catchContext = catchContext;
			_token = token;
			if (catchContext) _context = SynchronizationContext.Current;
		}

		public virtual void Dispose()
		{
			_enumerator?.Dispose();
		}

		public AsyncStateMachine<T> Start(bool newThread = false)
		{
			try
			{
				_enumerator = Run().GetEnumerator();

				if (_catchContext) _context.Post(_ => MoveNext(), null);
				else if (newThread) System.Threading.Tasks.Task.Factory.StartNew(MoveNext, _token);
				else MoveNext();
				return this;
			}
			catch (Exception)
			{
				return this;
			}
		}

		private void MoveNext()
		{
			try
			{
				while (_enumerator.MoveNext())
				{
					var task = _lastTask = _enumerator.Current;
					if (task == null) break;
					if (task.IsCompleted || task.IsCanceled || task.IsFaulted) continue;
					task.ContinueWith(ContinuationAction, CancellationToken.None);
					return;
				}
				if (!Task.IsCompleted && !Task.IsCanceled || !Task.IsFaulted)
					TrySetException(new InvalidOperationException("No result"));
			}
			catch (Exception exp)
			{
				TrySetException(exp);
			}

			//TrySetException(new InvalidOperationException("No Input"));
		}

		protected void CheckPoint()
		{
			_token.ThrowIfCancellationRequested();
			if (_lastTask == null) return;
			if (_lastTask.IsCanceled) throw new TaskCanceledException(_lastTask);
			if (_lastTask.IsFaulted) throw _lastTask.Exception;
		}

		public TResult GetLastResult<TResult>()
		{
			CheckPoint();
			var task = _lastTask as Task<TResult>;
			if (task != null)
			{
				return task.Result;
			}
			throw new Exception();
		}

		private void ContinuationAction(Task task)
		{
			if (_catchContext) _context.Post(_ => MoveNext(), null);
			else MoveNext();
		}

		protected abstract IEnumerable<Task> Run();
	}

	public class WrappedAsyncStateMachine<T> : AsyncStateMachine<T>
	{
		private readonly Func<WrappedAsyncStateMachine<T>, IEnumerable<Task>> _action;

		public WrappedAsyncStateMachine(Func<WrappedAsyncStateMachine<T>, IEnumerable<Task>> action)
		{
			_action = action;
		}

		protected override IEnumerable<Task> Run()
		{
			return _action(this);
		}
	}
}