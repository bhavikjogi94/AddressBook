using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AddressBookApplication.Models;
using PagedList;

namespace AddressBookApplication.Controllers
{
    public class ManageUserController : Controller
    {
        private AddressBookEntities db = new AddressBookEntities();
        // GET: ManageUser
        public ActionResult Index()
        {
            return View(db.UserDetails.ToList());
        }

        // GET: ManageUser/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ContactDetail = db.ContactDetails.Where(x => x.UserId == id).ToList();
            if (ContactDetail == null)
            {
                return HttpNotFound();
            }
            Session["userId"] = id;
            // ViewData["Userid"] = id;
            return View(ContactDetail.ToList());
        }

        // GET: ManageUser/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ManageUser/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserId,Email,Password,FirstName,LastName,UserInfo")] UserDetail userDetail)
        {
            var a = new HomeController();
            string keyBytes = a.ComputeHash(userDetail.Email, userDetail.Password, HomeController.HashName.SHA256);
            string encryptPassword = a.Encrypt(userDetail.Password, keyBytes, string.Empty);
            userDetail.Password = encryptPassword;
            if (ModelState.IsValid)
            {   
                db.UserDetails.Add(userDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(userDetail);
        }

        // GET: ManageUser/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDetail userDetail = db.UserDetails.Find(id);
            if (userDetail == null)
            {
                return HttpNotFound();
            }
            return View(userDetail);
        }

        // POST: ManageUser/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserId,Email,Password,FirstName,LastName,UserInfo")] UserDetail userDetail)
        {
            if (ModelState.IsValid)
            {
                var current = db.UserDetails.Where(m=>m.Email==userDetail.Email).SingleOrDefault();
                userDetail.UserId = current.UserId;
                //db.Entry(userDetail).State = EntityState.Modified;
                var obj = db.UserDetails.Find(userDetail.UserId);
                obj.FirstName = userDetail.FirstName;
                obj.LastName = userDetail.LastName;
                obj.UserInfo = userDetail.UserInfo;
                obj.Email = userDetail.Email;
                if (obj.Password != userDetail.Password)
                {
                    var a = new HomeController();
                    string keyBytes = a.ComputeHash(userDetail.Email, userDetail.Password, HomeController.HashName.SHA256);
                    string encryptPassword = a.Encrypt(userDetail.Password, keyBytes, string.Empty);
                    userDetail.Password = encryptPassword;
                    obj.Password = userDetail.Password;
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userDetail);
        }

        // GET: ManageUser/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserDetail userDetail = db.UserDetails.Find(id);
            if (userDetail == null)
            {
                return HttpNotFound();
            }
            return View(userDetail);
        }

        // POST: ManageUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            UserDetail userDetail = db.UserDetails.Find(id);
            db.UserDetails.Remove(userDetail);
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