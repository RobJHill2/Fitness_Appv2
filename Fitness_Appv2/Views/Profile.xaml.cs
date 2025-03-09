using Fitness_Appv2.Services;
using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Fitness_Appv2.Views
{
    public partial class Profile : ContentPage
    {
        public Profile()
        {
            InitializeComponent();         
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            DisplayUserDataAsync();
        }

        public async void DisplayUserDataAsync()
        {
            List<UserDataTable> userdata = await App.Db.GetUserDataAsync(); // get user data orders by date descending
            UserDataTable latest = userdata[0];

            // no need to check for null as CheckNeedNewUserData is called(sets bodyweight &goal = 0 on null)
                if (latest.BodyweightAttribute != 0)
                {
                    BodyweightDisplay.Text = "Current Bodyweight: \n" + latest.BodyweightAttribute;
                }
                else
                {
                    BodyweightDisplay.Text = "Current Bodyweight: \nNot Set";
                }
            if (latest.WeeklyConsistencyGoalAttribute != 0)
            {
                ConsistencyGoalDisplay.Text = "Current Goal: \n" + latest.WeeklyConsistencyGoalAttribute;
            }
            else
            {
                ConsistencyGoalDisplay.Text = "Current Goal: \nNot Set";
            }

            DateTime lastMonday = Utilities.GetLastMonday();
            UserDataTable userdataLastWeek;
            try
            {
                userdataLastWeek = userdata.Where(obj => obj.DateAttribute == lastMonday.AddDays(-7)).ToList()[0];
            }
            catch
            {
                userdataLastWeek = null;
            }

            if (userdataLastWeek != null)
            {
                if (userdataLastWeek.WeeklyConsistencyGoalAttribute != 0)
                {
                    ConsistencyDisplay.Text = "Last Week's Consistency: \n" + userdataLastWeek.WeeklyConsistencyAttribute
                    + " out of " + userdataLastWeek.WeeklyConsistencyGoalAttribute;
                }
                else
                {
                    ConsistencyDisplay.Text = "Last Week's Consistency: \n" + userdataLastWeek.WeeklyConsistencyAttribute;
                }
            }
            else
            {
                if (userdataLastWeek != null && userdataLastWeek.WeeklyConsistencyGoalAttribute != 0)
                {
                    ConsistencyDisplay.Text = "Last Week's Consistency: \n0" + " out of " + userdataLastWeek.WeeklyConsistencyGoalAttribute;
                }
                else
                {
                    ConsistencyDisplay.Text = "Last Week's Consistency: \n0";
                }
            }

        }


        private void BodyweightUpdate_Clicked(object sender, EventArgs e) // input string not in right format
        {
            float bodyweight = Convert.ToSingle(BodyweightInput.Text);
            if (bodyweight > 0) 
            { 
                App.Db.UpdateBodyweightThisWeekAsync(bodyweight);

                BodyweightInput.Text = "";
                DisplayUserDataAsync();
            }             
        }
        
        private void ConsistencyGoalUpdate_Clicked(object sender, EventArgs e) // input string not in right format
        {
            int consistencyGoal = Convert.ToInt16(ConsistencyGoalInput.Text);
            if (consistencyGoal > 0)
            {
                App.Db.UpdateConsistencyGoalThisWeekAsync(consistencyGoal);

                ConsistencyGoalInput.Text = "";
                DisplayUserDataAsync();
            }
        }
    }
}