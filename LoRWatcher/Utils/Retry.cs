using System;
using System.Threading.Tasks;

namespace LoRWatcher.Utils
{
    public class Retry
    {
        private static int NumOfRetries = 5;

        private static int WaitTimeMS = 1000;

        /// <summary>
        /// Retries execution of an async lamda function.
        /// </summary>
        /// <param name="func">A function that should return a non null object if no more retires need to occur; else null to carry on retrying.</param>
        /// <returns>A task.</returns>
        public static async Task<T> InvokeAsync<T>(Func<Task<T>> func)
            where T : class
        {
            var retryCount = 0;
            while (retryCount < NumOfRetries)
            {
                var result = await func();
                if (result != null)
                {
                    return result;
                }

                await Task.Delay(WaitTimeMS);

                retryCount++;
            }

            return null;
        }

        /// <summary>
        /// Retries execution of an async lamda function.
        /// </summary>
        /// <param name="func">A function that should return true if no more retires need to occur; else false to carry on retrying.</param>
        /// <returns>A task.</returns>
        public static async Task<bool> InvokeAsync(Func<Task<bool>> func)
        {
            var retryCount = 0;
            while (retryCount < NumOfRetries)
            {
                var result = await func();
                if (result == true)
                {
                    return result;
                }

                await Task.Delay(WaitTimeMS);

                retryCount++;
            }

            return false;
        }

        /// <summary>
        /// Retries execution of a lamda function.
        /// </summary>
        /// <param name="func">A function that should return true if no more retires need to occur; else false to carry on retrying.</param>
        /// <returns>True if successful; else false.</returns>
        public static bool Invoke(Func<bool> func)
        {
            var retryCount = 0;
            while (retryCount < NumOfRetries)
            {
                var result = func();
                if (result == true)
                {
                    return result;
                }

                Task.Delay(WaitTimeMS);

                retryCount++;
            }

            return false;
        }
    }
}
