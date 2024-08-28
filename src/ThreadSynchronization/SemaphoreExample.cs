internal partial class Program
{
    public class SemaphoreExample
    {
        // SemaphoreSlim is a lightweight alternative to Semaphore.
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        /*
         * initialCount: it is the number of times Wait can be called without blocking immediately after the semaphore was instantiated.
         * maximumCount: is the highest count the semaphore can obtain. It is the number of times Release can be called without throwing an exception assuming initialCount count was zero
         * release will increment the semaphore counter
         * wait will decrement the semaphore counter
         * If initialCount is set to the same value as maximumCount then calling Release immediately after the semaphore was instantiated will throw an exception.
         * You can't increment the counter ("CurrentCount" property) greater than maximum count which you set in initialization.
        */

        public void DoWorkWithSemaphore()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} waiting for semaphore signal...");
            semaphore.Wait();
            try
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Working...");
                Thread.Sleep(2000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Work done.");
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task DoWorkWithSemaphoreAsync()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} waiting for semaphore signal...");
            await semaphore.WaitAsync();
            try
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Working...");
                await Task.Delay(2000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Work done.");
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}