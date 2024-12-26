using WagentjeApp.Services; // Voor Service-gerelateerde klassen zoals MqttService

namespace WagentjeApp.Views
{
    public partial class SettingsPage : ContentPage
    {
        // Constructor voor de SettingsPage
        public SettingsPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            MqttIpEntry.Text = MqttService.Instance.GetIpAddress(); // Stel het tekstveld in op het huidige IP-adres
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker
        }

        // Methode die wordt aangeroepen wanneer de "Opslaan" knop wordt ingedrukt
        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            string newIpAddress = MqttIpEntry.Text; // Verkrijg het nieuwe IP-adres uit het tekstveld

            // Stel het nieuwe IP-adres in via de MQTT-service
            MqttService.Instance.SetIpAddress(newIpAddress);
            // Toon een succesmelding
            DisplayAlert("Success", "IP-adres opgeslagen!", "OK");
        }
    }
}