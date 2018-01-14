namespace Authentication.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using Authentication.Models;
    using System.Security.Cryptography;
    using System.Text;

    internal sealed class Configuration : DbMigrationsConfiguration<Authentication.Models.AuthenticationContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        private static string HashPassword(string plainMessage)
        {
            byte[] data = Encoding.UTF8.GetBytes(plainMessage);
            using (HashAlgorithm SHA = new SHA256Managed())
            {
                byte[] encryptedBytes = SHA.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(SHA.Hash);
            }
        }

        protected override void Seed(Authentication.Models.AuthenticationContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            context.Users.AddOrUpdate(x => x.Id, 
                new User() { Id = 1, UserName = "Offender1", UserRole = "human", UserPassword = HashPassword("1234") },
                new User() { Id = 2, UserName = "Offender2", UserRole = "human", UserPassword = HashPassword("1234") },
                new User() { Id = 3, UserName = "user", UserRole = "admin", UserPassword = HashPassword("0000") });
        }
    }
}
