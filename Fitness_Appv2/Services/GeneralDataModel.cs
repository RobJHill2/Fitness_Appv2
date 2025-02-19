namespace Fitness_Appv2.Services
{
    abstract public class GeneralDataModel { 
        // serves as a template for display data models that are required because the table contains xciseId (which needs to be translated to an xciseName)
        public int Id { get; set; }
        public int XciseIdAttribute { get; set; }
        public string XciseNameAttribute { get; set; }
    }
}