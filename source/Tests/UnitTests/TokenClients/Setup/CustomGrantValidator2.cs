using IdentityServer.Core.Services;
using IdentityServer.Core.Validation;
using System.Threading.Tasks;

namespace IdentityServer.Tests.TokenClients
{
    public class CustomGrantValidator2 : ICustomGrantValidator
    {
        public Task<CustomGrantValidationResult> ValidateAsync(ValidatedTokenRequest request)
        {
            var credential = request.Raw.Get("custom_credential");

            if (credential != null)
            {
                // valid credential
                return Task.FromResult(new CustomGrantValidationResult("818727", "custom"));
            }
            else
            {
                // custom error message
                return Task.FromResult(new CustomGrantValidationResult("invalid custom credential"));
            }
        }

        public string GrantType
        {
            get { return "custom2"; }
        }
    }
}