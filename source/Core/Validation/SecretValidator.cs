using IdentityServer.Core.Extensions;
using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    internal class SecretValidator
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IEnumerable<ISecretValidator> _validators;

        public SecretValidator(IEnumerable<ISecretValidator> validators)
        {
            _validators = validators;
        }

        public async Task<SecretValidationResult> ValidateAsync(ParsedSecret parsedSecret, IEnumerable<Secret> secrets)
        {
            var expiredSecrets = secrets.Where(s => s.Expiration.HasExpired());
            if (expiredSecrets.Any())
            {
                expiredSecrets.ToList().ForEach(
                    ex => Logger.InfoFormat("Secret [{0}] is expired", ex.Description ?? "no description"));
            }

            var currentSecrets = secrets.Where(s => !s.Expiration.HasExpired());

            // see if a registered validator can validate the secret
            foreach (var validator in _validators)
            {
                var secretValidationResult = await validator.ValidateAsync(currentSecrets, parsedSecret);

                if (secretValidationResult.Success)
                {
                    Logger.DebugFormat("Secret validator success: {0}", validator.GetType().Name);
                    return secretValidationResult;
                }
            }

            Logger.Info("Secret validators could not validate secret");
            return new SecretValidationResult { Success = false };
        }
    }
}