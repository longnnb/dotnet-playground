using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Demos;

/// <summary>
/// Demonstrates keyed services (.NET 8): registering multiple implementations of the same
/// interface distinguished by a key, and resolving a specific one either explicitly via
/// <see cref="ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService{T}"/> or
/// automatically via the <see cref="FromKeyedServicesAttribute"/> constructor parameter.
/// </summary>
public static class KeyedServicesDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<INotifier, EmailNotifier>("email");
        services.AddKeyedSingleton<INotifier, SmsNotifier>("sms");
        services.AddSingleton<NotificationDispatcher>();

        using var provider = services.BuildServiceProvider();

        var email = provider.GetRequiredKeyedService<INotifier>("email");
        var sms = provider.GetRequiredKeyedService<INotifier>("sms");
        Console.WriteLine($"Explicit keyed resolution: {email.Describe()}");
        Console.WriteLine($"Explicit keyed resolution: {sms.Describe()}");

        var dispatcher = provider.GetRequiredService<NotificationDispatcher>();
        dispatcher.NotifyUrgent("server is on fire");

        return Task.CompletedTask;
    }
}

file interface INotifier
{
    string Describe();

    void Send(string message);
}

file sealed class EmailNotifier : INotifier
{
    public string Describe() => nameof(EmailNotifier);

    public void Send(string message) => Console.WriteLine($"[email] {message}");
}

file sealed class SmsNotifier : INotifier
{
    public string Describe() => nameof(SmsNotifier);

    public void Send(string message) => Console.WriteLine($"[sms] {message}");
}

/// <summary>Gets its "urgent" notifier injected by key -- via <see cref="FromKeyedServicesAttribute"/> -- with no manual lookup in its own body.</summary>
file sealed class NotificationDispatcher([FromKeyedServices("sms")] INotifier urgentNotifier)
{
    public void NotifyUrgent(string message) => urgentNotifier.Send(message);
}

/* Expected output
Explicit keyed resolution: EmailNotifier
Explicit keyed resolution: SmsNotifier
[sms] server is on fire
*/
