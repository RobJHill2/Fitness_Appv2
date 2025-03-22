using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Fitness_Appv2.Views
{
    public partial class Stats : ContentPage
    {
        List<SetMediansTable> setsGraphSource;
        List<UserDataTable> bodyweightGraphSource;
        List<UserDataTable> consistencyGraphSource;

        List<string> userDataSecondaryOptions;
        List<XcisesTable> xciseSecondaryOptions;

        string primaryChoice;
        XcisesTable xciseSecondaryChoice;
        string userDataSecondaryChoice;

        public Stats()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            primaryPicker.ItemsSource = new List<string>() { "Exercises", "User Data" }; // provides data setsGraphSource for picker

            setsGraphSource = await App.Db.GetSetsGraphDataAsync();
            bodyweightGraphSource = await App.Db.GetBodyweightGraphDataAsync();
            consistencyGraphSource = await App.Db.GetConsistencyGraphDataAsync();

            userDataSecondaryOptions = new List<string>() { "Bodyweight", "Consistency" };
            xciseSecondaryOptions = await App.Db.GetXcisesAsync();
        }
        private void PrimaryPicker_Changed (object sender, EventArgs e) 
        {
            primaryChoice = primaryPicker.SelectedItem as string;

            if (primaryChoice == "Exercises")
            {
                secondaryPicker.ItemsSource = xciseSecondaryOptions;
                secondaryPicker.ItemDisplayBinding = new Binding("XciseNameAttribute");
            }
            else if (primaryChoice == "User Data")
            {
                secondaryPicker.ItemsSource = userDataSecondaryOptions;
                secondaryPicker.ItemDisplayBinding = null; 
            }
            secondaryPicker.IsEnabled = true;
        }
        private void SecondaryPicker_Changed (object sender, EventArgs e)
        {
            primaryChoice = primaryPicker.SelectedItem as string;
            if (primaryChoice == "Exercises")
            {
                xciseSecondaryChoice = secondaryPicker.SelectedItem as XcisesTable;
                if (xciseSecondaryChoice != null)
                {
                    chartSeries.ItemsSource = setsGraphSource.Where(obj => obj.XciseIdAttribute == xciseSecondaryChoice.Id);
                    chartSeries.YBindingPath = "E1RMaxAttribute";
                }
            }
            else if (primaryChoice == "User Data")
            {
                userDataSecondaryChoice = secondaryPicker.SelectedItem as string;
                if (userDataSecondaryChoice != null)
                {
                    if (userDataSecondaryChoice == "Bodyweight")
                    {
                        chartSeries.ItemsSource = bodyweightGraphSource;
                        chartSeries.YBindingPath = "BodyweightAttribute";
                    }
                    else if (userDataSecondaryChoice == "Consistency")
                    {
                        chartSeries.ItemsSource = consistencyGraphSource;
                        chartSeries.YBindingPath = "WeeklyConsistencyAttribute";
                    }
                }
            }
        }
    }
} 