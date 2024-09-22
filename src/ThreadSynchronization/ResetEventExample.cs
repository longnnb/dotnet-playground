internal partial class Program
{
    public class ResetEventExample
    {
        private readonly AutoResetEvent autoResetEvent = new(true);
        private readonly ManualResetEvent manualResetEvent = new(false);

        public void WriteWithManualResetEvent()
        {
            manualResetEvent.Reset(); // state = false
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} writing...");
            Thread.Sleep(1000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} writing completed.");
            manualResetEvent.Set(); // state = true
        }

        public void ReadWithManualResetEvent()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} waiting for reading...");
            manualResetEvent.WaitOne(); // wait until state = true
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} reading...");
            Thread.Sleep(500);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} reading completed.");
        }

        public void WriteWithAutoResetEvent()
        {
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} waiting for writing...");
            autoResetEvent.WaitOne(); // auto reset to false when acquired by a thread
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} writing...");
            Thread.Sleep(1000);
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} writing completed.");
            autoResetEvent.Set(); // state = true
            // Note: resetevent can be set from any thread, this can cause a problem, should use mutex instead
        }
    }
}