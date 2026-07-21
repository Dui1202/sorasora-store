using Amazon.S3.Model;
using Microsoft.AspNetCore.Identity;

namespace SoraSora.Infrastructure.Identity
{
    public sealed class ApplicationUser : IdentityUser
    {
        public static ApplicationUser Create(string email)
        {
            return new ApplicationUser { Email = email, UserName = email };
        }
    }
}
