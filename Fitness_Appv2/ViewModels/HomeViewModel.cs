using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Fitness_Appv2.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        // **** TEMPLATE CODE START ****
        public HomeViewModel()
        {
            Title = "Home";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        public ICommand OpenWebCommand { get; }
        // **** TEMPLATE CODE END ****
    }
}