using System;

namespace Fitness_Appv2.Services
{
    public class DisplaySetsDataModel:GeneralDataModel 
    { 
        public float E1RMaxAttribute { get; set; }
        public DateTime DateAttribute { get; set; }
        public bool DailyMedianTakenAttribute { get; set; }
    }
}