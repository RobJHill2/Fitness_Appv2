using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    public class TestMediansTable { 
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string XciseAttribute { get; set; }
        public DateTime DateAttribute { get; set; }
        public float E1RMaxAttribute { get; set; }
        public bool IsDailyMedian { get; set; }
    }
}

