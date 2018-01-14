namespace Authentication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Codes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessCode = c.String(),
                        Timeofrelease = c.DateTime(),
                        OwnerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Owners", t => t.OwnerId, cascadeDelete: true)
                .Index(t => t.OwnerId);
            
            CreateTable(
                "dbo.Owners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClienSecret = c.Int(nullable: false),
                        RedirectURI = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccessToken = c.String(),
                        RefreshToken = c.String(),
                        TimeOfReleaseAccessToken = c.DateTime(),
                        TimeOfReleaseRefreshToken = c.DateTime(),
                        UserId = c.Int(nullable: false),
                        AccessCodeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Codes", t => t.AccessCodeId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.AccessCodeId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        UserPassword = c.String(),
                        UserRole = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tokens", "UserId", "dbo.Users");
            DropForeignKey("dbo.Tokens", "AccessCodeId", "dbo.Codes");
            DropForeignKey("dbo.Codes", "OwnerId", "dbo.Owners");
            DropIndex("dbo.Tokens", new[] { "AccessCodeId" });
            DropIndex("dbo.Tokens", new[] { "UserId" });
            DropIndex("dbo.Codes", new[] { "OwnerId" });
            DropTable("dbo.Users");
            DropTable("dbo.Tokens");
            DropTable("dbo.Owners");
            DropTable("dbo.Codes");
        }
    }
}
