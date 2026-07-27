using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SoraSora.Infrastructure.Identity;
using SoraSora.Infrastructure.Persistence;
using SoraSora.Modules.Auth.Application.Controllers;
using SoraSora.Modules.Auth.Application.Services;
using SoraSora.Modules.Auth.Application.Services.Abstractions;
using SoraSora.Modules.Auth.Authorization;
using SoraSora.Modules.Auth.Contracts.Validators;

namespace SoraSora.Modules.Auth.Application.Extensions;

public static class AuthModuleServiceCollectionExtension
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            foreach (var permission in AppPermissions.All)
            {
                options.AddPolicy(
                    permission,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        policy.AddRequirements(new PermissionRequirement(permission));
                    }
                );
            }
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.SignIn.RequireConfirmedEmail = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SoraSoraDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "sorasora.auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;

            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);

            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    return Task.CompletedTask;
                },

                OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;

                    return Task.CompletedTask;
                },
            };
        });

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRolesService, RolesService>();
        services.AddScoped<IUserRolesService, UserRolesService>();

        services.AddControllers().AddApplicationPart(typeof(AuthenticationController).Assembly);

        return services;
    }
}
