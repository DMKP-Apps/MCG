namespace MultiplayerServices.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGameStatetoGameStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GameStatus", "GameState", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GameStatus", "GameState");
        }
    }
}
