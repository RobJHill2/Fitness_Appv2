using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Fitness_Appv2.Views
{
    public partial class EditRoutine : ContentPage
    {
        List<RoutineComponentsTable> activeComponents;
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
            ComponentsEdit.IsVisible = false;
            xcisePicker.ItemsSource = await App.Db.GetXciseNamesAsync();
            RoutineName.Text = await App.Db.GetRoutineNameAsync(RoutineId);
            if (RoutineId == 0)
            {
                activeComponents = new List<RoutineComponentsTable>() { new RoutineComponentsTable() { Id = 1 } };
            }
            else
            {
                activeComponents = await App.Db.GetRoutineComponentsAsync(RoutineId);
            }
            ComponentsView.ItemsSource = activeComponents;
        }
        private void NewComponent_Clicked(object sender, EventArgs e)
        {
            if (activeComponents.Count() == 0)
            {
                activeComponents.Add(new RoutineComponentsTable() { Id = 1 });
            }
            else
            {
                activeComponents.Add(new RoutineComponentsTable() { Id = activeComponents.Last().Id + 1 });
            }
            ComponentsView.ItemsSource = null; // Resets ItemsSource so Display notices change
            ComponentsView.ItemsSource = activeComponents;
        }
        private void ComponentsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currComponentIndex = activeComponents.IndexOf(e.CurrentSelection[0] as RoutineComponentsTable);
            ComponentsEdit.IsVisible = true;
            xcisePicker.SelectedIndex = activeComponents[currComponentIndex].XciseIdAttribute - 1; // SelectedIndex starts from 0, XciseId starts from 1 (SelectedIndex = -1 --> xcisePicker not set)
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
            int sets = Convert.ToInt16(setsInput.Text);
            activeComponents[currComponentIndex].XciseIdAttribute = xciseId;
            activeComponents[currComponentIndex].SetsAttribute = sets;
            ComponentsView.ItemsSource = null;
            ComponentsView.ItemsSource = activeComponents;
            ComponentsEdit.IsVisible = false;
        }
        private void DeleteComponent_Clicked(object sender, EventArgs e)
        {
            activeComponents.RemoveAt(currComponentIndex);
            ComponentsEdit.IsVisible = false;
            ComponentsView.ItemsSource = null;
            ComponentsView.ItemsSource = activeComponents;
        }
        private async void SaveRoutine_Clicked(object sender, EventArgs e)
        {
            if (RoutineName.Text == null)
            {
                inputObjection.IsVisible = true;
                inputObjection.Text = "Routine Must Have A Name";
                return;
            }
            foreach (RoutineComponentsTable component in activeComponents)
            {
                if (component.SetsAttribute <= 0 || component.XciseIdAttribute == 0) 
                {
                    inputObjection.IsVisible = true;
                    inputObjection.Text = "Num. Sets must be above 0, You must choose an exercise for each component.";
                    return;
                }
            }
            inputObjection.IsVisible = false;

            if (RoutineId == 0) // i.e. if Routine is new 
            {
                App.Db.SaveRoutine(new RoutinesTable { NameAttribute = RoutineName.Text });
                List<RoutinesTable> routines = await App.Db.GetRoutinesAsync();
                RoutineId = routines.Last().Id;
            }
            else
            {
                App.Db.UpdateRoutineNameAsync(RoutineName.Text, RoutineId);
                App.Db.DeleteRoutineComponents(RoutineId); // clears previous version of routine
            }
            foreach (RoutineComponentsTable component in activeComponents)
            {
                component.Id = 0; // reset to 0 so autoIncrement is triggered (autoIncrement starts from 1)
                component.RoutineAttribute = RoutineId;
                App.Db.SaveRoutineComponent(component);
            }
            await Navigation.PopAsync();

        }


    }
}
