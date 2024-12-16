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
            DataView.ItemsSource = await App.Db.GetPendingSets();
            xcisePicker.ItemsSource = await App.Db.GetXciseNamesAsync();
        }
        private async void SubmitSet_ClickedAsync(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Submit Set") // this is to prevent double clicks registering twice
            {
                ((Button)sender).Text = "Submitting ...";
                inputObjection.IsVisible = false;
                XcisesTable item = xcisePicker.SelectedItem as XcisesTable;
                int xciseId = item.Id; // record Index instead?
                bool xciseIsBodyweight = await App.Db.GetIsBodyweightXciseAsync(xciseId);
                float reps = Convert.ToSingle(repsInput.Text);
                float weight;
                if (!xciseIsBodyweight) {
                    weight = Convert.ToSingle(weightInput.Text);
                }
                else
                {
                    UserDataTable userdata = (await App.Db.GetThisWeeksUserData());
                    if (userdata != null && userdata.BodyweightAttribute != 0)
                    {
                        weight = userdata.BodyweightAttribute + Convert.ToSingle(weightInput.Text);
                    } // If it is a bodyweight exercise, must add bodyweight to any additional weight
                    else
                    {
                        inputObjection.IsVisible = true;
                        inputObjection.Text = "This is a Bodyweight Exercise. Please record your bodyweight on the home page.";
                        await Task.Delay(1500);
                        ((Button)sender).Text = "Submit Set";
                        return;
                    }
                }
                
                if ((reps > 0) && (weight > 0 || xciseIsBodyweight)) //sanitisation
                {
                    DateTime date = DateTime.Today;
                    float e1RMax = Utilities.GetE1RMax(reps, weight);
                    App.Db.SaveSets(new PendingSetsTable
                    {
                        XciseIdAttribute = xciseId,
                        DateAttribute = date,
                        E1RMaxAttribute = e1RMax,
                        DailyMedianTaken = false,
                    }); // passes record obj into save method
                    DataView.ItemsSource = null;
                    DataView.ItemsSource = await App.Db.GetPendingSets();
                    repsInput.Text = weightInput.Text = "";
                    xcisePicker.SelectedIndex = -1;
                }
                else
                {
                    inputObjection.IsVisible = true;
                    inputObjection.Text = "Reps and Weight must be above 0 if it is not a Bodyweight Exercise";
                }
                await Task.Delay(1500);
                ((Button)sender).Text = "Submit Set";
            }
        }

        private async void TempButton_Clicked(object sender, EventArgs e)
        {
            // currently deletes all Routines
            await App.Db.CustomMethod();
            DataView.ItemsSource = await App.Db.GetPendingSets();
        }
    }
}