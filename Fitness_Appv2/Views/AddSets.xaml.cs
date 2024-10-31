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
    [XamlCompilation(XamlCompilationOptions.Compile)] // required for collection view in XAML file
    public partial class AddSets : ContentPage
    {
        public AddSets()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DataView.ItemsSource = await App.Database.GetSetsDataAsync();
            xcisePicker.ItemsSource = await App.Database.GetXciseNamesAsync();
        }
        private async void SubmitSet_ClickedAsync(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Submit Set") // this is to prevent double clicks registering twice
            {
                XcisesTable item = xcisePicker.SelectedItem as XcisesTable;
                int xciseId = item.Id; // record Id instead?
                float reps = Convert.ToSingle(repsInput.Text);
                float weight;
                if (!item.IsBodyweightAttribute) {
                    weight = Convert.ToSingle(weightInput.Text);
                }
                else
;                {
                    UserDataTable userdata = (await App.Database.GetLatestUserDataAsync());
                    if (userdata != null)
                    {
                        weight = (await App.Database.GetLatestUserDataAsync()).BodyweightAttribute + Convert.ToSingle(weightInput.Text);
                    } // If it is a bodyweight exercise, must add bodyweight to any additional weight
                    else
                    {
                        inputObjection.Text = "This is a Bodyweight Exercise. Please record your bodyweight on the home page.";
                        return;
                    }
                }
                DateTime date = DateTime.Today;
                System.Diagnostics.Debug.WriteLine("Clicked: xcise = {0}; reps = {1}; weight = {2}", xciseId, reps, weight);
                if ((reps > 0) && ((weight > 0) | (weight >= 0 && item.IsBodyweightAttribute))) //sanitisation
                {
                    ((Button)sender).Text = "Submitted";
                    float e1RMax;
                    if ( 1 <= reps && reps < 7.614) { e1RMax = weight * 36 / (37 - reps); }
                                               else { e1RMax = weight * Convert.ToSingle(Math.Pow(reps, 0.1)); }
                    // 1RM = w * (36/(37-r)) is the Brzyki Formula. It is more accurate* for 1 <= r < 7.614
                    // 1RM = w * r^0.1 is the Lombardi Formula. It is more accurate* for r < 1 U r >= 7.614
                    // 7.614 and 1 are the intersections between the graphs, chosen as these ranges match my personal data and also avoiding jumps in e1RMax
                    await App.Database.SaveSets(new SetsTable
                    {
                        XciseIdAttribute = xciseId,
                        DateAttribute = date,
                        E1RMaxAttribute = e1RMax,
                        DailyMedianTaken = false,
                    }); // passes record obj into save method
                    DataView.ItemsSource = await App.Database.GetSetsDataAsync();
                    
                    await Task.Delay(1500);
                    ((Button)sender).Text = "Submit Set";
                }
                else
                {
                    inputObjection.Text = "Reps and Weight must be above 0 if it is not a Bodyweight Exercise";
                }
            }
        }
        private void ClearInputs_Clicked(object sender, EventArgs e)
        {
            repsInput.Text = weightInput.Text = "";
        }
        private async void TempButton_Clicked(object sender, EventArgs e)
        {
            // currently deletes all
            await App.Database.CustomMethod();
            DataView.ItemsSource = await App.Database.GetSetsDataAsync();
        }
    }
}