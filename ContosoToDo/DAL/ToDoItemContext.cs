using ContosoToDo.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace ContosoToDo.DAL
{
    public class ToDoItemContext : DbContext
    {
        public ToDoItemContext(): base("ToDoItemContext")
        {

        }

        public DbSet<ToDoItem> ToDoItems { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}