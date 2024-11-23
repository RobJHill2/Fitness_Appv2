using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Appv2.Services
{
    public class LogRoutineDataModel { 

        public int Id { get; set; }
        public int XciseId { get; set; }
        public int Sets { get; set; }
        public List<SetLogDataModel> SetsList { get; set; }

    }
}

