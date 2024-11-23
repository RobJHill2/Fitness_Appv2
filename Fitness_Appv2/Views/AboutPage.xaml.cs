using Fitness_Appv2.Services;
using System;
using System.ComponentModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fitness_Appv2.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            DisplayBodyweightAsync();
        }
        public async void DisplayBodyweightAsync()
        {
            var userdata = await App.Db.GetLatestUserDataAsync();
            if (userdata != null)
            {
                BodyweightDisplay.Text = "Current Bodyweight: \n" + userdata.BodyweightAttribute;
            }
            else
            {
                BodyweightDisplay.Text = "Current Bodyweight: \nNot Set";
            }
        }

        private async void BodyweightUpdate_Pressed(object sender, EventArgs e) // input string not in right format
        {
            float bodyweight = Convert.ToSingle(BodyweightInput.Text);
            if (bodyweight > 0) { 
                var userdata = await App.Db.GetLatestUserDataAsync();
                if (userdata != null)
                {
                    float consistency = userdata.ConsistencyAttribute;
                    await App.Db.SaveUserData(new UserDataTable { 
                        BodyweightAttribute = bodyweight,
                        ConsistencyAttribute = consistency,
                        DateAttribute = DateTime.Today
                        });
                }
                else {
                    await App.Db.SaveUserData(new UserDataTable {
                        BodyweightAttribute = bodyweight,
                        ConsistencyAttribute = 0,
                        DateAttribute = DateTime.Today});
                }
                BodyweightInput.Text = "";
                DisplayBodyweightAsync();

            }
        }
    }
}