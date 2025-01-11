namespace Fitness_Appv2.Services
{
     public class PendingSetsTable:SetsTable 
    {
        public bool DailyMedianTakenAttribute { get; set; } 
        // need this since record needs to be kept after daily median taken for monthly median
    }
}

