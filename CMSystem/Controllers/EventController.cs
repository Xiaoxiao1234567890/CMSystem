﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CMSystem.Models;
using CMSystem.Attribute;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace CMSystem.Controllers
{
    [Authorize(Roles = "Member, Customer")]
    [ClaimsAuthorize(ClaimTypes.Role, "Member, Customer")]
    public class EventController : Controller
    {
        private int count = 0;
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Event
        public ActionResult Index()
        {
            return View();
        }

        // Return the event objects as a list in Json.
        public JsonResult GetEventJSON()
        {
            List<Event> eventList = db.Event.ToList();
            return Json(eventList, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<Event> GetEvent()
        {
            return db.Event.ToList();
        }

        public ActionResult BuildEventTable()
        {
            return PartialView("_EventTable", GetEvent());
        }

        //Get one event and return a partial view with the event data
        public ActionResult GetOneEvent(int eventId)
        {
            Event @event = db.Event.FirstOrDefault(y => y.EventId == eventId);
            IEnumerable<EventSignUp> EventSignUp = db.EventSignUp.Where(x => x.Event.EventId == @event.EventId).ToList();

            int countSignUp = 0;
            foreach (EventSignUp eventSignUp in EventSignUp)
            {
                countSignUp++;
            }

            ViewBag.Percent = Math.Round(100f * ((float)countSignUp / (float)@event.Capacity));

            ViewData["Number"] = countSignUp;
            return PartialView("_SingleEvent", @event);
        }


        // GET: Event/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Event.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // GET: Event/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Event/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EventId,EventName,EventDescription,StartTime,EndTime,Location,Deadline,Capacity,Role")] Event @event)
        {
            if (ModelState.IsValid)
            {
                @event.CreationTime = DateTime.Today;
                db.Event.Add(@event);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(@event);
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        [ClaimsAuthorize(ClaimTypes.Role, "Member")]
        public ActionResult AJAXCreate([Bind(Include = "EventId,EventName,EventDescription,StartTime,EndTime,Location,Deadline,Capacity")] Event @event)
        {
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.FirstOrDefault
                (x => x.Id == currentUserId);

            count = @event.EventId;
            @event.User = currentUser;
            @event.CreationTime = DateTime.Today;
            @event.Role = 1;
            db.Event.Add(@event);
            db.SaveChanges();
            return PartialView("_EventTable", GetEvent());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public ActionResult AJAXCreateSignUp(int EventId)
        {
            string currentUserId = User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.FirstOrDefault
                (x => x.Id == currentUserId);
            Event @event = db.Event.FirstOrDefault(y => y.EventId == EventId);
            //SQL: select * from Event where EventId = EventId;
            EventSignUp e = new EventSignUp();
            e.Customer = currentUser;
            e.Event = @event;
            e.SignUpTime = DateTime.Now;
            db.EventSignUp.Add(e);
            db.SaveChanges();
            GetOneEvent(EventId);
            return PartialView("_SingleEvent", @event);
        }

        // GET: Event/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Event.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Event/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EventId,EventName,EventDescription,StartTime,EndTime,Location,CreationTime,Deadline,Capacity,Role")] Event @event)
        {
            if (ModelState.IsValid)
            {
                db.Entry(@event).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(@event);
        }

        // GET: Event/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = db.Event.Find(id);
            if (@event == null)
            {
                return HttpNotFound();
            }
            return View(@event);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Event @event = db.Event.Find(id);
            db.Event.Remove(@event);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member")]
        [ClaimsAuthorize(ClaimTypes.Role, "Member")]
        public ActionResult AJAXDeleteConfirmed(int eventId)
        {
            Event @event = db.Event.Find(eventId);
            db.EventSignUp.RemoveRange(db.EventSignUp.Where(x => x.Event.EventId == eventId));
            db.Event.Remove(@event);
            db.SaveChanges();

            return PartialView("_EventTable", GetEvent());
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
