﻿using NetMud.Data.Architectural;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Script.Serialization;

namespace NetMud.Data.System
{
    [Serializable]
    public class GlobalConfig : ConfigData, IGlobalConfig
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None;

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.GameWorld;

        /// <summary>
        /// Is the websockets portal allowing new connections
        /// </summary>
        [Display(Name = "Websocket Portal Available", Description = "Are new connections being accepted over websockets?")]
        [UIHint("Boolean")]
        public bool WebsocketPortalActive { get; set; }

        /// <summary>
        /// Are new users allowed to register
        /// </summary>
        [Display(Name = "User Creation", Description = "Are new users allowed to register?")]
        [UIHint("Boolean")]
        public bool UserCreationActive { get; set; }

        /// <summary>
        /// Are only admins allowed to log in - noone at StaffRank.Player
        /// </summary>
        [Display(Name = "Admins Only", Description = "Are only admins allowed to log in - noone at StaffRank.Player?")]
        [UIHint("Boolean")]
        public bool AdminsOnly { get; set; }

        /// <summary>
        /// The base language for the system
        /// </summary>
        [JsonProperty("BaseLanguage")]
        private ConfigDataCacheKey _baseLanguage { get; set; }

        /// <summary>
        /// The language this is derived from
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Base Language", Description = "The base language for the system.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public ILanguage BaseLanguage
        {
            get
            {
                if (_baseLanguage == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<ILanguage>(_baseLanguage);
            }
            set
            {
                if (value == null)
                {
                    _baseLanguage = null;
                    return;
                }

                _baseLanguage = new ConfigDataCacheKey(value);
            }
        }

        public GlobalConfig()
        {
            Name = "LiveSettings";
            WebsocketPortalActive = true;
            AdminsOnly = false;
            UserCreationActive = true;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new GlobalConfig
            {
                Name = Name,
                WebsocketPortalActive = WebsocketPortalActive,
                UserCreationActive = UserCreationActive,
                AdminsOnly = AdminsOnly,
            };
        }
    }
}
