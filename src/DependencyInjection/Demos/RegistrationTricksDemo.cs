using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DependencyInjection.Demos;

/// <summary>
/// Demonstrates three registration behaviors worth knowing: resolving every registration of an
/// interface as <see cref="IEnumerable{T}"/>, <c>TryAdd*</c> as a no-op-if-already-registered
/// guard (how a library can offer a default without overriding the host application's own
/// registration), and <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>
/// for constructing a type that mixes container-resolved and manually-supplied constructor
/// arguments.
/// </summary>
public static class RegistrationTricksDemo
{
    /// <summary>Runs the demo.</summary>
    public static Task RunAsync()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidationRule, NotEmptyRule>();
        services.AddSingleton<IValidationRule, MaxLengthRule>();

        // A no-op: IValidationRule already has registrations above, so TryAddSingleton skips
        // this one. This is how a library offers a default implementation without clobbering
        // whatever the host application already registered.
        services.TryAddSingleton<IValidationRule, NotEmptyRule>();

        using var provider = services.BuildServiceProvider();

        var rules = provider.GetRequiredService<IEnumerable<IValidationRule>>();
        Console.WriteLine($"Registered rules: {string.Join(", ", rules.Select(r => r.Describe()))}");

        // FieldValidator isn't registered in the container at all. ActivatorUtilities resolves
        // what it can from the container (IEnumerable<IValidationRule>) and takes the rest
        // (the field name) as an explicit argument.
        var validator = ActivatorUtilities.CreateInstance<FieldValidator>(provider, "Username");
        Console.WriteLine(validator.Validate(string.Empty));
        Console.WriteLine(validator.Validate("a-perfectly-reasonable-name-that-is-nonetheless-too-long"));
        Console.WriteLine(validator.Validate("ok"));

        return Task.CompletedTask;
    }
}

file interface IValidationRule
{
    string Describe();

    bool IsValid(string value, out string error);
}

file sealed class NotEmptyRule : IValidationRule
{
    public string Describe() => nameof(NotEmptyRule);

    public bool IsValid(string value, out string error)
    {
        error = "must not be empty";
        return !string.IsNullOrEmpty(value);
    }
}

file sealed class MaxLengthRule : IValidationRule
{
    private const int MaxLength = 20;

    public string Describe() => nameof(MaxLengthRule);

    public bool IsValid(string value, out string error)
    {
        error = $"must be {MaxLength} characters or fewer";
        return value.Length <= MaxLength;
    }
}

/// <summary>Not registered in the container -- built on demand via <see cref="ActivatorUtilities.CreateInstance{T}(IServiceProvider, object[])"/>.</summary>
file sealed class FieldValidator(IEnumerable<IValidationRule> rules, string fieldName)
{
    public string Validate(string value)
    {
        foreach (var rule in rules)
        {
            if (!rule.IsValid(value, out var error))
            {
                return $"{fieldName}: invalid -- {error}";
            }
        }

        return $"{fieldName}: valid";
    }
}

/* Expected output
Registered rules: NotEmptyRule, MaxLengthRule
Username: invalid -- must not be empty
Username: invalid -- must be 20 characters or fewer
Username: valid
*/
