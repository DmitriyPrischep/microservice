namespace Machines.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Machines.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Machines.Models.MachinesContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Machines.Models.MachinesContext context)
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
            context.Machines.AddOrUpdate(x => x.Id,
                new Machine() { Id = 1, Type = "automobile", Mark = "Audi", Model = "A6", Year = 2010, StateNumber = "a432bo77", VIN = "", IdUsers = 1 },
                new Machine() { Id = 2, Type = "automobile", Mark = "Mersedes", Model = "E-class", Year = 2005, StateNumber = "t865oc777", VIN = "", IdUsers = 2 },
                new Machine() { Id = 3, Type = "automobile", Mark = "Lada", Model = "Vesta", Year = 2016, StateNumber = "m865ee99", VIN = "", IdUsers = 3 },
                new Machine() { Id = 4, Type = "automobile", Mark = "BMW", Model = "X5", Year = 2013, StateNumber = "k005kk799", VIN = "", IdUsers = 4 },
                new Machine() { Id = 5, Type = "automobile", Mark = "Toyota", Model = "Camry", Year = 2008, StateNumber = "m233po77", VIN = "", IdUsers = 5 }
            );
        }
    }
}
