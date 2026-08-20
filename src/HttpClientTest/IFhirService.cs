using Hl7.Fhir.Model;

namespace HttpClientTest;

public interface IFhirService
{
    Task<Bundle> GetPatients();
    Task<TResource> GetResource<TResource>(string patientId) where TResource : Resource;
    Task<Patient> CreatePatient();

    Task<Resource> UpdatePatient(string patientId);
}