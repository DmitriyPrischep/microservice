namespace Fines.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Fines.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Fines.Models.FinesContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Fines.Models.FinesContext context)
        {
            context.Fines.AddOrUpdate(x => x.Id,
                new Fine() { Id = 1, NameFine = "Превышение скорости от 21 до 40 км/ч", AmountFine = 500 },
                new Fine() { Id = 2, NameFine = "Проезд на красный свет", AmountFine = 1000 },
                new Fine() { Id = 3, NameFine = "Неправильная парковка", AmountFine = 2000 },
                new Fine() { Id = 4, NameFine = "Управление транспортным средством при наличии неисправностей", AmountFine = 500 },
                new Fine() { Id = 5, NameFine = "Превышение скорости от 21 до 40 км/ч", AmountFine = 1500 }
                );
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


        }
    }
}
