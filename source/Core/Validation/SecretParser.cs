using IdentityServer.Core.Logging;
using IdentityServer.Core.Models;
using IdentityServer.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityServer.Core.Validation
{
    internal class SecretParser
    {
        private readonly static ILog Logger = LogProvider.GetCurrentClassLogger();

        private readonly IEnumerable<ISecretParser> _parsers;

        public SecretParser(IEnumerable<ISecretParser> parsers)
        {
            _parsers = parsers;
        }

        public async Task<ParsedSecret> ParseAsync(IDictionary<string, object> environment)
        {
            // see if a registered parser finds a secret on the request
            ParsedSecret parsedSecret = null;
            foreach (var parser in _parsers)
            {
                parsedSecret = await parser.ParseAsync(environment);
                if (parsedSecret != null)
                {
                    Logger.DebugFormat("Parser found secret: {0}", parser.GetType().Name);
                    Logger.InfoFormat("Secret id found: {0}", parsedSecret.Id);

                    return parsedSecret;
                }
            }

            Logger.InfoFormat("Parser found no secret");
            return null;
        }
    }
}