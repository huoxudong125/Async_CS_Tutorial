using System;
using System.Collections.Generic;
using System.Linq;
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

        public static async Task<T> RetryOnFault<T>(
            Func<Task<T>> function, int maxTries)
        {
            for (int i = 0; i < maxTries; i++)
            {
                try
                {
                    return await function().ConfigureAwait(false);
                }
                catch
                {
                    if (i == maxTries - 1) throw;
                }
            }
            return default(T);
        }

        /// <summary>
        ///     the task will not caputure the context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="functions"></param>
        /// <returns></returns>
        public static async Task<T> NeedOnlyOne<T>(
            params Func<CancellationToken, Task<T>>[] functions)
        {
            var cts = new CancellationTokenSource();
            Task<T>[] tasks = (from function in functions
                select function(cts.Token)).ToArray();
            T completed = await Task.WhenAny(tasks).Unwrap().ConfigureAwait(false);

            cts.Cancel();

            foreach (var task in tasks)
            {
                Task ignored = task.ContinueWith(
                    t => Console.WriteLine(t), TaskContinuationOptions.OnlyOnFaulted);
            }

            return completed;
        }

        public static IEnumerable<Task<T>> Interleaved<T>(IEnumerable<Task<T>> tasks)
        {
            List<Task<T>> inputTasks = tasks.ToList();
            List<TaskCompletionSource<T>> sources = (from _ in Enumerable.Range(0, inputTasks.Count)
                select new TaskCompletionSource<T>()).ToList();
            int nextTaskIndex = -1;
            foreach (var inputTask in inputTasks)
            {
                inputTask.ContinueWith(completed =>
                {
                    TaskCompletionSource<T> source = sources[Interlocked.Increment(ref nextTaskIndex)];
                    if (completed.IsFaulted)
                    {
                        if (completed.Exception != null)
                        {
                            source.TrySetException(completed.Exception.InnerExceptions);
                        }
                    }
                    else if (completed.IsCanceled)
                        source.TrySetCanceled();
                    else
                        source.TrySetResult(completed.Result);
                }, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
            return from source in sources
                select source.Task;
        }

        public static Task<T[]> WhenAllOrFirstException<T>(IEnumerable<Task<T>> tasks)
        {
            List<Task<T>> inputs = tasks.ToList();
            var ce = new CountdownEvent(inputs.Count);
            var tcs = new TaskCompletionSource<T[]>();

            Action<Task> onCompleted = (Task completed) =>
            {
                if (completed.IsFaulted)
                    if (completed.Exception != null)
                    {
                        tcs.TrySetException(completed.Exception.InnerExceptions);
                    }
                if (ce.Signal() && !tcs.Task.IsCompleted)
                    tcs.TrySetResult(inputs.Select(t => t.Result).ToArray());
            };

            foreach (var t in inputs) t.ContinueWith(onCompleted);
            return tcs.Task;
        }
    }
}