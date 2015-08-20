﻿using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomData : IEntityBackingData
    {
        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }
    }
}
