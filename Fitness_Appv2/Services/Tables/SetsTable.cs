using System;

namespace Fitness_Appv2.Services
{
    abstract public class SetsTable:Table
    {
        public int XciseIdAttribute { get; set; } // FOREIGN KEY
        public DateTime DateAttribute { get; set; }
        public float E1RMaxAttribute { get; set; }
    }
}
