using Fitness_Appv2.Services;
using System;
using Xamarin.Forms;
using System.IO;
namespace Fitness_Appv2
{
    public partial class App : Application
    {
        private static Database db; // Initiates new Db object 'db' as a property
        public static Database Db
        {
            get
            {
                if (db == null)
                {
                    db = new Database(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FitnessAppv2.db3")); // creates db on disk
                }
                return db;
            } // Getter for 'db' object
        }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NMaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWX5fcXVVRWFcUUNyXko=");
            // **** TEMPLATE CODE START ****
            InitializeComponent();
            MainPage = new AppShell();
            // **** TEMPLATE CODE END ****
        }
    }
}