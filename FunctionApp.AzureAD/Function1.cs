using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Threading.Tasks;

namespace FunctionApp.AzureAD
{
    public  class Function1
    {
        private readonly ILogger<Function1> _logger;
        // The web API will only accept tokens 1) for users, and 2) having the "access_as_user" scope for this API	
        static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };
        static readonly string[] appRoleRequiredByApi = new string[] { "Read" };
        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }


        //https://github.com/AzureAD/microsoft-identity-web/wiki/Azure-Functions
        //https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/how-to-assign-app-role-managed-identity-powershell -- assign app role
        [FunctionName("Function1")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var (authenticationStatus, authenticationResponse) =
                await req.HttpContext.AuthenticateAzureFunctionAsync();
            if (!authenticationStatus) return authenticationResponse;

            //req.HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            req.HttpContext.ValidateAppRole(appRoleRequiredByApi);

            string name = req.HttpContext.User.Identity.IsAuthenticated ? req.HttpContext.User.GetDisplayName() : null;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new JsonResult(responseMessage);
        }
    }
}
