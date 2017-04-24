using FluentAssertions;
using IdentityServer.Core;
using IdentityServer.Core.Configuration;
using Xunit;

namespace IdentityServer.Tests.Configuration
{
    public class AuthenticationOptionsTests
    {
        [Fact]
        public void SigninMessageThreshold_Default_SameAsDefinedConstant()
        {
            new AuthenticationOptions()
                .SignInMessageThreshold
                .Should()
                .Be(Constants.SignInMessageThreshold);
        }
    }
}
