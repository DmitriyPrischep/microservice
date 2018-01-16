namespace Statistic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Statistics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServerName = c.Int(nullable: false),
                        RequestType = c.Int(nullable: false),
                        State = c.Guid(nullable: false),
                        Detail = c.String(),
                        Time = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.State, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Statistics", new[] { "State" });
            DropTable("dbo.Statistics");
        }
    }
}
