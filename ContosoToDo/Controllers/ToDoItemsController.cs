using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ContosoToDo.DAL;
using ContosoToDo.Models;

namespace ContosoToDo.Controllers
{
    public class ToDoItemsController : Controller
    {
        private ToDoItemContext db = new ToDoItemContext();

        // GET: ToDoItems
        public ActionResult Index()
        {
            if (db.UserProfiles.Where(x => x.UserName == HttpContext.User.Identity.Name).FirstOrDefault() == null)
            {
                return RedirectToAction("Create","UserProfiles");
            }
            else
            {
                return View(db.ToDoItems.ToList());
            }
            //return View(db.ToDoItems.ToList());
        }

        // GET: ToDoItems/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ToDoItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Description,DateCreated,DateDeadline,Completed,Owner")] ToDoItem toDoItem)
        {
            if (ModelState.IsValid)
            {
                toDoItem.Owner = HttpContext.User.Identity.Name;
                toDoItem.DateCreated = DateTime.UtcNow;
                db.ToDoItems.Add(toDoItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(toDoItem);
        }

        // GET: ToDoItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDoItem toDoItem = db.ToDoItems.Find(id);
            if (toDoItem == null)
            {
                return HttpNotFound();
            }
            return View(toDoItem);
        }

        // POST: ToDoItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Description,DateCreated,DateDeadline,Completed,Owner")] ToDoItem toDoItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(toDoItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(toDoItem);
        }

        // GET: ToDoItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ToDoItem toDoItem = db.ToDoItems.Find(id);
            if (toDoItem == null)
            {
                return HttpNotFound();
            }
            return View(toDoItem);
        }

        // POST: ToDoItems/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ToDoItem toDoItem = db.ToDoItems.Find(id);
            db.ToDoItems.Remove(toDoItem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
