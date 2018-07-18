namespace ContosoToDo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userprofile : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        Email = c.String(),
                        EnableNotifications = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserProfile");
        }
    }
}
