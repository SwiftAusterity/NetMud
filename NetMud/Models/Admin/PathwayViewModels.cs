﻿using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace NetMud.Models.Admin
{
    public class AddEditPathwayDataViewModel : TwoDimensionalEntityEditViewModel
    {
        public AddEditPathwayDataViewModel()
        {
            ValidModels = Enumerable.Empty<IDimensionalModelData>();
            ValidMaterials = Enumerable.Empty<IMaterial>();
            ValidRooms = Enumerable.Empty<IRoomData>();
            NewRoomModel = new AddEditRoomDataViewModel();
        }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Name")]
        [DataType(DataType.Text)]
        public string NewName { get; set; }

        [Range(0, 16, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Visible Strength")]
        [DataType(DataType.Text)]
        public int VisibleStrength { get; set; }

        [Range(0, 16, ErrorMessage = "The {0} must be between {2} and {1}.")] 
        [Display(Name = "Audible Strength")]
        [DataType(DataType.Text)]
        public int AudibleStrength { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Visible message")]
        [DataType(DataType.Text)]
        public string VisibleToSurroundings { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]       
        [Display(Name = "Audible message")]
        [DataType(DataType.Text)]
        public string AudibleToSurroundings { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]  
        [Display(Name = "Message to Destination")]
        [DataType(DataType.Text)]
        public string MessageToDestination { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]     
        [Display(Name = "Message to Origin")]
        [DataType(DataType.Text)]
        public string MessageToOrigin { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]      
        [Display(Name = "Message to Actor")]
        [DataType(DataType.Text)]
        public string MessageToActor { get; set; }
        
        [Display(Name = "To Room")]
        [DataType(DataType.Text)]
        public long DestinationID { get; set; }

        [Display(Name = "From Room")]
        [DataType(DataType.Text)]
        public long OriginID { get; set; }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        public IEnumerable<IRoomData> ValidRooms { get; set; }
        public IRoomData Origin { get; set; }
        public IRoomData Destination { get; set; }
        public IPathwayData DataObject { get; set; }

        public AddEditRoomDataViewModel NewRoomModel { get; set; }
    }
}