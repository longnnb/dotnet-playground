using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace HttpClientTest;

/// <summary>
/// Talks to the public FHIR test server at hapi.fhir.org using the Firely .NET SDK's
/// <see cref="FhirClient"/>. <see cref="CreateClient"/> builds one <see cref="FhirClient"/> per
/// call instead of the <see cref="FhirClientSettings"/> block that used to be duplicated
/// verbatim in all four methods here, and the two cross-cutting request headers this project
/// used to set by hand in every method now live in <see cref="Handlers.FhirHeadersHandler"/>.
/// </summary>
/// <remarks>
/// hapi.fhir.org is a shared public test server maintained by the FHIR community: reads are
/// always safe, and writes (<see cref="CreateNewPatientAsync"/>, <see cref="UpdatePatientAsync"/>)
/// are expected and accepted there -- but they add data other people can see, so the demo
/// dispatcher treats them as opt-in rather than part of the default run (see
/// <see cref="Demos.FhirDemos"/>).
/// </remarks>
public class FhirService(IHttpClientFactory httpClientFactory) : IFhirService
{
    private const string BaseUrl = "https://hapi.fhir.org/baseR4";

    private FhirClient CreateClient()
    {
        var settings = new FhirClientSettings
        {
            PreferredFormat = ResourceFormat.Json,
            VerifyFhirVersion = false, // avoids calling /metadata on every request
            PreferredParameterHandling = SearchParameterHandling.Lenient,
        };

        var httpClient = httpClientFactory.CreateClient("FhirHttpClient");
        return new FhirClient(BaseUrl, httpClient, settings);
    }

    /// <inheritdoc />
    public async Task<Bundle> SearchPatientsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        using var fhirClient = CreateClient();
        var searchParams = new SearchParams { Count = count };

        var bundle = await fhirClient.SearchAsync<Patient>(searchParams, ct: cancellationToken);
        return bundle ?? throw new InvalidOperationException("Search returned no bundle.");
    }

    /// <inheritdoc />
    public async Task<TResource> ReadResourceAsync<TResource>(string id, CancellationToken cancellationToken = default)
        where TResource : Resource
    {
        using var fhirClient = CreateClient();

        try
        {
            var resource = await fhirClient.ReadAsync<TResource>($"{typeof(TResource).Name}/{id}", ct: cancellationToken);
            return resource ?? throw new InvalidOperationException($"Read returned no {typeof(TResource).Name}.");
        }
        catch (FhirOperationException ex)
        {
            Console.WriteLine($"FHIR error reading {typeof(TResource).Name}/{id}: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Patient> CreateNewPatientAsync(CancellationToken cancellationToken = default)
    {
        using var fhirClient = CreateClient();

        // A unique identifier per call, not just fixed demo data: hapi.fhir.org detects and
        // rejects a Create whose content duplicates an existing resource, so two calls to this
        // method with identical data back to back (as the fhir-create and fhir-update demos do
        // in the same run) would otherwise fail the second one with HAPI-2840.
        var newPatient = new Patient
        {
            Identifier = [new Identifier("https://dotnet-playground.local/demo-patients", Guid.NewGuid().ToString())],
            Name = [new HumanName { Family = "Nguyen", Given = ["Long"] }],
            Gender = AdministrativeGender.Male,
            BirthDate = "1980-01-01",
            Address =
            [
                new Address
                {
                    Line = ["123 Main St"],
                    City = "Somewhere",
                    State = "NY",
                    PostalCode = "12345",
                    Country = "USA",
                },
            ],
        };

        var created = await fhirClient.CreateAsync(newPatient, ct: cancellationToken);
        return created ?? throw new InvalidOperationException("Create returned no Patient.");
    }

    /// <inheritdoc />
    public async Task<Patient> UpdatePatientAsync(string id, CancellationToken cancellationToken = default)
    {
        using var fhirClient = CreateClient();

        var patient = await fhirClient.ReadAsync<Patient>($"Patient/{id}", ct: cancellationToken)
            ?? throw new InvalidOperationException($"Read returned no Patient/{id}.");
        patient.Name = [new HumanName { Family = "Nguyen-Updated", Given = ["Long"] }];

        // UpdateAsync builds the request URL from the resource's own Id -- unlike an earlier
        // version of this method, which fetched `id` but then always PUT to a hardcoded
        // ".../Patient/45069285", silently updating the wrong record whenever the argument
        // wasn't exactly that one id.
        var updated = await fhirClient.UpdateAsync(patient, ct: cancellationToken);
        return updated ?? throw new InvalidOperationException($"Update returned no Patient/{id}.");
    }
}
