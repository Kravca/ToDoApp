using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoToDo.Models
{
    public class ToDoItem
    {
        public int ID { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateDeadline { get; set; }

        public bool Completed { get; set; }
        public string Owner { get; set; }
    }

    public class UserProfile
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool EnableNotifications { get; set; }
    }
}