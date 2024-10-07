using Hl7.Fhir.Model;

namespace HttpClientTest;

public interface IFhirService
{
    Task<Bundle> GetPatients();
    Task<Patient> GetPatient(string id);
    Task<Patient> CreatePatient();
}