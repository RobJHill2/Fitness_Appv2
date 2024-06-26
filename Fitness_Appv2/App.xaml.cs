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
                database = new Database(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FitnessAppv2.db3"));
            }
                return database;
            }
        }

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            System.Diagnostics.Debug.WriteLine("Hello World");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
