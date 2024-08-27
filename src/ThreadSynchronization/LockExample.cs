internal partial class Program
{
    public class LockExample
    {
        private readonly object lockObj = new object();

        public void DoWorkWithLock()
        {
            lock (lockObj)
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Working...");
                Thread.Sleep(1000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Work done.");
            }
        }

        // Monitor has Wait(), Pulse(), and PulseAll() methods which can be used to send signal to other threads
        public void DoWorkWithMonitor()
        {
            try
            {
                Monitor.Enter(lockObj);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Working...");
                Thread.Sleep(1000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Work done.");
            }
            finally
            {
                Monitor.Exit(lockObj);
            }
        }

        public void DoWorkWithTryEnter()
        {
            if (Monitor.TryEnter(lockObj))
            {
                try
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Working...");
                    Thread.Sleep(1000);
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Work done.");
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
            }
            else
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} Failed to acquire lock.");
            }
        }        
    }
}