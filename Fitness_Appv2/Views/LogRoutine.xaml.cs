using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Fitness_Appv2.Views
{
    public partial class LogRoutine : ContentPage
    {
        List<LogRoutineComponentDataModel> activeComponents;
        int currComponentIndex;
        int currSetIndex;
        readonly int RoutineId;
        public LogRoutine(int inputRoutineId) 
        {
            InitializeComponent();
            RoutineId = inputRoutineId;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            xcisePicker.ItemsSource = await App.Db.GetXciseNamesAsync();
            activeComponents = await App.Db.GetRoutineComponentsToLogAsync(RoutineId);
            ComponentsView.ItemsSource = activeComponents;
        }
        private void NewComponent_Clicked(object sender, EventArgs e)
        {
            if (activeComponents.Count() == 0)
            {
                activeComponents.Add(new LogRoutineComponentDataModel() { Id = 1 });
            } else
            {
                activeComponents.Add(new LogRoutineComponentDataModel() { Id = activeComponents.Last().Id + 1 });
            }
            ComponentsView.ItemsSource = null;
            ComponentsView.ItemsSource = activeComponents;
        }
        private void ComponentsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currComponentIndex = activeComponents.IndexOf(e.CurrentSelection[0] as LogRoutineComponentDataModel);
            ComponentsEdit.IsVisible = true;
            xcisePicker.SelectedIndex = activeComponents[currComponentIndex].XciseIdAttribute - 1;
            if (activeComponents[currComponentIndex].SetsAttribute != 0)
            {
                setsInput.Text = Convert.ToString(activeComponents[currComponentIndex].SetsAttribute);
            }
            else
            {
                setsInput.Text = "";
            }
        }

        private void DeleteComponent_Clicked(object sender, EventArgs e)
        {
            activeComponents.RemoveAt(currComponentIndex); // tempId not changing when delete therefore transition to index based
            ComponentsEdit.IsVisible = false;
            ComponentsView.ItemsSource = null;
            ComponentsView.ItemsSource = activeComponents;
        }
        private void Continue_Clicked(object sender, EventArgs e)
        {
            if (setsInput.Text != "")
            {
                bool setsChanged = false; 
                AlterRoutine.IsVisible = false;
                SetsData.IsVisible = true;
                if (activeComponents[currComponentIndex].SetsAttribute != Convert.ToInt16(setsInput.Text))
                {
                    setsChanged = true;
                    activeComponents[currComponentIndex].SetsAttribute = Convert.ToInt16(setsInput.Text);
                }
                activeComponents[currComponentIndex].XciseIdAttribute = xcisePicker.SelectedIndex + 1;
                activeComponents[currComponentIndex].XciseNameAttribute = (xcisePicker.SelectedItem as XcisesTable).XciseNameAttribute;
                if (activeComponents[currComponentIndex].SetsList == null || setsChanged == true)
                {
                    List<SetLogDataModel> expectedInputs = new List<SetLogDataModel>();
                    for (int i = 0; i < activeComponents[currComponentIndex].SetsAttribute; i++)
                    {
                        expectedInputs.Add(new SetLogDataModel { Index = i });
                    }
                    activeComponents[currComponentIndex].SetsList = expectedInputs;
                }
                SetsView.ItemsSource = activeComponents[currComponentIndex].SetsList;
            }
        }

        private void SetsView_SelectionChanged (object sender, SelectionChangedEventArgs e)
        {
            currSetIndex = (e.CurrentSelection[0] as SetLogDataModel).Index;
            SetEdit.IsVisible = true;
            if (activeComponents[currComponentIndex].SetsList[currSetIndex].Reps != 0)
            {
                repsInput.Text = Convert.ToString(activeComponents[currComponentIndex].SetsList[currSetIndex].Reps);
            }
            else
            {
                repsInput.Text = "";
            }
            if (activeComponents[currComponentIndex].SetsList[currSetIndex].Weight != 0)
            {
                weightInput.Text = Convert.ToString(activeComponents[currComponentIndex].SetsList[currSetIndex].Weight);
            }
            else
            {
                weightInput.Text = "";
            }
        }

        private void SaveSet_Clicked(object sender, EventArgs e)
        {
            float reps;
            if (repsInput.Text == "") { reps = 0; } else { reps = Convert.ToSingle(repsInput.Text); };
            float weight;
            if (weightInput.Text == "") { weight = 0; } else { weight = Convert.ToSingle(weightInput.Text); };
            activeComponents[currComponentIndex].SetsList[currSetIndex].Reps = reps;
            activeComponents[currComponentIndex].SetsList[currSetIndex].Weight = weight;
            SetsView.ItemsSource = null;
            SetsView.ItemsSource = activeComponents[currComponentIndex].SetsList;
            SetEdit.IsVisible = false;
        }
        private void EditFinished_Clicked(object sender, EventArgs e)
        {
            SetsData .IsVisible = false;
            AlterRoutine.IsVisible = true;
            ComponentsView.ItemsSource = null;
            ComponentsView.ItemsSource = activeComponents;
            if (activeComponents[currComponentIndex].SetsAttribute != 0)
            {
                setsInput.Text = Convert.ToString(activeComponents[currComponentIndex].SetsAttribute);
            }
            else
            {
                setsInput.Text = "";
            }
            repsInput.Text = weightInput.Text = "";
        }

        private async void LogWorkout_ClickedAsync(object sender, EventArgs e)
        {
            DateTime date = DateTime.Today;
            UserDataTable userdata = await App.Db.GetThisWeeksUserDataAsync();
            // sanitisation
            foreach (LogRoutineComponentDataModel component in activeComponents)
            {
                bool isBodyweight = await App.Db.GetIsBodyweightXciseAsync(component.XciseIdAttribute);
                
                if (isBodyweight && userdata.BodyweightAttribute == 0)
                {
                    inputObjection.IsVisible = true;
                    inputObjection.Text = "Bodyweight Exercise Inputed. Please record your bodyweight on the home page first.";
                    return;
                }
                if (component.SetsAttribute <= 0 || component.XciseIdAttribute == 0)
                {
                    inputObjection.IsVisible = true;
                    inputObjection.Text = "Workout Data Incomplete";
                    return;
                }
               
                foreach (SetLogDataModel set in component.SetsList)
                {
                    if (set.Reps <= 0 || (set.Weight <= 0 && !isBodyweight))
                    {
                        inputObjection.IsVisible = true;
                        inputObjection.Text = "Workout Data Incomplete";
                        return;
                    }
                }
            }
            inputObjection.IsVisible = false;

            // log sets
            foreach (LogRoutineComponentDataModel component in activeComponents)
            {
                bool isBodyweight = await App.Db.GetIsBodyweightXciseAsync(component.XciseIdAttribute);
                foreach (SetLogDataModel set in component.SetsList)
                {
                    float weight;
                    if (isBodyweight) { weight = set.Weight + userdata.BodyweightAttribute; }
                    else { weight = set.Weight; }
                    float reps = set.Reps;
                    float e1RMax = Utilities.GetE1RMax(reps, weight);

                    App.Db.SaveSetAsync(new PendingSetsTable
                    {
                        XciseIdAttribute = component.XciseIdAttribute,
                        DateAttribute = date,
                        E1RMaxAttribute = e1RMax,
                        DailyMedianTakenAttribute = false,
                    });
                }
             }
            await Navigation.PopAsync();
        }


    }
}

