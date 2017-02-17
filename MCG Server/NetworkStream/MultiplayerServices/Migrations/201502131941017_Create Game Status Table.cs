namespace MultiplayerServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateGameStatusTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameStatus",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 50),
                        PropertyValue = c.String(storeType: "ntext"),
                        CreatedOn = c.DateTime(nullable: false),
                        ModifiedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GameStatus");
        }
    }
}
