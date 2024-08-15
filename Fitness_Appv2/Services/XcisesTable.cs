using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    public class XcisesTable { 

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string XciseNameAttribute { get; set; }
        public bool IsBodyweightAttribute { get; set; }
        public float PBAttribute { get; set; }
        public float GoalAttribute { get; set; }
    }
}

