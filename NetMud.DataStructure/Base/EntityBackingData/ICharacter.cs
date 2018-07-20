﻿using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    public interface ICharacter : IEntityBackingData, IDescribable, IGender
    {

        /// <summary>
        /// Account data object unique key
        /// </summary>
        string AccountHandle { get; set; }

        /// <summary>
        /// What account owns this character
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Command permissions for player character
        /// </summary>
        StaffRank GamePermissionsRank { get; set; }

        /// <summary>
        /// Family name for character
        /// </summary>     
        string SurName { get; set; }

        /// <summary>
        /// The race of this
        /// </summary>
        IRace RaceData { get; set; }

        /// <summary>
        /// Is this character not graduated from the tutorial
        /// </summary>
        bool StillANoob { get; set; }

        /// <summary>
        /// Sensory overrides for staff member characters
        /// </summary>
        IDictionary<MessagingType, bool> SuperSenses { get; set; }

        /// <summary>
        /// Last known location Id for character in live world
        /// </summary>
        IGlobalPosition CurrentLocation { get; set; }

        /// <summary>
        /// Given name + surname
        /// </summary>
        /// <returns></returns>
        string FullName();
    }
}
