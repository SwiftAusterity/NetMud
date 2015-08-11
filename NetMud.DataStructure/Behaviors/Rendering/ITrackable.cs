﻿using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Indicates an entity's movement can be tracked
    /// </summary>
    public interface ITrackable
    {
        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        IEnumerable<string> RenderToTrack(IEntity actor);
    }
}
