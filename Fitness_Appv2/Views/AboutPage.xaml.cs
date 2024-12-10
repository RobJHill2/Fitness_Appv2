using Fitness_Appv2.Services;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Fitness_Appv2.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            DisplayUserDataAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            viewPinned.ItemsSource = await App.Db.GetPinnedXcisesAsync();
        }

        public async void DisplayUserDataAsync()
        {
            List<UserDataTable> userdata = await App.Db.GetUserDataAsync();
            UserDataTable latest = userdata[0];
            if (latest != null || latest.BodyweightAttribute == 0)
            {
                BodyweightDisplay.Text = "Current Bodyweight: \n" + latest.BodyweightAttribute;
            }
            else
            {
                BodyweightDisplay.Text = "Current Bodyweight: \nNot Set";
            }
            DateTime lastMonday = Utilities.GetLastMonday(DateTime.Today);
            List<UserDataTable> userdataLastWeek = userdata.Where(obj =>
            (obj.DateAttribute >= lastMonday.AddDays(-7)) && (obj.DateAttribute < lastMonday)).ToList();
            if (userdataLastWeek.Count == 0)
            {
                ConsistencyDisplay.Text = "0";
            }
            else
            {
                ConsistencyDisplay.Text = Convert.ToString(userdataLastWeek[0].WeeklyConsistencyAttribute);
            }

        }

        public async void GenerateQuoteAsync()
        {
            StreamReader sr = new StreamReader("");
            string[] quotes = (await sr.ReadToEndAsync()).Split('\n');
            
            int index = (DateTime.Today.Year*DateTime.Today.Month*DateTime.Today.Day) % 1000; // hashing algorithm

            MotivationalQuote.Text = quotes[index];
        }

        private async void BodyweightUpdate_Pressed(object sender, EventArgs e) // input string not in right format
        {
            float bodyweight = Convert.ToSingle(BodyweightInput.Text);
            if (bodyweight > 0) { 
                var userdata = await App.Db.GetLatestUserDataAsync() ;
                if (userdata != null)
                {
                    if (userdata.DateAttribute < Utilities.GetLastMonday(DateTime.Today))
                    {
                        // if no record this week
                        App.Db.SaveUserData(new UserDataTable
                        {
                            BodyweightAttribute = bodyweight,
                            WeeklyConsistencyAttribute = userdata.WeeklyConsistencyAttribute,
                            DateAttribute = Utilities.GetLastMonday(DateTime.Today)
                        });
                    }
                    else
                    {
                        // if record this week
                        App.Db.UpdateBodyweightAsync(bodyweight, userdata.Id);
                    }
                }
                else 
                {
                    App.Db.SaveUserData(new UserDataTable {
                        BodyweightAttribute = bodyweight,
                        WeeklyConsistencyAttribute = 0,
                        DateAttribute = Utilities.GetLastMonday(DateTime.Today)});
                }
                BodyweightInput.Text = "";
                DisplayUserDataAsync();
            }
        }
    }
}