using Fitness_Appv2.Services;
using Fitness_Appv2.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fitness_Appv2
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
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
