using System.Globalization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Task = System.Threading.Tasks.Task; // Hl7.Fhir.Model has its own Task resource type

namespace HttpClientTest.Demos;

/// <summary>
/// Demonstrates <see cref="IFhirService"/> against the public hapi.fhir.org test server (see
/// <see cref="FhirService"/>'s remarks). Search and read are safe and run by default; create
/// and update are opt-in only -- see <c>Program.cs</c>'s default demo set -- since they write
/// to a server other people share, and there's no reason to do that on every routine "does this
/// still build and run" check.
/// </summary>
public static class FhirDemos
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    /// <summary>Searches for a handful of patients and prints how many were found.</summary>
    public static Task RunSearchAsync(IServiceProvider services) => RunSafelyAsync(services, async (fhirService, ct) =>
    {
        var bundle = await fhirService.SearchPatientsAsync(count: 5, ct);
        var totalText = bundle.Total is { } total ? total.ToString(CultureInfo.InvariantCulture) : "not reported";
        Console.WriteLine($"Found {bundle.Entry.Count} patient(s) (server-reported total: {totalText}).");
    });

    /// <summary>Searches for one patient, then reads it back by id -- avoids depending on a hardcoded id that may no longer exist on the shared server.</summary>
    public static Task RunReadAsync(IServiceProvider services) => RunSafelyAsync(services, async (fhirService, ct) =>
    {
        var bundle = await fhirService.SearchPatientsAsync(count: 1, ct);
        var id = bundle.Entry.FirstOrDefault()?.Resource?.Id;

        if (id is null)
        {
            Console.WriteLine("No patients found to read back.");
            return;
        }

        var patient = await fhirService.ReadResourceAsync<Patient>(id, ct);
        Console.WriteLine(new FhirJsonSerializer().SerializeToString(patient));
    });

    /// <summary>Creates a new patient on the shared server. Opt-in only -- see the type-level remarks.</summary>
    public static Task RunCreateAsync(IServiceProvider services) => RunSafelyAsync(services, async (fhirService, ct) =>
    {
        var created = await fhirService.CreateNewPatientAsync(ct);
        Console.WriteLine($"Created patient {created.Id}");
        Console.WriteLine(new FhirJsonSerializer().SerializeToString(created));
    });

    /// <summary>Creates a patient and then updates it -- never mutates anyone else's data. Opt-in only -- see the type-level remarks.</summary>
    public static Task RunUpdateAsync(IServiceProvider services) => RunSafelyAsync(services, async (fhirService, ct) =>
    {
        var created = await fhirService.CreateNewPatientAsync(ct);
        var updated = await fhirService.UpdatePatientAsync(created.Id, ct);
        Console.WriteLine($"Updated patient {updated.Id}, new family name: {updated.Name.FirstOrDefault()?.Family}");
    });

    private static async Task RunSafelyAsync(IServiceProvider services, Func<IFhirService, CancellationToken, Task> action)
    {
        var fhirService = services.GetRequiredService<IFhirService>();
        using var cts = new CancellationTokenSource(Timeout);

        try
        {
            await action(fhirService, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("hapi.fhir.org did not respond within 30s -- skipping.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"hapi.fhir.org call failed -- skipping. ({ex.GetType().Name}: {ex.Message})");
        }
    }
}
