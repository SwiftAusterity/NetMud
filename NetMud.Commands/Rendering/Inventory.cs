﻿using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.Data.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

namespace NetMud.Commands.Rendering
{
    [CommandKeyword("inventory", false, true, true)]
    [CommandKeyword("inv", false, false, true)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Inventory : CommandPartial
    {        
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Inventory()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();
            var chr = (IMobile)Actor;
            var toActor = new List<IMessage>();

            toActor.Add(
                new Message(MessagingType.Visible, new Occurrence() { Strength = 9999 })
                {
                    Override = new string[] { "You look through your belongings." }
                });

            foreach (var thing in chr.Inventory.EntitiesContained())
                toActor.Add(new Message(MessagingType.Visible, thing.RenderAsContents(chr, new[] { MessagingType.Visible })));

            var toOrigin = new Message(MessagingType.Visible, new Occurrence() { Strength = 30 })
            {
                Override = new string[] { "$A$ sifts through $G$ belongings." }
            };

            var messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: inventory",
                "inv".PadWithString(14, "&nbsp;", true)
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override MarkdownString HelpText
        {
            get
            {
                return string.Format("Inventory lists out all inanimates currently on your person. It is an undetectable action unless a viewer has high perception.");
            }
            set { }
        }

    }
}
