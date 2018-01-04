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
                new Fine() { Id = 1, NameFine = "���������� �������� �� 21 �� 40 ��/�", AmountFine = 500 },
                new Fine() { Id = 2, NameFine = "������ �� ������� ����", AmountFine = 1000 },
                new Fine() { Id = 3, NameFine = "������������ ��������", AmountFine = 2000 },
                new Fine() { Id = 4, NameFine = "���������� ������������ ��������� ��� ������� ��������������", AmountFine = 500 },
                new Fine() { Id = 5, NameFine = "���������� �������� �� 21 �� 40 ��/�", AmountFine = 1500 }
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
