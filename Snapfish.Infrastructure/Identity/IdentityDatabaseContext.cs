using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SintefSecureBoilerplate.DAL.Identity
{
    public class IdentityDatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityDatabaseContext(DbContextOptions<IdentityDatabaseContext> options) : base(options)
        {
        }

        // ReSharper disable once RedundantOverriddenMember Incase someone in the future needs to override the model
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        //TODO: Do this with dependency injection so its easier for derived application to implement their own subset based on whatever their need it. This will also do the derived methods further down for creation simplified if they can be done generic

        #region Roles

        public const string AdministratorRoleString = "ADMIN";
        public const string PrivilegedShitlordRoleString = "White-Male";
        public const string BaseUsersRoleString = "User";

        #endregion

        #region User Creation

        private static ApplicationUser CreateUser(IdentityDatabaseContext context, string login, string password,
            string eMail, string role)
        {
            var user = new ApplicationUser
            {
                UserName = login,
                Email = eMail,
                NormalizedUserName = login.Normalize(),
                NormalizedEmail = eMail.Normalize(),
                EmailConfirmed = false,
                LockoutEnabled = false,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            var userStore = new UserStore<ApplicationUser>(context);
            userStore.CreateAsync(user).Wait();
            userStore.AddToRoleAsync(user, role).Wait();

            context.SaveChanges();
            return user;
        }

        public static ApplicationUser CreateAdministrator(IdentityDatabaseContext context, string login,
            string password, string eMail)
        {
            return CreateUser(context, login, password, eMail, AdministratorRoleString);
        }

        public static ApplicationUser CreatePrivilegedShitlord(IdentityDatabaseContext context, string login,
            string password, string eMail)
        {
            return CreateUser(context, login, password, eMail, PrivilegedShitlordRoleString);
        }

        public static ApplicationUser CreateBasicUser(IdentityDatabaseContext context, string login,
            string password, string eMail)
        {
            return CreateUser(context, login, password, eMail, BaseUsersRoleString);
        }

        #endregion
    }
}