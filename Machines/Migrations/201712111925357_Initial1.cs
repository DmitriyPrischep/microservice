namespace Machines.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Machines", "VIN", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Machines", "VIN", c => c.String(nullable: false));
        }
    }
}
