using System.Net;
using System.Text;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
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
    
    public async Task<TResource> GetResource<TResource>(string patientId) where TResource : Resource
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
        
        using var fhirClient = new FhirClient(endpoint, httpclient, settings);
        
        try
        {
            // Read a Patient resource (use an existing Patient ID from the server)
            //string patientId = "example"; // Replace with an actual Patient ID
             var response = await fhirClient.OperationAsync(new Uri(endpoint), useGet: true);

             if (response is TResource resource)
             {
                 return resource;
             }
            
             throw new InvalidOperationException($"Invalid Resource Type (Request: {typeof(TResource)} - Response: {response.GetType()})");
        }
        catch (FhirOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            var operationOutcome = new OperationOutcome
            {
                Issue = new List<OperationOutcome.IssueComponent>
                {
                    new()
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Code = OperationOutcome.IssueType.Exception,
                        Diagnostics = ex.Message
                    }
                }
            };

            throw new FhirOperationException("FHIR Operation Error", HttpStatusCode.InternalServerError,
                operationOutcome);
        }
    }
    
    public async Task<Resource> UpdatePatient(string patientId)
    {
        var settings = new FhirClientSettings
        {
            PreferredFormat = ResourceFormat.Json,
            VerifyFhirVersion = false, // avoids calling /metadata on every request
            PreferredParameterHandling = SearchParameterHandling.Lenient
        };
        
        var httpclient = _fhirHttpClientFactory.CreateClient("FhirHttpClient");
        httpclient.DefaultRequestHeaders.Add("x-random-header", Guid.NewGuid().ToString());
        httpclient.DefaultRequestHeaders.Add("x-method-name", "update patient");
        var endpoint = $"https://hapi.fhir.org/baseR4/";
        
        using var fhirClient = new FhirClient(endpoint, httpclient, settings);
        
        try
        {
            var patient = await GetResource<Patient>(patientId);
            
            patient.Name = new List<HumanName>
            {
                new HumanName
                {
                    Family = "Nguyen Super 123123",
                    Given = new[] { "Test test Long" }
                }
            };
            
            var parameters = new Parameters();
            parameters.Add("resource", patient);
            
            // Update the Patient resource
            var updatedPatient = await fhirClient.OperationAsync(new Uri("https://hapi.fhir.org/baseR4/Patient/45069285"), parameters);
            
            return updatedPatient;
        }
        catch (FhirOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
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
        var endpoint = $"https://hapi.fhir.org/baseR4/Patient";
        
        //var fhirClient = new FhirClient(endpoint, httpclient, settings);
        
        // Create a new Patient resource
        var newPatient = new Patient
        {
            Name = new List<HumanName>
            {
                new HumanName
                {
                    Family = "Nguyen 123",
                    Given = new[] { "Long 456", "yoyo" }
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
            // // Post the Patient resource to the server
            // var createdPatient = await fhirClient.CreateAsync(newPatient);
            //
            // // var createdPatient = await fhirClient.OperationAsync(new Uri($"{endpoint}/Patient/$validate"), newPatient as Parameters, useGet: false);
            //
            // // Print the ID of the created Patient
            // Console.WriteLine($"Created Patient ID: {createdPatient.Id}");
            //
            // return createdPatient;
            
            var resourcePayload = new FhirJsonSerializer().SerializeToString(newPatient);
            var content = new StringContent(resourcePayload, Encoding.UTF8, "application/fhir+json");

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            var response = await httpclient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var consent = await new FhirJsonParser().ParseAsync<Consent>(responseContent);
                var resource = await new FhirJsonParser().ParseAsync<Patient>(responseContent);
                return resource;
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var operationOutcome = await  new FhirJsonParser().ParseAsync<OperationOutcome>(responseContent);
                throw new FhirOperationException("FHIR Operation Error", response.StatusCode, operationOutcome);
            }
        }
        catch (FhirOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return new Patient();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            var operationOutcome = new OperationOutcome
            {
                Issue = new List<OperationOutcome.IssueComponent>
                {
                    new()
                    {
                        Severity = OperationOutcome.IssueSeverity.Error,
                        Code = OperationOutcome.IssueType.Exception,
                        Diagnostics = ex.Message
                    }
                }
            };

            throw new FhirOperationException("FHIR Operation Error", HttpStatusCode.InternalServerError,
                operationOutcome);
        }
    }
}