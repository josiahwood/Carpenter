using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StableHordeApi;

namespace CarpenterApi
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> log)
        {
            _logger = log;
        }

        [FunctionName("Function1")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "name", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Name** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ClaimsPrincipal claimsPrincipal)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            HttpClient httpClient = new HttpClient();
            StableHordeApi.Client client = new Client(httpClient);
            
            client.BaseUrl = "https://stablehorde.net/api";

            GenerationInputKobold payload = new GenerationInputKobold();
            payload.Prompt = name;
            payload.Params = new ModelGenerationInputKobold();
            payload.Params.N = 1;
            payload.Params.Max_context_length = 1024;
            payload.Params.Max_length = 256;

            var generateResult = await client.Post_text_async_generateAsync("***REMOVED***", null, payload, null);
            string id = generateResult.Id;

            var statusResult = await client.Get_text_async_statusAsync(null, null, id);

            while(statusResult.Done != true)
            {
                await Task.Delay(1000);
                statusResult = await client.Get_text_async_statusAsync(null, null, id);
            }

            string identity = "no ClaimsPrincipal";

            //claimsPrincipal = req.HttpContext.User;

            if (claimsPrincipal != null)
            {
                identity = "no Identity";

                if (claimsPrincipal.Identity != null)
                {
                    identity = "no Name or Claims";

                    if (claimsPrincipal.Identity.Name != null)
                    {
                        identity = claimsPrincipal.Identity.Name;
                    }
                    else if (claimsPrincipal.Claims.Count() > 0)
                    {
                        identity = "";

                        foreach (var claim in claimsPrincipal.Claims)
                        {
                            string properties = "";

                            foreach(var property in claim.Properties)
                            {
                                properties += property.Key + ":" + property.Value + ",";
                            }

                            string subject = "";

                            if(claim.Subject != null)
                            {
                                subject = $"{claim.Subject.Label}:{claim.Subject.Name}:{claim.Subject.NameClaimType}:{claim.Subject.RoleClaimType}";
                            }

                            identity += $"Issuer: {claim.Issuer}, Subject: {subject}, Value: {claim.Value}, ValueType: {claim.ValueType}, Type: {claim.Type}, Properties: {properties};";
                        }
                    }
                }
            }

            return new OkObjectResult(new object[] { statusResult, identity });
            //return new OkObjectResult(responseMessage);
        }
    }
}

