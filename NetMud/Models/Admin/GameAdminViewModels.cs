﻿using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetMud.Models.Admin
{
    public class DashboardViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public DashboardViewModel()
        {
            Inanimates = Enumerable.Empty<IInanimateData>();
            Rooms = Enumerable.Empty<IRoomData>();
            NPCs = Enumerable.Empty<INonPlayerCharacter>();

            DimensionalModels = Enumerable.Empty<IDimensionalModelData>();
            HelpFiles = Enumerable.Empty<IHelp>();
            Materials = Enumerable.Empty<IMaterial>();
            Races = Enumerable.Empty<IRace>();
            Zones = Enumerable.Empty<IZone>();
            Constants = Enumerable.Empty<IConstants>();

            WebsocketServers = Enumerable.Empty<NetMud.Websock.Server>();

            LiveRooms = 0;
            LiveInanimates = 0;
            LiveNPCs = 0;

            LivePlayers = 0;
        }

        //Backing Data
        public IEnumerable<IRoomData> Rooms { get; set; }
        public IEnumerable<IInanimateData> Inanimates { get; set; }
        public IEnumerable<INonPlayerCharacter> NPCs { get; set; }

        //Lookup Data
        public IEnumerable<IDimensionalModelData> DimensionalModels { get; set; }
        public IEnumerable<IHelp> HelpFiles { get; set; }
        public IEnumerable<IMaterial> Materials { get; set; }
        public IEnumerable<IRace> Races { get; set; }
        public IEnumerable<IZone> Zones { get; set; }
        public IEnumerable<IConstants> Constants { get; set; }
        public IEnumerable<IFlora> Flora { get; set; }
        public IEnumerable<IFauna> Fauna { get; set; }
        public IEnumerable<IMineral> Minerals { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        public IEnumerable<NetMud.Websock.Server> WebsocketServers { get; set; }

        public int LiveRooms { get; set; }
        public int LiveInanimates { get; set; }
        public int LiveNPCs { get; set; }
        public int LivePlayers { get; set; }
    }
}