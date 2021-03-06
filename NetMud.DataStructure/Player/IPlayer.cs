﻿using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.System;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// Player character + account entity class
    /// </summary>
    public interface IPlayer : IMobile, IPlayerFramework, ISpawnAsSingleton<IPlayer>
    {
        /// <summary>
        /// Function used to close the connection
        /// </summary>
        void CloseConnection();

        /// <summary>
        /// How this player is connected
        /// </summary>
        IDescriptor Descriptor { get; set; }
    }
}
