﻿using NetMud.Commands.Attributes;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("drop", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IEntity), new CacheReferenceType[] { CacheReferenceType.Entity }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Drop : CommandPartial, IHelpful
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Drop()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();
            var thing = (IEntity)Subject;
            var actor = (IContains)Actor;
            IContains place = (IContains)OriginLocation;

            actor.MoveFrom(thing);
            place.MoveInto(thing);

            sb.Add("You drop $S$.");

            var messagingObject = new MessageCluster(RenderUtility.EncapsulateOutput(sb), string.Empty, string.Empty, "$A$ drops $S$.", string.Empty);

            messagingObject.ExecuteMessaging(Actor, thing, null, OriginLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: look");
            sb.Add("look &lt;target&gt;".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// Renders the help text
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Look provides useful information about the location you are in or a target object or mobile."));

            return sb;
        }
    }
}
