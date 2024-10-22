using Microsoft.Maui.Controls;

namespace WagentjeApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCreateTrajectClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TrajectPage());
        }

        private async void OnLiveTrajectClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LiveTrajectPage());
        }

        private async void OnLiveMetingenClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LiveMetingenPage());
        }

        private async void OnManualMeasurementsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ManueleMetingenPage());
        }
    }
}
