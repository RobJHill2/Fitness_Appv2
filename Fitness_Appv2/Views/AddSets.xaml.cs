using Fitness_Appv2.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Fitness_Appv2.Views
{
    public partial class AddSets : ContentPage
    {
        public AddSets()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            DataView.ItemsSource = await App.Db.GetPendingSetsToViewAsync();
            xcisePicker.ItemsSource = await App.Db.GetXciseNamesAsync();
        }
        private async void SubmitSet_ClickedAsync(object sender, EventArgs e)
        {
            if (submitSet.Text == "Submit Set") // this is to prevent double clicks registering twice
            {
                submitSet.Text = "Submitting ...";
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
                    UserDataTable userdata = (await App.Db.GetThisWeeksUserDataAsync());
                    if (userdata != null && userdata.BodyweightAttribute != 0)
                    {
                        weight = userdata.BodyweightAttribute + Convert.ToSingle(weightInput.Text);
                    } // If it is a bodyweight exercise, must add bodyweight to any additional weight
                    else
                    {
                        inputObjection.IsVisible = true;
                        inputObjection.Text = "This is a Bodyweight Exercise. Please record your bodyweight on the home page.";
                        await Task.Delay(1500);
                        submitSet.Text = "Submit Set";
                        return;
                    }
                }
                
                if ((reps > 0) && (weight > 0)) //sanitisation
                {
                    DateTime date = DateTime.Today;
                    float e1RMax = Utilities.GetE1RMax(reps, weight);
                    await App.Db.SaveSetAsync(new PendingSetsTable
                    {
                        XciseIdAttribute = xciseId,
                        DateAttribute = date,
                        E1RMaxAttribute = e1RMax,
                        DailyMedianTakenAttribute = false,
                    }); // passes record obj into save method
                    DataView.ItemsSource = null;
                    DataView.ItemsSource = await App.Db.GetPendingSetsToViewAsync();
                    repsInput.Text = weightInput.Text = "";
                    xcisePicker.SelectedIndex = -1;
                }
                else
                {
                    inputObjection.IsVisible = true;
                    inputObjection.Text = "Reps and Weight must be above 0 if it is not a Bodyweight Exercise";
                    return;
                }
                await Task.Delay(1500);
                submitSet.Text = "Submit Set";
            }
        }
        private async void UndoSet_ClickedAsync(object sender, EventArgs e)
        {
            App.Db.UndoSetAsync();
            DataView.ItemsSource = null;
            DataView.ItemsSource = await App.Db.GetPendingSetsToViewAsync();
        }
        private async void RedoSet_ClickedAsync(object sender, EventArgs e)
        {
            App.Db.RedoSetAsync();
            DataView.ItemsSource = null;
            DataView.ItemsSource = await App.Db.GetPendingSetsToViewAsync();
        }
    }
}