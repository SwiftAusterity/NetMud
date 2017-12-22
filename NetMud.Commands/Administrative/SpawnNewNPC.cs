﻿using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.Data.Game;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.SupportingClasses;

namespace NutMud.Commands.System
{
    /// <summary>
    /// Spawns a new NPC into the world. Missing target parameter = container you're standing in
    /// </summary>
    [CommandKeyword("SpawnNewNPC", false)]
    [CommandPermission(StaffRank.Admin)]
    [CommandParameter(CommandUsage.Subject, typeof(NetMud.Data.EntityBackingData.NonPlayerCharacter), new CacheReferenceType[] { CacheReferenceType.Data }, "[0-9]+", false)] //for IDs
    [CommandParameter(CommandUsage.Subject, typeof(NetMud.Data.EntityBackingData.NonPlayerCharacter), new CacheReferenceType[] { CacheReferenceType.Data }, "[a-zA-z]+", false)] //for names
    [CommandParameter(CommandUsage.Target, typeof(IContains), new CacheReferenceType[] { CacheReferenceType.Entity }, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class SpawnNewNPC : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public SpawnNewNPC()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var newObject = (INonPlayerCharacter)Subject;
            var sb = new List<string>();
            IContains spawnTo;

            //No target = spawn to room you're in
            if (Target != null)
                spawnTo = (IContains)Target;
            else
                spawnTo = OriginLocation;

            var entityObject = new Intelligence(newObject, spawnTo);

            //TODO: keywords is janky, location should have its own identifier name somehow for output purposes - DISPLAY short/long NAME
            sb.Add(string.Format("{0} spawned to {1}", entityObject.DataTemplateName, spawnTo.Keywords[0]));

            var toActor = new Message(MessagingType.Visible, 1);
            toActor.Override = sb;

            var toOrigin = new Message(MessagingType.Visible, 30);
            toOrigin.Override = new string[] { "$S$ appears suddenly." };

            var toSubject = new Message(MessagingType.Visible, 30);
            toSubject.Override = new string[] { "You are ALIVE" };

            var messagingObject = new MessageCluster(toActor);
            messagingObject.ToOrigin = new List<IMessage> { toOrigin };
            messagingObject.ToSubject = new List<IMessage> { toSubject };

            messagingObject.ExecuteMessaging(Actor, entityObject, spawnTo, OriginLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Valid Syntax: spawnNewNPC &lt;object name&gt;"));
            sb.Add("spawnNewNPC  &lt;NPC name&gt;  &lt;location name to spawn to&gt;".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("spawnNewNPC spawns a new NPC from its data template into the room or into a specified location.");
            }
            set { }
        }
    }
}
