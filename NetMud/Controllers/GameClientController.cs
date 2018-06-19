﻿using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Models;

namespace Controllers
{
    public class GameClientController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public GameClientController()
        {
        }

        public GameClientController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index()
        {
            var model = new GameContextModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult ModularWindow()
        {
            return View("~/Views/GameClient/ModularWindow.cshtml");
        }
    }
}