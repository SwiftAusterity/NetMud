﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Linguistic;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class DictionaryController : Controller
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

        public DictionaryController()
        {
        }

        public DictionaryController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageDictionaryViewModel vModel = new ManageDictionaryViewModel()
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Dictionary/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Dictionary/Remove/{removeId?}/{authorizeRemove?}")]
        public ActionResult Remove(string removeId = "", string authorizeRemove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                ILexeme obj = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, removeId));

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstants[" + removeId + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Purge()
        {
            IEnumerable<ILexeme> dictionary = ConfigDataCache.GetAll<ILexeme>();

            foreach (ILexeme dict in dictionary)
            {
                dict.SystemRemove();
            }

            return RedirectToAction("Index", new { Message = "By fire, it is purged." });
        }

        [HttpGet]
        public ActionResult Add()
        {
            AddEditDictionaryViewModel vModel = new AddEditDictionaryViewModel()
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Dictionary/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditDictionaryViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            ILexeme newObj = vModel.DataObject;
            string message;
            if (newObj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddLexeme[" + newObj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ILexeme obj = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, id));

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            AddEditDictionaryViewModel vModel = new AddEditDictionaryViewModel(obj.WordForms)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = (Lexeme)obj
            };

            return View("~/Views/GameAdmin/Dictionary/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id, AddEditDictionaryViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            ILexeme obj = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, id));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Language = vModel.DataObject.Language;
            obj.Phonetics = vModel.DataObject.Phonetics;
            obj.Curated = vModel.DataObject.Curated;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditLexeme[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #region dictata
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRelatedWord(string lexemeId, string id, AddEditDictataViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, lexemeId));
            if (lex == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            IDictata dict = lex.WordForms.FirstOrDefault(form => form.UniqueKey == id);
            if (dict == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            Dictata relatedWord = new Dictata()
            {
                Name = vModel.Word,
                Severity = vModel.Severity,
                Quality = vModel.Quality,
                Elegance = vModel.Elegance,
                Tense = dict.Tense,
                Language = dict.Language,
                WordType = dict.WordType,
                Feminine = dict.Feminine,
                Possessive = dict.Possessive,
                Plural = dict.Plural,
                Determinant = dict.Determinant,
                Positional = dict.Positional,
                Perspective = dict.Perspective,
                Semantics = dict.Semantics,
                Context = dict.Context,
                Vulgar = dict.Vulgar
            };

            string message;
            if (dict.MakeRelatedWord(dict.Language, vModel.Word, vModel.Synonym, vModel.Severity, vModel.Elegance, vModel.Quality, dict.Semantics, relatedWord) != null)
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditLexeme[" + lex.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Dictionary/RemoveDictata/{removeId?}/{authorizeRemove?}")]
        public ActionResult RemoveDictata(string removeId = "", string authorizeRemove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                ILexeme lex = ConfigDataCache.Get<ILexeme>(removeId.Substring(0, removeId.LastIndexOf("_") - 1), ConfigDataType.Dictionary);
                IDictata obj = lex?.WordForms?.FirstOrDefault(form => form.UniqueKey == removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else
                {
                    HashSet<IDictata> wordForms = lex.WordForms.ToHashSet();
                    wordForms.RemoveWhere(form => form.UniqueKey == removeId);
                    lex.WordForms = wordForms.ToArray();

                    lex.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstants[" + removeId + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddDictata(string lexemeId)
        {
            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, lexemeId));
            if (lex == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            AddEditDictataViewModel vModel = new AddEditDictataViewModel(lex)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Dictionary/AddDictata.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDictata(string lexemeId, AddEditDictataViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, lexemeId));
            if (lex == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            IDictata newObj = vModel.DataObject;

            lex.AddNewForm(newObj);

            string message;
            if (lex.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddDictata[" + newObj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditDictata(string lexemeId, string id)
        {
            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, lexemeId));
            IDictata obj = lex?.WordForms?.FirstOrDefault(form => form.UniqueKey == id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            AddEditDictataViewModel vModel = new AddEditDictataViewModel(lex, obj)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Dictionary/EditDictata.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDictata(string lexemeId, string id, AddEditDictataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            ILexeme lex = ConfigDataCache.Get<ILexeme>(new ConfigDataCacheKey(typeof(ILexeme), ConfigDataType.Dictionary, lexemeId));
            IDictata obj = lex?.WordForms?.FirstOrDefault(form => form.UniqueKey == id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Severity = vModel.DataObject.Severity;
            obj.Quality = vModel.DataObject.Quality;
            obj.Elegance = vModel.DataObject.Elegance;
            obj.Tense = vModel.DataObject.Tense;
            obj.RelatedWords = vModel.DataObject.RelatedWords;
            obj.Language = vModel.DataObject.Language;
            obj.WordType = vModel.DataObject.WordType;
            obj.Feminine = vModel.DataObject.Feminine;
            obj.Possessive = vModel.DataObject.Possessive;
            obj.Plural = vModel.DataObject.Plural;
            obj.Determinant = vModel.DataObject.Determinant;
            obj.Positional = vModel.DataObject.Positional;
            obj.Perspective = vModel.DataObject.Perspective;
            obj.Semantics = vModel.DataObject.Semantics;
            obj.Vulgar = vModel.DataObject.Vulgar;
            obj.Context = vModel.DataObject.Context;

            lex.AddNewForm(obj);

            if (lex.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditDictata[" + obj.UniqueKey + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #endregion
    }
}