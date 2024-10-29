using Fitness_Appv2.Services;
using Fitness_Appv2.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using System.IO;
namespace Fitness_Appv2
{
    public partial class App : Application
    {
        private static Database database; // Initiates new Database object 'database' as a property
        public static Database Database
        {
            get
            {
                if (database == null)
                {
                    database = new Database(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FitnessAppv2.db3")); // creates database on disk
                }
                return database;
            } // Getter for 'database' object
        }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH9fcXVVR2RYVURxXUE=");
            System.Diagnostics.Debug.WriteLine("Hello World");
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
