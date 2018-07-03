﻿using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NutMud.Commands.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Commands.Comm
{
    [CommandKeyword("gossip", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), new CacheReferenceType[] { CacheReferenceType.Text }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Gossip : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Gossip()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>
            {
                string.Format("You gossip '{0}'", Subject)
            };

            var toActor = new Message(MessagingType.Audible, new Occurrence() { Strength = 1 })
            {
                Override = sb
            };

            //TODO: language outputs
            var messagingObject = new MessageCluster(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);

            var gossipClient = LiveCache.Get<IGossipClient>("GossipWebClient");

            var userName = Actor.DataTemplateName;

            if (Actor.GetType().GetInterfaces().Contains(typeof(IPlayer)))
                userName = ((IPlayer)Actor).AccountHandle;

            gossipClient.SendMessage(userName, Subject.ToString());
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: gossip &lt;text&gt;",
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("Gossip allows you to speak over the gossip inter-mud network.");
            }
            set { }
        }
    }
}
