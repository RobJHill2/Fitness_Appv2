 using System;

namespace Fitness_Appv2.Services
{
    public class UserDataTable:Table { 
        public DateTime DateAttribute { get; set; }
        public float BodyweightAttribute { get; set; }
        public int WeeklyConsistencyAttribute { get; set; }
        public int WeeklyConsistencyGoalAttribute { get; set; }
    }
}

