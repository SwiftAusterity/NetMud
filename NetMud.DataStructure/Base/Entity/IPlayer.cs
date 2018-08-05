﻿using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Entity
{
    /// <summary>
    /// Player character + account entity class
    /// </summary>
    public interface IPlayer : IMobile, ISpawnAsSingleton<IPlayer>
    {
        /// <summary>
        /// Function used to close the connection
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// How this player is connected
        /// </summary>
        IDescriptor Descriptor { get; set; }

        /// <summary>
        /// Account this player belongs to
        /// </summary>
        string AccountHandle { get; set; }
    }
}
