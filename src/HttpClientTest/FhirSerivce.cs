using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Task = System.Threading.Tasks.Task;

namespace HttpClientTest;

public class FhirService : IFhirService
{
    IHttpClientFactory _fhirHttpClientFactory;
    
    public FhirService(IHttpClientFactory fhirHttpClientFactory)
    {
        _fhirHttpClientFactory = fhirHttpClientFactory;
    }
    
    public async Task<Bundle> GetPatients()
    {
        var settings = new FhirClientSettings
        {
            PreferredFormat = ResourceFormat.Json,
            VerifyFhirVersion = false, // avoids calling /metadata on every request
            PreferredParameterHandling = SearchParameterHandling.Lenient
        };
        
        var httpclient = _fhirHttpClientFactory.CreateClient("FhirHttpClient");
        httpclient.DefaultRequestHeaders.Add("x-random-header", Guid.NewGuid().ToString());
        httpclient.DefaultRequestHeaders.Add("x-method-name", "get patients");
        
        var endpoint = "https://hapi.fhir.org/baseR4/Patient";
        var fhirClient = new FhirClient(endpoint, httpclient, settings);
        
        var response = await fhirClient.OperationAsync(new Uri(endpoint), useGet: true);
        
        return response as Bundle;
    }
    
    public async Task<Patient> GetPatient(string patientId)
    {
        var settings = new FhirClientSettings
        {
            PreferredFormat = ResourceFormat.Json,
            VerifyFhirVersion = false, // avoids calling /metadata on every request
            PreferredParameterHandling = SearchParameterHandling.Lenient
        };
        
        var httpclient = _fhirHttpClientFactory.CreateClient("FhirHttpClient");
        httpclient.DefaultRequestHeaders.Add("x-random-header", Guid.NewGuid().ToString());
        httpclient.DefaultRequestHeaders.Add("x-method-name", "get patient");
        var endpoint = $"https://hapi.fhir.org/baseR4/Patient/{patientId}";
        
        var fhirClient = new FhirClient(endpoint, httpclient, settings);
        
        try
        {
            // Read a Patient resource (use an existing Patient ID from the server)
            //string patientId = "example"; // Replace with an actual Patient ID
            var response = await fhirClient.OperationAsync(new Uri(endpoint), useGet: true);

            return response as Patient;
            // Print the Patient's name
            // Console.WriteLine($"Patient Name: {patient.Name[0].ToString()}");
        }
        catch (FhirOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new Patient();
        }
    }

    public async Task<Patient> CreatePatient()
    {
        var settings = new FhirClientSettings
        {
            PreferredFormat = ResourceFormat.Json,
            VerifyFhirVersion = false, // avoids calling /metadata on every request
            PreferredParameterHandling = SearchParameterHandling.Lenient
        };
        
        var httpclient = _fhirHttpClientFactory.CreateClient("FhirHttpClient");
        httpclient.DefaultRequestHeaders.Add("x-random-header", Guid.NewGuid().ToString());
        httpclient.DefaultRequestHeaders.Add("x-method-name", "get patient");
        var endpoint = $"https://hapi.fhir.org/baseR4/";
        
        var fhirClient = new FhirClient(endpoint, httpclient, settings);
        
        // Create a new Patient resource
        var newPatient = new Patient
        {
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Family = "Nguyen",
                    Given = new[] { "Long" }
                }
            },
            Gender = AdministrativeGender.Male,
            BirthDate = "1980-01-01",
            Address = new List<Address>
            {
                new Address
                {
                    Line = new[] { "123 Main St" },
                    City = "Somewhere",
                    State = "NY",
                    PostalCode = "12345",
                    Country = "USA"
                }
            }
        };

        try
        {
            // Post the Patient resource to the server
            var createdPatient = await fhirClient.CreateAsync(newPatient);
            
            // var createdPatient = await fhirClient.OperationAsync(new Uri($"{endpoint}/Patient/$validate"), newPatient as Parameters, useGet: false);

            // Print the ID of the created Patient
            Console.WriteLine($"Created Patient ID: {createdPatient.Id}");
            
            return createdPatient;
        }
        catch (FhirOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new Patient();
        }
    }
}