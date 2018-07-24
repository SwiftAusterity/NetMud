﻿using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Rendering methods for when a location that contains the entity being rendered is being rendered
    /// </summary>
    public interface IRenderInLocation
    {
        /// <summary>
        /// Renders output for this entity when Look targets the container it is in
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="sensoryTypes">What senses to include. </param>
        /// <returns>the output</returns>
        IOccurrence RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes);

        /// <summary>
        /// A fully described short description (includes adjectives)
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="sensoryTypes">What senses to include. EMPTY/NULL = ALL</param>
        /// <returns>the output</returns>
        IOccurrence GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes);

        /// <summary>
        /// A fully described short description (includes adjectives)
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <param name="sensoryTypes">What senses to include. EMPTY/NULL = ALL</param>
        /// <returns>the output</returns>
        IOccurrence GetImmediateDescription(IEntity viewer, MessagingType sense);

        /// <summary>
        /// The name of a thing based on visual description
        /// </summary>
        /// <param name="viewer">Who is looking</param>
        /// <param name="sensoryTypes">What senses to include. EMPTY/NULL = ALL</param>
        /// <returns>a string of the name</returns>
        string GetDescribableName(IEntity viewer);
    }
}
