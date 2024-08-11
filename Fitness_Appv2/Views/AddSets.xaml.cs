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
            testDataView.ItemsSource = await App.Database.GetTestDataAsync();
        }
        private async void submitSet_Clicked(object sender, EventArgs e)
        {
            string xcise = xciseInput.Text;
            float reps = Convert.ToSingle(repsInput.Text);
            float weight = Convert.ToSingle(weightInput.Text);
            DateTime date = DateTime.Now;
            float e1RMax;
            if (reps > 1 && reps <= 7.5)
            {
                e1RMax = weight * (36 / (37 - reps));
            } 
            else
            {
                e1RMax = weight * Convert.ToSingle(Math.Pow(reps, 0.1));
            }
            // 1RM = w * (36/(37-r)) is the Brzyki Formula. It is best for 1 < r <= 7.5
            // 1RM = w * r^0.1 is the Lombardi Formula. It is best for r <= 1 U r > 7.5  
            System.Diagnostics.Debug.WriteLine("Clicked: xcise = {0}; reps = {1}; weight = {2}", xcise, reps, weight);
            if ((!string.IsNullOrEmpty(xcise)) && (reps > 0) && (weight > 0)) //sanitisation
            {
                await App.Database.SaveTestDataAsync(new Services.testTable
                {
                    XciseAttribute = xcise,
                    RepsAttribute = reps,
                    WeightAttribute = weight,
                    DateAttribute = date,
                    e1RMaxAttribute = e1RMax
                }); // passes record obj into save method
                testDataView.ItemsSource = await App.Database.GetTestDataAsync();
                ((Button)sender).Text = "Submitted";
                await Task.Delay(2000);
                ((Button)sender).Text = "Submit Set";
            }
        }
        private void clearInputs_Clicked(object sender, EventArgs e)
        {
            xciseInput.Text = repsInput.Text = weightInput.Text = "";
        }
        private async void tempButton_Clicked(object sender, EventArgs e)
        {
            // currently deletes all items in table
            await App.Database.CustomMethod();
            testDataView.ItemsSource = await App.Database.GetTestDataAsync();
        }
    }
}