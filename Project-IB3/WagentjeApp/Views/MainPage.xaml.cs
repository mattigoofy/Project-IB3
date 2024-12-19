using Microsoft.Maui.Controls;
using WagentjeApp.Services;

namespace WagentjeApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;
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

        private async void OnAlleMeasurementsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AlleMetingenPage());
        }

        // Method for logging out
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Hier kun je eventueel de MQTT-verbinding verbreken
            await MqttService.Instance.DisconnectAsync();

            // Navigeer terug naar de LoginPage
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
       

    }

}
