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
        private static Database database; // Initiates new Database object 'database'
        public static Database Database { get {  // Getter for 'database' object
            if (database == null){
                database = new Database(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FitnessAppv2.db3")); // creates database on disk
            }
                return database;
            }
        }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NCaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXdecnRRQmZeVEx2Vks=");
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
