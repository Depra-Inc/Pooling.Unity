using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Depra.Pooling
{
	internal static class AsyncInstantiateOperationExtensions
	{
		public static Task<T[]> AsTask<T>(this AsyncInstantiateOperation<T> self,
			CancellationToken cancellationToken = default) where T : UnityEngine.Object
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<T[]>(cancellationToken);
			}

			return self.isDone
				? Task.FromResult(self.Result)
				: self.AwaitWithProgress(cancellationToken);
		}

		private static async Task<T[]> AwaitWithProgress<T>(this AsyncInstantiateOperation<T> self,
			CancellationToken cancellationToken = default) where T : UnityEngine.Object
		{
			while (!self.isDone)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return await Task.FromCanceled<T[]>(cancellationToken);
				}

				await Task.Yield();
			}

			return self.Result;
		}
	}
}