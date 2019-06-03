using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SintefSecureBoilerplate.DAL.Identity
{
    public class IdentityDatabaseIntializer
    {
        private static string DefaultPassword { get; } = "1234Ab.";
        private static string DefaultEMail { get; } = "mail@mail.com";

        private static void SeedRolesToDatabase(IdentityDatabaseContext context)
        {
            //TODO: Figure out some sort of for roles in role in order to avoid having to remember all roles and remember to add them. This is just another step of misdirection in which we can forget to add all the roles
            context.Roles.Add(new IdentityRole(IdentityDatabaseContext.AdministratorRoleString)
            {
                NormalizedName = IdentityDatabaseContext.AdministratorRoleString.Normalize()
            });
            context.Roles.Add(new IdentityRole(IdentityDatabaseContext.PrivilegedShitlordRoleString)
            {
                NormalizedName = IdentityDatabaseContext.PrivilegedShitlordRoleString.Normalize()
            });
            context.Roles.Add(new IdentityRole(IdentityDatabaseContext.BaseUsersRoleString)
            {
                NormalizedName = IdentityDatabaseContext.BaseUsersRoleString.Normalize()
            });
            context.SaveChanges();
        }

        public static void IntializeProductionForceDeleteDatabase(IdentityDatabaseContext context)
        {
            if (context.Database.EnsureDeleted())
            {
                InitializeProductionEnvironment(context);
            }
        }

        public static void InitializeProductionEnvironment(IdentityDatabaseContext context)
        {
            context.Database.Migrate();
            if (context.Roles.Any())
            {
                return; //DB has been seeded
            }

            SeedRolesToDatabase(context);
            context.SaveChanges(); //Double guard in case you need more seeding, async changes etc
        }

        public static void IntializeDevelopmentForceDeleteDatabase(IdentityDatabaseContext context)
        {
            if (context.Database.EnsureDeleted())
            {
                InitializeDevelopmentEnvironment(context);
            }
        }

        public static void InitializeDevelopmentEnvironment(IdentityDatabaseContext context)
        {
            context.Database.Migrate();
            if (context.Roles.Any())
            {
                return; //DB has been seeded
            }

            SeedRolesToDatabase(context);
            context.SaveChanges(); //Double guard in case you need more seeding, async changes etc
        }

        // Never use in prod
        public static ApplicationUser DebugCreateRole(IdentityDatabaseContext context, string login,
            string password = null, string eMail = null, string role = IdentityDatabaseContext.BaseUsersRoleString)
        {
            password = password ?? DefaultPassword;
            eMail = eMail ?? DefaultEMail;
            switch (role) //TODO: Auto switch roles if we manage to do foreach role in roles, maybe polymorphism?
            {
                case IdentityDatabaseContext.AdministratorRoleString:
                    return IdentityDatabaseContext.CreateAdministrator(context, login, password, eMail);
                case IdentityDatabaseContext.PrivilegedShitlordRoleString:
                    return IdentityDatabaseContext.CreatePrivilegedShitlord(context, login, password, eMail);
                case IdentityDatabaseContext.BaseUsersRoleString:
                    return IdentityDatabaseContext.CreateBasicUser(context, login, password, eMail);
                default:
                    return null;
            }
        }
    }
}