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
        public Xcises()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            xcisesView.ItemsSource = await App.Db.GetXcisesAsync();
        }
        private async void SubmitXcise_Clicked(object sender, EventArgs e){
            if (((Button)sender).Text == "Submit Exercise")
            {
                string name = xciseNameInput.Text;
                bool isBodyweight = isBodyweightInput.IsChecked;
                IEnumerable<string> existingXcises = (await App.Db.GetXciseNamesAsync()).Select(obj => obj.XciseNameAttribute);
                if (!string.IsNullOrEmpty(name) && !existingXcises.Contains(name))
                {
                    submitXcise.Text = "Submitted";
                    await App.Db.SaveXcise(new XcisesTable
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
    }
}
