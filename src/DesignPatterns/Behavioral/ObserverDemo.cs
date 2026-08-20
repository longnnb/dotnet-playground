namespace DesignPatterns.Behavioral;

/// <summary>
/// Observer: lets one or more observers be notified of changes in a subject without the
/// subject needing to know anything about them. Shown two ways: the BCL's
/// <see cref="IObservable{T}"/>/<see cref="IObserver{T}"/> pair, and the more common C#
/// <c>event</c> keyword, which is a lighter-weight built-in take on the same idea.
/// </summary>
public static class ObserverDemo
{
    /// <summary>Runs both observer styles over the same sequence of price changes.</summary>
    public static Task RunAsync()
    {
        Console.WriteLine("-- IObservable<T> / IObserver<T> --");
        var priceFeed = new PriceFeed();
        using (priceFeed.Subscribe(new ConsoleObserver()))
        {
            priceFeed.Publish(101.5m);
            priceFeed.Publish(99.25m);
        }

        Console.WriteLine("-- event --");
        var ticker = new PriceTicker();
        ticker.PriceChanged += price => Console.WriteLine($"event: price changed to {price}");
        ticker.SetPrice(101.5m);
        ticker.SetPrice(99.25m);

        return Task.CompletedTask;
    }
}

file sealed class PriceFeed : IObservable<decimal>
{
    private readonly List<IObserver<decimal>> _observers = [];

    public IDisposable Subscribe(IObserver<decimal> observer)
    {
        _observers.Add(observer);
        return new Unsubscriber(_observers, observer);
    }

    public void Publish(decimal price)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(price);
        }
    }

    private sealed class Unsubscriber(List<IObserver<decimal>> observers, IObserver<decimal> observer) : IDisposable
    {
        public void Dispose() => observers.Remove(observer);
    }
}

file sealed class ConsoleObserver : IObserver<decimal>
{
    public void OnNext(decimal value) => Console.WriteLine($"IObserver: price changed to {value}");

    public void OnError(Exception error) => Console.WriteLine($"IObserver: error {error.Message}");

    public void OnCompleted() => Console.WriteLine("IObserver: feed completed");
}

file sealed class PriceTicker
{
    public event Action<decimal>? PriceChanged;

    public void SetPrice(decimal price) => PriceChanged?.Invoke(price);
}

/* Expected output
-- IObservable<T> / IObserver<T> --
IObserver: price changed to 101.5
IObserver: price changed to 99.25
-- event --
event: price changed to 101.5
event: price changed to 99.25
*/
