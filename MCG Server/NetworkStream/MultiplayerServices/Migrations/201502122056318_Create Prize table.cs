namespace MultiplayerServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreatePrizetable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Prizes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 128),
                        ShortName = c.String(maxLength: 25),
                        Type = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .Index(t => t.Name)
                .Index(t => t.ShortName);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Prizes", new[] { "ShortName" });
            DropIndex("dbo.Prizes", new[] { "Name" });
            DropTable("dbo.Prizes");
        }
    }
}
