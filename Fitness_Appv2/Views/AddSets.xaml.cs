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
            int reps = Convert.ToInt16(repsInput.Text);
            float weight = Convert.ToSingle(weightInput.Text);
            DateTime date = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("Clicked: xcise = {0}; reps = {1}; weight = {2}", xcise, reps, weight);
            if ((!string.IsNullOrEmpty(xcise)) && (reps > 0) && (weight > 0)) //sanitisation
            {
                await App.Database.SaveTestDataAsync(new Services.testTable
                {
                    XciseAttribute = xcise,
                    RepsAttribute = reps,
                    WeightAttribute = weight,
                    DateAttribute = date
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
            // currently deletes oldest item in testTable
            await App.Database.DeleteTestDataAsync((await App.Database.GetTestItemAsync(1)));
            testDataView.ItemsSource = await App.Database.GetTestDataAsync();
        }
    }
}