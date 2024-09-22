internal partial class Program
{
    public class MutexExample
    {
        private readonly Mutex mutex = new(false, "MutexExample");

        public void DoWorkWithMutex()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} waiting for mutex signal...");
            if (mutex.WaitOne())
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Working...");
                Thread.Sleep(1000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Work done.");
                mutex.ReleaseMutex(); // can only be released by the thread that acquired it, this solves the problem of resetevent
            }
            else
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Failed to acquire lock.");
            }
        }
    }
}