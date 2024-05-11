using Fitness_Appv2.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Fitness_Appv2.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}