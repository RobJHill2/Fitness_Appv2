using Fitness_Appv2.Services;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Fitness_Appv2.Views
{
    public partial class Routines : ContentPage
    {
        List<RoutinesTable> displayedRoutines;
        int currRoutineIndex;
        public Routines()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            RoutineOptions.IsVisible = false;
            displayedRoutines = await App.Db.GetRoutinesAsync();
            RoutinesView.ItemsSource = displayedRoutines;
        }

        private void RoutinesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currRoutineIndex = displayedRoutines.IndexOf(e.CurrentSelection[0] as RoutinesTable);
            RoutineOptions.IsVisible = true;
        }
        private void EditRoutine_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new EditRoutine(displayedRoutines[currRoutineIndex].Id));
        }

        private void DeleteRoutine_Clicked(object sender, EventArgs e)
        {
            App.Db.DeleteRoutineComponentsAsync(displayedRoutines[currRoutineIndex].Id);
            App.Db.DeleteRoutineAsync(displayedRoutines[currRoutineIndex].Id);
            displayedRoutines.RemoveAt(currRoutineIndex);
            RoutineOptions.IsVisible = false;
            RoutinesView.ItemsSource = null;
            RoutinesView.ItemsSource = displayedRoutines;
        }

        private void LogRoutine_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new LogRoutine(displayedRoutines[currRoutineIndex].Id));
        }

        private void NewRoutine_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new EditRoutine(0));
        }


    }
}
