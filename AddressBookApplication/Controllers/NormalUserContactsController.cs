using System;
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
    public class NormalUserContactsController : Controller
    {
        private AddressBookEntities db = new AddressBookEntities();

     
        public ActionResult Main()
        {
            return View();
        }

        // GET: NormalUser
        public ActionResult Index()
        {
            var id = TempData["id"];
            return View(db.ContactDetails.ToList().Where(x => x.UserId == Convert.ToInt64(id)).ToList());
         
        }
     

        // GET: NormalUser/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NormalUser/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "ContactId,UserId,FirstName,LastName,PhoneNumber,StreetName,City,Province,PostalCode,Country,Note")] ContactDetail contact)
        {
           
            if (ModelState.IsValid)
            {
                db.ContactDetails.Add(contact);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(contact);
        }

        // GET: NormalUser/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactDetail contact = db.ContactDetails.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: NormalUser/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit([Bind(Include = "ContactId,UserId,FirstName,LastName,PhoneNumber,StreetName,City,Province,PostalCode,Country,Note")] ContactDetail contact)
        {
            if (ModelState.IsValid)
            {
                string str = contact.Note;
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contact);
        }

        // GET: NormalUser/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactDetail contact = db.ContactDetails.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: NormalUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            ContactDetail contact = db.ContactDetails.Find(id);
            db.ContactDetails.Remove(contact);
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
