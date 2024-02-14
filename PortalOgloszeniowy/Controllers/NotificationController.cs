using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PortalOgloszeniowy.Data;
using PortalOgloszeniowy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalOgloszeniowy.Controllers
{
    public class NotificationController : Controller
    {

        private readonly ApplicationDbContext _db;
        private UserManager<User> _userManager;
        private SignInManager<User> _signInManager;

        public NotificationController(
            ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager
            )
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: NotificationController
        public async Task<ActionResult> Index()
        {
            if(!_signInManager.IsSignedIn(User)) return View();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            return View(user.Notifications);
        }

        // GET: NotificationController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: NotificationController/Create
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (!_signInManager.IsSignedIn(User)) return View("Index");
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user.UserName != User.Identity.Name) return View("Index");
                var notList = user.Notifications.ToList();
                notList.Remove(notList.Find(not=>not.Id==id));
                user.Notifications = notList;
                await _userManager.UpdateAsync(user);
                return RedirectToAction("Index");
            }
            catch
            {
                return View("Index");
            }
        }

    }
}
