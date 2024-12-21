using Fitness_Appv2.Services;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Fitness_Appv2.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            viewPinned.ItemsSource = await App.Db.GetPinnedXcisesAsync();
        }
    }
}