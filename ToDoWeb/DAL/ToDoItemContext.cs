using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ToDoWeb.DAL
{
    public class ToDoItemContext : DbContext
    {
        public ToDoItemContext() : base("ToDoItemContext")
        {

        }

        public DbSet<ToDoItem> ToDoItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}