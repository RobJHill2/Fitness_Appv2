using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fitness_Appv2.Views
{
    public partial class Stats : ContentPage
    {
        public Stats()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            picker.ItemsSource = await App.Database.GetXciseNamesAsync(); // provides data source 
        }
        private async void Picker_ChangedAsync(object sender, EventArgs e) 
        {
            XcisesTable item = picker.SelectedItem as XcisesTable; // casting from 'object' type to 'SetsTable' so can access attributes
            chartSeries.ItemsSource = await App.Database.GetXciseMediansAsync(item.Id);
        }
    }
}