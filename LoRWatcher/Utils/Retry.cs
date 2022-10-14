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
        /// <param name="func">An async function that should return a non null object if no more retires need to occur; else null to carry on retrying.</param>
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
        /// <param name="func">An async function that should return true if no more retires need to occur; else false to carry on retrying.</param>
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
        /// Retries execution of an lamda function.
        /// </summary>
        /// <param name="func">A function that should return true if no more retires need to occur; else false to carry on retrying.</param>
        /// <param name="waitTimeMs">An optional parameter to set the wait time between retries. Defaults to 1000 if null.</param>
        /// <param name="numOfRetries">An optional parameter to set the number of retries. Defaults to 5 if null.</param>
        /// <returns>A task.</returns>
        public static async Task<bool> InvokeAsync(Func<bool> func, int? waitTimeMs = null, int? numOfRetries = null)
        {
            numOfRetries = numOfRetries.HasValue ? numOfRetries.Value : NumOfRetries;
            waitTimeMs = waitTimeMs.HasValue ? waitTimeMs.Value : WaitTimeMS;

            var retryCount = 0;
            while (retryCount < numOfRetries.Value)
            {
                var result = func();
                if (result == true)
                {
                    return result;
                }

                await Task.Delay(waitTimeMs.Value);

                retryCount++;
            }

            return false;
        }

        /// <summary>
        /// Retries execution of an async lamda function.
        /// </summary>
        /// <param name="func">A function that should return a non null object if no more retires need to occur; else null to carry on retrying.</param>
        /// <returns>A task.</returns>
        public static T Invoke<T>(Func<T> func, int? waitTimeMS = null)
            where T : class
        {
            var retryCount = 0;
            while (retryCount < NumOfRetries)
            {
                var result = func();
                if (result != null)
                {
                    return result;
                }

                Task.Delay(waitTimeMS ?? WaitTimeMS).Wait();

                retryCount++;
            }

            return null;
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

                Task.Delay(WaitTimeMS).Wait();

                retryCount++;
            }

            return false;
        }
    }
}
