using System.Threading;
using System.Threading.Tasks;

namespace AsyncLibraryDemo
{
    public static class TaskExtends
    {

        //this function make task will return as soon as possible.if the cancellationToken is canceled.
        public static async Task<T> UntilCompletionOrCancellation<T>(
         this Task<T> asyncOp, CancellationToken ct)
        {
            T result;
            var tcs = new TaskCompletionSource<T>();
            using (ct.Register(() => tcs.TrySetResult(default(T))))
            {
                result = await Task.WhenAny(asyncOp, tcs.Task).Unwrap();
            }
            return result;
        }



    }
}