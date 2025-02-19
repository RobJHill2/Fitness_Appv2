using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Fitness_Appv2.Views
{
    public partial class EditRoutine : ContentPage
    {
        List<DisplayComponentsDataModel> activeComponents;
        int currComponentIndex;
        int RoutineId;
        public EditRoutine(int inputRoutineId) 
        {
            InitializeComponent();
            RoutineId = inputRoutineId;
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ComponentEdits.IsVisible = false;
            xcisePicker.ItemsSource = await App.Db.GetXciseNamesAsync();
            if (RoutineId == 0)
            {
                activeComponents = new List<DisplayComponentsDataModel>() { new DisplayComponentsDataModel() { Id = 1 } };
            }
            else
            {
                RoutineName.Text = await App.Db.GetRoutineNameAsync(RoutineId);
                activeComponents = await App.Db.GetRoutineComponentsToEditAsync(RoutineId);
            }
            ComponentsView.ItemsSource = activeComponents;
        }
        private void NewComponent_Clicked(object sender, EventArgs e)
        {
            if (activeComponents.Count() == 0)
            {
                activeComponents.Add(new DisplayComponentsDataModel() { Id = 1 });
            }
            else
            {
                activeComponents.Add(new DisplayComponentsDataModel() { Id = activeComponents.Last().Id + 1 });
            }
            ComponentsView.ItemsSource = null; // Resets ItemsSource so Display notices change
            ComponentsView.ItemsSource = activeComponents;
        }
        private void ComponentsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currComponentIndex = activeComponents.IndexOf(e.CurrentSelection[0] as DisplayComponentsDataModel);
            ComponentEdits.IsVisible = true;
            xcisePicker.SelectedIndex = activeComponents[currComponentIndex].XciseIdAttribute - 1; // SelectedIndex starts from 0, XciseIdAttribute starts from 1 (SelectedIndex = -1 --> xcisePicker not set)
            if (activeComponents[currComponentIndex].SetsAttribute != 0)
            {
                setsInput.Text = Convert.ToString(activeComponents[currComponentIndex].SetsAttribute);
            }
            else
            {
                setsInput.Text = "";
            }
        }
        private void SaveComponent_Clicked(object sender, EventArgs e)
        {
            XcisesTable item = xcisePicker.SelectedItem as XcisesTable;
            int xciseId = item.Id;
            string xciseName = item.XciseNameAttribute;
            int sets = Convert.ToInt16(setsInput.Text);
            activeComponents[currComponentIndex].XciseIdAttribute = xciseId;
            activeComponents[currComponentIndex].XciseNameAttribute = xciseName;
            activeComponents[currComponentIndex].SetsAttribute = sets;
            ComponentsView.ItemsSource = null;
            ComponentsView.ItemsSource = activeComponents;
            ComponentEdits.IsVisible = false;
        }
        private void DeleteComponent_Clicked(object sender, EventArgs e)
        {
            activeComponents.RemoveAt(currComponentIndex);
            ComponentEdits.IsVisible = false;
            ComponentsView.ItemsSource = null;
            ComponentsView.ItemsSource = activeComponents;
        }
        private async void SaveRoutine_ClickedAsync(object sender, EventArgs e)
        {
            if (RoutineName.Text == null)
            {
                inputObjection.IsVisible = true;
                inputObjection.Text = "Routine Must Have A Name";
                return;
            }
            foreach (DisplayComponentsDataModel component in activeComponents)
            {
                if (component.SetsAttribute <= 0 || component.XciseIdAttribute == 0) 
                {
                    inputObjection.IsVisible = true;
                    inputObjection.Text = "Num. SetsAttribute must be above 0, You must choose an exercise for each component.";
                    return;
                }
            }
            inputObjection.IsVisible = false;

            if (RoutineId == 0) // i.e. if Routine is new 
            {
                RoutinesTable newRoutine = new RoutinesTable { NameAttribute = RoutineName.Text };
                RoutineId = await App.Db.SaveRoutineAsync(newRoutine);
            }
            else
            {
                App.Db.UpdateRoutineNameAsync(RoutineName.Text, RoutineId);
                App.Db.DeleteRoutineComponentsAsync(RoutineId); // clears previous version of routine
            }
            foreach (DisplayComponentsDataModel component in activeComponents)
            {
                component.Id = 0; // reset to 0 so autoIncrement is triggered (autoIncrement starts from 1)
                component.RoutineAttribute = RoutineId;
                App.Db.SaveRoutineComponentAsync(new RoutineComponentsTable() { RoutineAttribute = component.RoutineAttribute, SetsAttribute = component.SetsAttribute, XciseIdAttribute = component.SetsAttribute});
            }
            await Navigation.PopAsync();

        }


    }
}
