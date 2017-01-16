namespace NetworkServer.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPlayerstable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        UID = c.String(nullable: false, maxLength: 128),
                        AccountName = c.String(maxLength: 256),
                    })
                .PrimaryKey(t => t.UID)
                .Index(t => t.AccountName, unique: true);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Players", new[] { "AccountName" });
            DropTable("dbo.Players");
        }
    }
}
