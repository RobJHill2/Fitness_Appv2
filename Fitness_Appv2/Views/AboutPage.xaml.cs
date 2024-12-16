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
        public async void GenerateQuoteAsync()
        {
            StreamReader sr = new StreamReader("");
            string[] quotes = (await sr.ReadToEndAsync()).Split('\n');
            
            int index = (DateTime.Today.Year*DateTime.Today.Month*DateTime.Today.Day) % 1000; // hashing algorithm

            MotivationalQuote.Text = quotes[index];
        }
    }
}