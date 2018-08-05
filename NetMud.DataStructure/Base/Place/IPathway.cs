﻿
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Base.Supporting;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Entity for Pathways
    /// </summary>
    public interface IPathway : IActor, ISpawnAsSingleton<IPathway>
    {
        /// <summary>
        /// Location this pathway leads to
        /// </summary>
        ILocation Destination { get; set; }

        /// <summary>
        /// Location this pathway spawns into and leads away from
        /// </summary>
        ILocation Origin { get; set; }

        /// <summary>
        /// Message cluster for entities entering
        /// </summary>
        IMessageCluster Enter { get; set; }

        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }

        /// <summary>
        /// Cardinality direction this points towards
        /// </summary>
        MovementDirectionType MovementDirection { get; set; }
    }
}
