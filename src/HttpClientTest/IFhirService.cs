using Hl7.Fhir.Model;

namespace HttpClientTest;

/// <summary>Talks to a FHIR server for a small set of representative operations.</summary>
public interface IFhirService
{
    /// <summary>Searches for up to <paramref name="count"/> patients.</summary>
    Task<Bundle> SearchPatientsAsync(int count = 5, CancellationToken cancellationToken = default);

    /// <summary>Reads a single resource of type <typeparamref name="TResource"/> by id.</summary>
    Task<TResource> ReadResourceAsync<TResource>(string id, CancellationToken cancellationToken = default)
        where TResource : Resource;

    /// <summary>Creates a new patient with a fixed set of demo data.</summary>
    Task<Patient> CreateNewPatientAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads the patient identified by <paramref name="id"/> and updates their name.</summary>
    Task<Patient> UpdatePatientAsync(string id, CancellationToken cancellationToken = default);
}
