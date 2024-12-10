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
    public partial class Xcises : ContentPage
    {
        List<XcisesTable> XcisesList;
        int currComponentIndex;
        public Xcises()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            XcisesList = await App.Db.GetXcisesAsync();
            xcisesView.ItemsSource = XcisesList;
        }
        private async void SubmitXcise_Clicked(object sender, EventArgs e){
            if (((Button)sender).Text == "Submit Exercise")
            {
                string name = xciseNameInput.Text;
                bool isBodyweight = isBodyweightInput.IsChecked;
                List<string> existingXcises = (await App.Db.GetXciseNamesAsync()).Select(obj => obj.XciseNameAttribute).ToList();
                if (!string.IsNullOrEmpty(name) && !existingXcises.Contains(name))
                {
                    submitXcise.Text = "Submitted";
                    App.Db.SaveXcise(new XcisesTable
                    {
                        XciseNameAttribute = name,
                        IsBodyweightAttribute = isBodyweight,
                    });
                    xcisesView.ItemsSource = await App.Db.GetXcisesAsync();
                    xciseNameInput.Text = "";
                    isBodyweightInput.IsChecked = false;
                    await Task.Delay(1500);
                    submitXcise.Text = "Submit Exercise";
                }
            }
        }

        private void XcisesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currComponentIndex = XcisesList.IndexOf(e.CurrentSelection[0] as XcisesTable);
            isPinnedInput.IsChecked = XcisesList[currComponentIndex].IsPinnedAttribute;
            changeIsPinned.IsVisible = true;
        }

        private async void SubmitIsPinned_Clicked(object sender, EventArgs e)
        {
            XcisesList[currComponentIndex].IsPinnedAttribute = isPinnedInput.IsChecked;    
            App.Db.UpdateXciseIsPinnedAsync(XcisesList[currComponentIndex].IsPinnedAttribute, XcisesList[currComponentIndex].Id);
            xcisesView.ItemsSource = null;
            xcisesView.ItemsSource = XcisesList;
            changeIsPinned.IsVisible = false;
        }
    }
}
