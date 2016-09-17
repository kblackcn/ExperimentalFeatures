using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ExperimentalFeatures.Async;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExperimentalFeatures.Tests
{
	[TestClass]
	public class AsynchronousTests
	{
		public const string Test1 = "I'm back!";

		public const string Test2 = "+1s!";

		[TestMethod]
		[ExpectedException(typeof (AggregateException))]
		public void TestEmpty()
		{
			new NoneAsyncMachine().Start().Task.Wait();
		}

		[TestMethod]
		public void TestSync()
		{
			Assert.IsTrue(new SyncAsyncMachine().Start().Task.Result == Test1);
		}

		[TestMethod]
		public void TestNest()
		{
			Assert.IsTrue(new NestAsyncMachine().Start().Task.Result == Test1 + Test2);
		}

		[TestMethod]
		public void TestDownloadData()
		{
			Assert.IsTrue(new DownloadDataAsyncMachine().Start().Task.Result.Contains("baidu"));
		}

		[TestMethod]
		public void TestWrapped()
		{
			Assert.IsTrue(TaskEx.Wrap<bool>(TestWrappedInternal).Result);
		}

		private IEnumerable<Task> TestWrappedInternal(WrappedAsyncStateMachine<bool> machine)
		{
			yield return new NestAsyncMachine().Start().Task;
			machine.SetResult(machine.GetLastResult<string>() == Test1 + Test2);
		}

		public class NoneAsyncMachine : AsyncStateMachine<bool>
		{
			protected override IEnumerable<Task> Run()
			{
				yield return null;
			}
		}

		public class SyncAsyncMachine : AsyncStateMachine<string>
		{
			protected override IEnumerable<Task> Run()
			{
				SetResult(Test1);
				yield break;
			}
		}

		public class NestAsyncMachine : AsyncStateMachine<string>
		{
			protected override IEnumerable<Task> Run()
			{
				var child = new SyncAsyncMachine().Start();
				yield return child.Task;
				SetResult(GetLastResult<string>() + Test2);
			}
		}


		public class DownloadDataAsyncMachine : AsyncStateMachine<string>
		{
			protected override IEnumerable<Task> Run()
			{
				using (var client = new WebClient())
				{
					yield return client.DownloadDataTaskAsync(new Uri("https://www.baidu.com/"), null);
					SetResult(Encoding.UTF8.GetString(GetLastResult<byte[]>()));
				}
			}
		}
	}
}