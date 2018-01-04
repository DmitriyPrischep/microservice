namespace Users.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Users.Models;
    internal sealed class Configuration : DbMigrationsConfiguration<Users.Models.UsersContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Users.Models.UsersContext context)
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
            new User() { Id = 1, FIO = "Ivanov Artem", Adress = "Moscow Sovetskaya d12 kv44", Phone = "89201472586", IdFines = 1 },
            new User() { Id = 2, FIO = "Sokolov Dmitriy", Adress = "Moscow Leninskiy prospekt d73 kv14", Phone = "89663214785", IdFines = 2 },
            new User() { Id = 3, FIO = "Rowan Miller", Adress = "Pskov Krasnaya d18 kv76", Phone = "89774561214", IdFines = 1 },
            new User() { Id = 4, FIO = "Andrew Peters", Adress = "Moscow Armeyskaya d10 kv88", Phone = "89157563214", IdFines = 3 },
            new User() { Id = 5, FIO = "Brice Lambson", Adress = "Kazan Svetlaya d57 kv10", Phone = "89851232514", IdFines = 1 }
            );

        }
    }
}
