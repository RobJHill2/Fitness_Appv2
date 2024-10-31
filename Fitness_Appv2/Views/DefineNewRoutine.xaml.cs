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
    public partial class DefineNewRoutine : ContentPage
    {
        List<RoutineComponentsTable> activeComponents = new List<RoutineComponentsTable>() { new RoutineComponentsTable() { Id = 0 } };
        // Components are given a temporary Id so they can be identified. This will change when they are entered into the DB
        RoutineComponentsTable currentComponent;
        public DefineNewRoutine()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            ComponentsView.ItemsSource = activeComponents;
            xcisePicker.ItemsSource = await App.Database.GetXciseNamesAsync();
        }
        private void NewComponent_Clicked(object sender, EventArgs e)
        {
            activeComponents.Add(new RoutineComponentsTable() { Id = activeComponents.Count });
            ComponentsView.ItemsSource = null; // Resets ItemsSource so Display notices change
            ComponentsView.ItemsSource = activeComponents;
        }
        private void ComponentsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentComponent = e.CurrentSelection[0] as RoutineComponentsTable;
            ComponentsEdit.IsVisible = true;
            xcisePicker.SelectedIndex = currentComponent.XciseIdAttribute; // SelectedIndex may not correlate to XciseId- TEST THIS
            if (currentComponent.SetsAttribute != 0)
            {
                setsInput.Text = Convert.ToString(currentComponent.SetsAttribute);
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
            int sets = Convert.ToInt32(setsInput.Text);
            if (sets > 0) //sanitisation
            {
                activeComponents[currentComponent.Id].XciseIdAttribute = xciseId;
                activeComponents[currentComponent.Id].SetsAttribute = sets;
                ComponentsView.ItemsSource = null;
                ComponentsView.ItemsSource = activeComponents;
                ComponentsEdit.IsVisible = false;
            }
            else
            {
                inputObjection.Text = "Number of sets must be more than 0";
            }
        }
    }
}
