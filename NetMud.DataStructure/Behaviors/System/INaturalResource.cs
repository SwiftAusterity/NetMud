﻿using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.System
{
    /// <summary>
    /// Natural resources (minerals, flora, fauna)
    /// </summary>
    public interface INaturalResource : ILookupData
    {
        /// <summary>
        /// How much spawns in one place in one spawn tick
        /// </summary>
        int AmountMultiplier { get; set; }

        /// <summary>
        /// How rare this is to spawn even in its optimal range
        /// </summary>
        int Rarity { get; set; }

        /// <summary>
        /// How much the spawned puissance varies
        /// </summary>
        int PuissanceVariance { get; set; }

        /// <summary>
        /// On-gather affects that can be applied to the raw material
        /// </summary>
        HashSet<IAffect> PotentialAffects { get; set; }

        /// <summary>
        /// Spawns in elevations within this range
        /// </summary>
        Tuple<int, int> ElevationRange { get; set; }

        /// <summary>
        /// Spawns in temperatures within this range
        /// </summary>
        Tuple<int, int> TemperatureRange { get; set; }

        /// <summary>
        /// Spawns in humidities within this range
        /// </summary>
        Tuple<int, int> HumidityRange { get; set; }

        /// <summary>
        /// What medium materials this can spawn in
        /// </summary>
        HashSet<IMaterial> OccursIn { get; set; }

        /// <summary>
        /// Can spawn in system zones like non-player owned cities
        /// </summary>
        Boolean CanSpawnInSystemAreas { get; set; }
    }
}
