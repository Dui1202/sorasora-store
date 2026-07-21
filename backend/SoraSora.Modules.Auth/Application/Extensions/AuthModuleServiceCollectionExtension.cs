using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Modules.Auth.Application.Controllers;

namespace SoraSora.Modules.Auth.Application.Extensions
{
    public static class AuthModuleServiceCollectionExtension
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services)
        {
            services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

            services
                .AddAuthorizationBuilder()
                .SetFallbackPolicy(
                    new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
                );

            services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);

            return services;
        }
    }
}
