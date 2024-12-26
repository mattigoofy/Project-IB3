using WagentjeApp.Services; // Voor Service-gerelateerde klassen zoals MqttService

namespace WagentjeApp.Views
{
    public partial class MainPage : ContentPage
    {
        // Constructor voor de MainPage
        public MainPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker
        }

        // Methode die wordt aangeroepen wanneer de "Traject Aanmaken" knop wordt ingedrukt
        private async void OnCreateTrajectClicked(object sender, EventArgs e)
        {
            // Navigeer naar de TrajectPage
            await Navigation.PushAsync(new TrajectPage());
        }

        // Methode die wordt aangeroepen wanneer de "Live Traject Besturen" knop wordt ingedrukt
        private async void OnLiveTrajectClicked(object sender, EventArgs e)
        {
            // Navigeer naar de LiveTrajectPage
            await Navigation.PushAsync(new LiveTrajectPage());
        }

        // Methode die wordt aangeroepen wanneer de "Live Metingen Bekijken" knop wordt ingedrukt
        private async void OnLiveMetingenClicked(object sender, EventArgs e)
        {
            // Navigeer naar de LiveMetingenPage
            await Navigation.PushAsync(new LiveMetingenPage());
        }

        // Methode die wordt aangeroepen wanneer de "Manuele Metingen" knop wordt ingedrukt
        private async void OnManualMeasurementsClicked(object sender, EventArgs e)
        {
            // Navigeer naar de ManueleMetingenPage
            await Navigation.PushAsync(new ManueleMetingenPage());
        }

        // Methode die wordt aangeroepen wanneer de "Alle Metingen" knop wordt ingedrukt
        private async void OnAlleMeasurementsClicked(object sender, EventArgs e)
        {
            // Navigeer naar de AlleMetingenPage
            await Navigation.PushAsync(new AlleMetingenPage());
        }

        // Methode voor uitloggen
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            // Verbreek de MQTT-verbinding
            await MqttService.Instance.DisconnectAsync();

            // Navigeer terug naar de LoginPage
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}