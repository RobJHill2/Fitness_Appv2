using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fitness_Appv2.Views
{
    public partial class Routines : ContentPage
    {
        public Routines()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }
        private async void NewRoutine_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DefineNewRoutine());
        }


    }
}
