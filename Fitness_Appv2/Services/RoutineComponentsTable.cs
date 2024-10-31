using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    public class RoutineComponentsTable { 

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int XciseIdAttribute { get; set; } // FOREIGN KEY
        public int SetsAttribute {  get; set; }
        public int RoutineAttribute { get; set; }  // FOREIGN KEY

    }
}

