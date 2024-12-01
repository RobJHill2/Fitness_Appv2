using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    public class PendingSetsTable { 

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        // This is a property, {get; set;} is just shorthand for a method that retrieves/allocates the value of the property
        public int XciseIdAttribute { get; set; } // FOREIGN KEY
        public DateTime DateAttribute { get; set; }
        public float E1RMaxAttribute { get; set; }
        public bool DailyMedianTaken { get; set; } // need this since record needs to be kept after daily median taken
    }
}

