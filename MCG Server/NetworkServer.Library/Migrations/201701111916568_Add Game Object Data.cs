namespace NetworkServer.Library.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGameObjectData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameObjects",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        SessionId = c.Guid(nullable: false),
                        holeId = c.String(maxLength: 128),
                        timeStamp = c.DateTime(nullable: false),
                        objectId = c.String(maxLength: 256),
                        objectName = c.String(maxLength: 256),
                        type = c.String(maxLength: 128),
                        data = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.GameObjects");
        }
    }
}
