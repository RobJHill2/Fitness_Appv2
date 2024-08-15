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
            testDataView.ItemsSource = await App.Database.GetTestMediansAsync();
            xcisePicker.ItemsSource = await App.Database.GetXciseNamesAsync();
        }
        private async void SubmitSet_ClickedAsync(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Submit Set") // this is to prevent double clicks registering twice
            {
                XcisesTable item = xcisePicker.SelectedItem as XcisesTable;
                string xcise = item.XciseNameAttribute;
                float reps = Convert.ToSingle(repsInput.Text);
                float weight = Convert.ToSingle(weightInput.Text);
                DateTime date = DateTime.Today;
                System.Diagnostics.Debug.WriteLine("Clicked: xcise = {0}; reps = {1}; weight = {2}", xcise, reps, weight);
                if ((reps > 0) && (weight > 0)) //sanitisation
                {
                    ((Button)sender).Text = "Submitted";
                    float e1RMax;
                    if ( 1 <= reps && reps < 7.614) { e1RMax = weight * 36 / (37 - reps); }
                                               else { e1RMax = weight * Convert.ToSingle(Math.Pow(reps, 0.1)); }
                    // 1RM = w * (36/(37-r)) is the Brzyki Formula. It is more accurate* for 1 <= r < 7.614
                    // 1RM = w * r^0.1 is the Lombardi Formula. It is more accurate* for r < 1 U r >= 7.614
                    // * Tested with personal workout data
                    await App.Database.SaveTestDataAsync(new TestTable
                    {
                        XciseAttribute = xcise,
                        RepsAttribute = reps,
                        WeightAttribute = weight,
                        DateAttribute = date,
                        E1RMaxAttribute = e1RMax,
                        DailyMedianTaken = false,
                    }); // passes record obj into save method
                    testDataView.ItemsSource = await App.Database.GetTestDataAsync();
                    
                    await Task.Delay(1500);
                    ((Button)sender).Text = "Submit Set";
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
            testDataView.ItemsSource = await App.Database.GetTestDataAsync();
        }

        private void xcisePicker_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}