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
    public class UserProfilesController : Controller
    {
        private ToDoItemContext db = new ToDoItemContext();

        // GET: UserProfiles
        public ActionResult Index()
        {
            if (db.UserProfiles.Where(x => x.UserName == HttpContext.User.Identity.Name).FirstOrDefault() == null)
            {
                return RedirectToAction("Create");
            }
            else
            {
                return RedirectToAction("Edit", new { id = db.UserProfiles.Where(x => x.UserName == HttpContext.User.Identity.Name).FirstOrDefault().ID });
            }
        }


        // GET: UserProfiles/Create
        public ActionResult Create()
        {
            UserProfile NewUser = new UserProfile();
            NewUser.UserName = HttpContext.User.Identity.Name;
            return View(NewUser);
        }

        // POST: UserProfiles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Create([Bind(Include = "ID,UserName,Email,EnableNotifications")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                db.UserProfiles.Add(userProfile);
                db.SaveChanges();
                return RedirectToAction("Index", "ToDoItems");
            }

            return View(userProfile);
        }

        // GET: UserProfiles/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserProfile userProfile = db.UserProfiles.Find(id);
            if (userProfile == null)
            {
                return HttpNotFound();
            }
            return View(userProfile);
        }

        // POST: UserProfiles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit([Bind(Include = "ID,UserName,Email,EnableNotifications")] UserProfile userProfile)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userProfile).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "ToDoItems");
            }
            return View(userProfile);
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
