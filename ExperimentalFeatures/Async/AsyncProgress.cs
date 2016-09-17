using System;
using System.Threading;

namespace ExperimentalFeatures.Async
{
	public class AsyncProgress<TProgress>
	{
		private readonly Action<object, TProgress> _action;
		private readonly SynchronizationContext _context;
		private readonly object _token;

		public AsyncProgress(Action<object, TProgress> action, object token)
		{
			_context = SynchronizationContext.Current;
			_action = action;
			_token = token;
		}

		public void InvokeSync(TProgress progress)
		{
			_context.Send(_ => _action.Invoke(_token, progress), null);
		}

		public void InvokeAsync(TProgress progress)
		{
			_context.Post(_ => _action.Invoke(_token, progress), null);
		}
	}
}