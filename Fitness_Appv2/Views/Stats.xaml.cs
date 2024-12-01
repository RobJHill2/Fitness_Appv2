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
            picker.ItemsSource = await App.Db.GetXciseNamesAsync(); // provides data source 
        }
        private async void Picker_ChangedAsync(object sender, EventArgs e) 
        {
            XcisesTable item = picker.SelectedItem as XcisesTable; // casting from 'object' type to 'PendingSetsTable' so can access attributes
            List<SetMediansTable> source = new List<SetMediansTable>();
            source.AddRange(await App.Db.GetXciseDailyMediansAsync(item.Id));
            source.AddRange(await App.Db.GetXciseMonthlyMediansAsync(item.Id));
            chartSeries.ItemsSource = source;
        }
    }
}