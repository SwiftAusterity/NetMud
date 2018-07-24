﻿using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// The account configuration for a player
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class AccountConfig : ConfigData, IAccountConfig
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None;

        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Player;

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _account { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IAccount Account
        {
            get
            {
                if (_account == null && !string.IsNullOrWhiteSpace(Name))
                    _account = System.Account.GetByHandle(Name);

                return _account;
            }
        }

        /// <summary>
        /// Whether or not the person wants the tutorial tooltips on; false = off
        /// </summary>
        public bool UITutorialMode { get; set; }

        /// <summary>
        /// Does someone see chatter from the Gossip network?
        /// </summary>
        public bool GossipSubscriber { get; set; }

        [JsonProperty("UIModules")]
        public IList<Tuple<BackingDataCacheKey, int>> _UIModules { get; set; }

        /// <summary>
        /// The modules to load. Module, quadrant
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<Tuple<IUIModule, int>> UIModules
        {
            get
            {
                if (_UIModules == null)
                    _UIModules = new List<Tuple<BackingDataCacheKey, int>>();

                return _UIModules.Select(k => new Tuple<IUIModule, int>(BackingDataCache.Get<IUIModule>(k.Item1), k.Item2));
            }
            set
            {
                if (value == null)
                    return;

                _UIModules = value.Select(k => new Tuple<BackingDataCacheKey, int>(new BackingDataCacheKey(k.Item1), k.Item2)).ToList();
            }
        }

        /// <summary>
        /// Friends and Foes of this account
        /// </summary>
        public IEnumerable<IAcquaintence> Acquaintences { get; set; }

        [JsonProperty("Notifications")]
        public HashSet<ConfigDataCacheKey> _notifications { get; set; }

        /// <summary>
        /// Messages to this account
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<IPlayerMessage> Notifications
        {
            get
            {
                if (_notifications == null)
                    _notifications = new HashSet<ConfigDataCacheKey>();

                return ConfigDataCache.GetMany<IPlayerMessage>(_notifications);
            }
            set
            {
                if (value != null)
                    _notifications = new HashSet<ConfigDataCacheKey>(value.Select(note => new ConfigDataCacheKey(note)));
            }
        }

        [JsonConstructor]
        public AccountConfig()
        {
        }

        public AccountConfig(IAccount account)
        {
            _account = account;

            if (string.IsNullOrWhiteSpace(Name))
                Name = _account.GlobalIdentityHandle;
        }

        /// <summary>
        /// Does this person want this notification
        /// </summary>
        /// <param name="playerName">The player's name who's triggering the notification</param>
        /// <param name="isGossipSystem">Is this the gossip system</param>
        /// <param name="type">what type of notification is this</param>
        /// <returns>Whether or not they want it</returns>
        public bool WantsNotification(string playerName, bool isGossipSystem, AcquaintenceNotifications type)
        {
            return Acquaintences.Any(acq => acq.IsFriend
                                                && acq.PersonHandle.Equals(playerName, StringComparison.InvariantCultureIgnoreCase)
                                                && acq.NotificationSubscriptions.Contains(type)
                                                && isGossipSystem == acq.GossipSystem);
        }

        /// <summary>
        /// Does this person want this notification
        /// </summary>
        /// <param name="playerName">The player's name who's triggering the notification</param>
        /// <param name="isGossipSystem">Is this the gossip system</param>
        /// <param name="type">what type of notification is this</param>
        /// <returns>Whether or not they want it</returns>
        public bool IsBlocking(string playerName, bool isGossipSystem)
        {
            return Acquaintences.Any(acq => !acq.IsFriend
                                                && acq.PersonHandle.Equals(playerName, StringComparison.InvariantCultureIgnoreCase)
                                                && isGossipSystem == acq.GossipSystem);
        }

        public bool RestoreConfig(IAccount account)
        {
            if (account == null)
                return false;

            if (_account == null)
                _account = account;

            var configData = new DataAccess.FileSystem.ConfigData();

            var directory = configData.GetCurrentDirectoryForEntity(this);

            var charDirectory = new DirectoryInfo(directory);
            IAccountConfig newConfig = null;

            try
            {
                var file = charDirectory.EnumerateFiles("*.AccountConfig", SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (file == null)
                    return false;

                newConfig = (IAccountConfig)configData.ReadEntity(file, GetType());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                //Let it keep going
            }

            if (newConfig != null)
            {
                UIModules = newConfig.UIModules;
                UITutorialMode = newConfig.UITutorialMode;
                GossipSubscriber = newConfig.GossipSubscriber;

                GetNotifications(configData, charDirectory);

                if (newConfig.Acquaintences == null)
                    Acquaintences = Enumerable.Empty<IAcquaintence>();
                else
                    Acquaintences = newConfig.Acquaintences;

                ConfigDataCache.Add(this);

                return true;
            }

            return false;
        }

        private void GetNotifications(DataAccess.FileSystem.ConfigData dataAccessor, DirectoryInfo charDirectory)
        {
            try
            {
                var files = charDirectory.EnumerateFiles("*.PlayerMessage", SearchOption.TopDirectoryOnly);

                var dataList = new List<IPlayerMessage>();
                foreach(var file in files)
                {
                    if (file == null)
                        continue;

                    var newMessage = (IPlayerMessage)dataAccessor.ReadEntity(file, typeof(IPlayerMessage));

                    if (newMessage != null)
                    {
                        ConfigDataCache.Add(newMessage);
                        dataList.Add(newMessage);
                    }
                }

                Notifications = dataList;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                //Let it keep going
            }
        }
    }
}
