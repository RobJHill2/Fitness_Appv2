using System.Collections.Generic;

namespace Fitness_Appv2.Services
{
    public class LogRoutineComponentDataModel:GeneralDataModel 
    { 
        public int SetsAttribute { get; set; }
        public List<SetLogDataModel> SetsList { get; set; }
    }
}

