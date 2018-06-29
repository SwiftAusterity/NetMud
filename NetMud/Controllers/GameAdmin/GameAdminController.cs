﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Models.Admin;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class GameAdminController : Controller
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

        public GameAdminController()
        {
        }

        public GameAdminController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        //Also called Dashboard in most of the html
        public ActionResult Index()
        {
            var dashboardModel = new DashboardViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                Inanimates = BackingDataCache.GetAll<IInanimateData>(),
                Rooms = BackingDataCache.GetAll<IRoomData>(),
                NPCs = BackingDataCache.GetAll<INonPlayerCharacter>(),
                Zones = BackingDataCache.GetAll<IZoneData>(),
                Locales = BackingDataCache.GetAll<ILocaleData>(),

                HelpFiles = BackingDataCache.GetAll<IHelp>(),
                DimensionalModels = BackingDataCache.GetAll<IDimensionalModelData>(),
                Materials = BackingDataCache.GetAll<IMaterial>(),
                Races = BackingDataCache.GetAll<IRace>(),
                Constants = BackingDataCache.GetAll<IConstants>(),
                Fauna = BackingDataCache.GetAll<IFauna>(),
                Flora = BackingDataCache.GetAll<IFlora>(),
                Minerals = BackingDataCache.GetAll<IMineral>(),
                UIModules = BackingDataCache.GetAll<IUIModule>(),

                DictionaryWords = ConfigDataCache.GetAll<IDictata>(),

                LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens(),
                LivePlayers = LiveCache.GetAll<IPlayer>().Count(),
                LiveInanimates = LiveCache.GetAll<IInanimate>().Count(),
                LiveRooms = LiveCache.GetAll<IRoom>().Count(),
                LiveNPCs = LiveCache.GetAll<IIntelligence>().Count(),
                LiveLocales = LiveCache.GetAll<ILocale>().Count(),
                LiveZones = LiveCache.GetAll<IZone>().Count(),

                WebsocketServers = LiveCache.GetAll<NetMud.Websock.Server>()
            };

            return View(dashboardModel);
        }

        public ActionResult ModalErrorOrClose(string Message = "")
        {
            return View("~/Views/GameAdmin/ModalErrorOrClose.cshtml", "_chromelessLayout", Message);
        }

        #region Live Threads
        public ActionResult StopRunningProcess(string processName)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownLoop(processName, 600, "{0} seconds before " + processName + " is shutdown.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningProcess[" + processName + "]", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent.";

            return RedirectToAction("Index", new { Message = message });
        }

        public ActionResult StopRunningAllProcess()
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownAll(600, "{0} seconds before TOTAL WORLD SHUTDOWN.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningALLPROCESSES", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent for entire world.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region "Communication"
        public ActionResult StopRunningAllWebsockets()
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var servers = LiveCache.GetAll<NetMud.Websock.Server>();

            foreach (var server in servers)
                server.Shutdown();

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopALLWebSockets", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent for all websockets.";

            return RedirectToAction("Index", new { Message = message });
        }

        public ActionResult StopWebSocket(int port)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var server = LiveCache.Get<NetMud.Websock.Server>("NetMud.Websock.Server_" + port.ToString());

            server.Shutdown();

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopWebSocket[" + port.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region Running Data
        public ActionResult BackupWorld()
        {
            var hotBack = new HotBackup();

            hotBack.WriteLiveBackup();
            BackingData.WriteFullBackup();

            return RedirectToAction("Index", new { Message = "Backup Started" });
        }

        public ActionResult RestoreWorld()
        {
            var hotBack = new HotBackup();

            //TODO: Ensure we suspend EVERYTHING going on (fights, etc), add some sort of announcement globally and delay the entire thing on a timer

            //Write the players out first to maintain their positions
            hotBack.WritePlayers();

            //restore everything
            hotBack.RestoreLiveBackup();

            return RedirectToAction("Index", new { Message = "Restore Started" });
        }
        #endregion
    }
}
