using System;
using System.Net;
using System.Threading.Tasks;

namespace ExperimentalFeatures.Async
{
	public static class AsyncExtensions
	{
		public enum TransferStage
		{
			Uploading,
			Downloading
		}

		public static Task<byte[]> DownloadDataTaskAsync(this WebClient wc, Uri uri, object userToken)
		{
			var taskCompletionSource = new TaskCompletionSource<byte[]>();
			wc.DownloadDataCompleted += (sender, e) =>
			{
				if (e.Cancelled) taskCompletionSource.TrySetCanceled();
				else if (e.Error != null) taskCompletionSource.TrySetException(e.Error);
				else taskCompletionSource.TrySetResult(e.Result);
			};
			wc.DownloadDataAsync(uri, userToken);
			return taskCompletionSource.Task;
		}

		public static Task<byte[]> DownloadDataTaskAsync(this WebClient wc, Uri uri,
			AsyncProgress<TransferProgress> progress, object userToken)
		{
			var taskCompletionSource = new TaskCompletionSource<byte[]>();
			wc.DownloadProgressChanged += (sender, e) =>
			{
				progress.InvokeAsync(new TransferProgress
				{
					Stage = TransferStage.Downloading,
					Transferred = e.BytesReceived,
					Total = e.TotalBytesToReceive,
					UserState = userToken
				});
			};
			wc.DownloadDataCompleted += (sender, e) =>
			{
				if (e.Cancelled) taskCompletionSource.TrySetCanceled();
				else if (e.Error != null) taskCompletionSource.TrySetException(e.Error);
				else taskCompletionSource.TrySetResult(e.Result);
			};
			wc.DownloadDataAsync(uri, userToken);
			return taskCompletionSource.Task;
		}

		public static Task DownloadFileTaskAsync(this WebClient wc, Uri uri, string fileName, object userToken)
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			wc.DownloadFileCompleted += (sender, e) =>
			{
				if (e.Cancelled) taskCompletionSource.TrySetCanceled();
				else if (e.Error != null) taskCompletionSource.TrySetException(e.Error);
				else taskCompletionSource.TrySetResult(userToken);
			};
			wc.DownloadFileAsync(uri, fileName, userToken);
			return taskCompletionSource.Task;
		}

		public static Task DownloadFileTaskAsync(this WebClient wc, Uri uri, string fileName,
			AsyncProgress<TransferProgress> progress, object userToken)
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			wc.DownloadProgressChanged += (sender, e) =>
			{
				progress.InvokeAsync(new TransferProgress
				{
					Stage = TransferStage.Downloading,
					Transferred = e.BytesReceived,
					Total = e.TotalBytesToReceive,
					UserState = userToken
				});
			};
			wc.DownloadFileCompleted += (sender, e) =>
			{
				if (e.Cancelled) taskCompletionSource.TrySetCanceled();
				else if (e.Error != null) taskCompletionSource.TrySetException(e.Error);
				else taskCompletionSource.TrySetResult(userToken);
			};
			wc.DownloadFileAsync(uri, fileName, userToken);
			return taskCompletionSource.Task;
		}

		public static Task<byte[]> UploadDataTaskAsync(this WebClient wc, Uri uri, byte[] buffer,
			AsyncProgress<TransferProgress> progress, object userToken)
		{
			var taskCompletionSource = new TaskCompletionSource<byte[]>();
			wc.UploadProgressChanged += (sender, e) =>
			{
				progress.InvokeAsync(new TransferProgress
				{
					Stage = TransferStage.Downloading,
					Transferred = e.BytesReceived,
					Total = e.TotalBytesToReceive,
					UserState = userToken
				});
			};
			wc.UploadDataCompleted += (sender, e) =>
			{
				if (e.Cancelled) taskCompletionSource.TrySetCanceled();
				else if (e.Error != null) taskCompletionSource.TrySetException(e.Error);
				else taskCompletionSource.TrySetResult(e.Result);
			};
			wc.UploadDataAsync(uri, "POST", buffer, userToken);
			return taskCompletionSource.Task;
		}

		public static Task<byte[]> UploadDataTaskAsync(this WebClient wc, Uri uri, byte[] buffer, object userToken)
		{
			var taskCompletionSource = new TaskCompletionSource<byte[]>();
			wc.UploadProgressChanged += (sender, e) => { };
			wc.UploadDataCompleted += (sender, e) =>
			{
				if (e.Cancelled) taskCompletionSource.TrySetCanceled();
				else if (e.Error != null) taskCompletionSource.TrySetException(e.Error);
				else taskCompletionSource.TrySetResult(e.Result);
			};
			wc.UploadDataAsync(uri, "POST", buffer, userToken);
			return taskCompletionSource.Task;
		}

		public class TransferProgress
		{
			public TransferStage Stage;

			public long Transferred { get; set; }

			public long Total { get; set; }

			public object UserState { get; set; }
		}
	}
}