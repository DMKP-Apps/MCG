namespace MultiplayerServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientPropertyBags",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        AccountId = c.String(maxLength: 50),
                        PropertyKey = c.String(maxLength: 128),
                        PropertyValue = c.String(storeType: "ntext"),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.ClientAccounts", t => t.AccountId)
                .Index(t => t.AccountId)
                .Index(t => t.PropertyKey);
            
            CreateTable(
                "dbo.ClientAccounts",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 128),
                        IsOnline = c.Boolean(nullable: false),
                        LastRequestOn = c.DateTime(),
                        IsLockedOut = c.Boolean(nullable: false),
                        LockedOutOn = c.DateTime(),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClientPropertyBags", "AccountId", "dbo.ClientAccounts");
            DropIndex("dbo.ClientPropertyBags", new[] { "PropertyKey" });
            DropIndex("dbo.ClientPropertyBags", new[] { "AccountId" });
            DropTable("dbo.ClientAccounts");
            DropTable("dbo.ClientPropertyBags");
        }
    }
}
