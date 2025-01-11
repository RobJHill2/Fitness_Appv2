namespace Fitness_Appv2.Services
{
    public class RoutineComponentsTable:Table { 
        public int XciseIdAttribute { get; set; } // FOREIGN KEY
        public int SetsAttribute {  get; set; }
        public int RoutineAttribute { get; set; }  // FOREIGN KEY

    }
}

