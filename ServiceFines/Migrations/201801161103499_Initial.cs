namespace ServiceFines.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InputStatisticMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ServerName = c.Int(nullable: false),
                        RequestType = c.Int(nullable: false),
                        State = c.Guid(nullable: false),
                        Detail = c.String(),
                        Time = c.DateTime(nullable: false),
                        CountSendMessage = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InputStatisticMessages");
        }
    }
}
