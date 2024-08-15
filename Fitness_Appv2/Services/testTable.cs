using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    public class TestTable { 

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string XciseAttribute { get; set; }
        // This is a property, {get; set;} is just shorthand for a method that retrieves/allocates the value of the property
        public float RepsAttribute { get; set; } // is float in case user wants to record half reps
        public float WeightAttribute { get; set; }
        public DateTime DateAttribute { get; set; }
        public float E1RMaxAttribute { get; set; }
        public bool DailyMedianTaken { get; set; }
    }
}

