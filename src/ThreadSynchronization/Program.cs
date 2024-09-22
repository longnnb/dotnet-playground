internal partial class Program
{
    private static void Main(string[] args)
    {
        //var lockExample = new LockExample();

        //for (int i = 0; i < 5; i++)
        //{
        //    Thread t = new Thread(lockExample.DoWorkWithLock);
        //    t.Start();
        //}

        //var resetEventExample = new ResetEventExample();

        //new Thread(resetEventExample.WriteWithManualResetEvent).Start();

        //for (int i = 0; i < 5; i++)
        //{
        //    Thread t = new Thread(resetEventExample.ReadWithManualResetEvent);
        //    t.Start();
        //}

        //for (int i = 0; i < 5; i++)
        //{
        //    Thread t = new Thread(resetEventExample.WriteWithAutoResetEvent);
        //    t.Start();
        //}

        //var mutexExample = new MutexExample();

        //for (int i = 0; i < 5; i++)
        //{
        //    Thread t = new Thread(mutexExample.DoWorkWithMutex);
        //    t.Start();
        //}

        var semaphoreExample = new SemaphoreExample();

        for (var i = 0; i < 5; i++)
        {
            var t = new Thread(semaphoreExample.DoWorkWithSemaphore);
            t.Start();
        }
    }
}