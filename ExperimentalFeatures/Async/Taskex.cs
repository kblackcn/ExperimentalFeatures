using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExperimentalFeatures.Async
{
	public static class TaskEx
	{
		public static Task<T> Wrap<T>(Func<WrappedAsyncStateMachine<T>, IEnumerable<Task>> action)
			=> new WrappedAsyncStateMachine<T>(action).Start().Task;

		public static Task<T> FromResult<T>(T result)
		{
			var completionSource = new TaskCompletionSource<T>();
			completionSource.TrySetResult(result);
			return completionSource.Task;
		}

		public static Task<T> Run<T>(Func<T> func)
			=> Task<T>.Factory.StartNew(func);

		public static Task Run(Action action)
			=> Task.Factory.StartNew(action);

		public static Task WhenAll(params Task[] tasks)
			=> Task.Factory.ContinueWhenAll(tasks, t => { });

		public static Task WhenAny(params Task[] tasks)
			=> Task.Factory.ContinueWhenAny(tasks, t => { });
	}
}