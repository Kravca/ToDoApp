namespace ContosoToDo.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class completed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ToDoItem", "Completed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ToDoItem", "Completed");
        }
    }
}
