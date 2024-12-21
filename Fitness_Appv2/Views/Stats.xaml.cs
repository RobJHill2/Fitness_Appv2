using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fitness_Appv2.Views
{
    public partial class Stats : ContentPage
    {
        List<SetMediansTable> setsGraphSource;
        List<UserDataTable> bodyweightGraphSource;
        List<UserDataTable> consistencyGraphSource;

        List<string> userDataChoices;
        List<XcisesTable> xciseChoices;

        string graphTypeChoice;
        XcisesTable xciseChoice;
        string userDataChoice;

        public Stats()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            graphTypePicker.ItemsSource = new List<string>() { "Exercises", "User Data" }; // provides data setsGraphSource for picker

            setsGraphSource = await App.Db.GetSetsGraphDataAsync();
            bodyweightGraphSource = await App.Db.GetBodyweightGraphDataAsync();
            consistencyGraphSource = await App.Db.GetConsistencyGraphDataAsync();

            userDataChoices = new List<string>() { "Bodyweight", "Consistency" };
            xciseChoices = await App.Db.GetXcisesAsync();
        }
        private void GraphTypePicker_Changed (object sender, EventArgs e) 
        {
            graphTypeChoice = graphTypePicker.SelectedItem as string;

            if (graphTypeChoice == "Exercises")
            {
                variablePicker.ItemsSource = xciseChoices;
                variablePicker.ItemDisplayBinding = new Binding("XciseNameAttribute");
            }
            else if (graphTypeChoice == "User Data")
            {
                variablePicker.ItemsSource = userDataChoices;
                variablePicker.ItemDisplayBinding = null; 
            }
            variablePicker.IsEnabled = true;
        }
        private void VariablePicker_Changed (object sender, EventArgs e)
        {
            graphTypeChoice = graphTypePicker.SelectedItem as string;
            if (graphTypeChoice == "Exercises")
            {
                xciseChoice = variablePicker.SelectedItem as XcisesTable;
                if (xciseChoice != null)
                {
                    chartSeries.ItemsSource = setsGraphSource.Where(obj => obj.XciseIdAttribute == xciseChoice.Id);
                }
            }
            else if (graphTypeChoice == "User Data")
            {
                userDataChoice = variablePicker.SelectedItem as string;
                if (userDataChoice != null)
                {
                    if (userDataChoice == "Bodyweight")
                    {
                        chartSeries.ItemsSource = bodyweightGraphSource;
                    }
                    else if (userDataChoice == "Consistency")
                    {
                        chartSeries.ItemsSource = consistencyGraphSource;
                    }
                }
            }
        }
    }
} 