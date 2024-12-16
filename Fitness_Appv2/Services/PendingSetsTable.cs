using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
     public class PendingSetsTable:SetsTable 
    { 
        public bool DailyMedianTaken { get; set; } // need this since record needs to be kept after daily median taken
    }
}

