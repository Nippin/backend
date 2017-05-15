using System;
using System.Threading.Tasks;

namespace Endpoint
{

    public static class TaskExtensions
    {
        public static Task<T> WithTimeout<T>(this Task<T> task, int seconds)
        {
            var tcs = new TaskCompletionSource<T>();
            Task
                .WhenAny(task, Task.Delay(TimeSpan.FromSeconds(seconds)))
                .Unwrap()
                .ContinueWith(it =>
                {
                    if (task.IsCompleted)
                        tcs.SetResult(task.Result);
                    else if (task.IsFaulted)
                        tcs.SetException(task.Exception);
                    else
                        tcs.SetCanceled();
                });

            return tcs.Task;
        }
    }
}
