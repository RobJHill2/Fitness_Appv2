using Xamarin.Forms;

namespace Fitness_Appv2.Views
{
    public partial class Home : ContentPage
    {
        public Home()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            viewPinned.ItemsSource = await App.Db.GetPinnedXcisesAsync();
        }
    }
}