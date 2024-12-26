using WagentjeApp.Views; // Voor toegang tot de weergaveklassen zoals LoginPage
using WagentjeApp.Services; // Voor toegang tot de serviceklassen zoals MqttService

namespace WagentjeApp
{
    public partial class App : Application
    {
        private MqttService _mqttService; // Instantie van de MQTT-service

        // Constructor voor de App
        public App()
        {
            InitializeComponent(); // Initialiseer de componenten van de applicatie

            // Initialiseer de MQTT service en verbind met de broker
            _mqttService = MqttService.Instance;  // Gebruik de singleton-instantie van de MQTT-service

            // Zorg dat de connectie in de achtergrond wordt gemaakt
            InitializeMqttConnection();

            // Stel de initiële pagina in op LoginPage
            MainPage = new NavigationPage(new LoginPage());
        }

        // Asynchrone methode om de MQTT-verbinding te initialiseren
        private async Task InitializeMqttConnection()
        {
            try
            {
                // Verbind met de MQTT-broker
                await _mqttService.ConnectAsync();
                Console.WriteLine("MQTT connection initialized."); // Log een succesbericht
            }
            catch (Exception ex)
            {
                // Log een foutmelding als de verbinding mislukt
                Console.WriteLine($"Failed to initialize MQTT connection: {ex.Message}");
            }
        }

        // Methode die wordt aangeroepen wanneer de app wordt gestart
        protected override void OnStart()
        {
            base.OnStart(); // Roep de basisimplementatie aan
            InitializeMqttConnection();
        }

        // Methode die wordt aangeroepen wanneer de app in de achtergrond gaat
        protected override void OnSleep()
        {
            base.OnSleep(); // Roep de basisimplementatie aan
        }

        // Methode die wordt aangeroepen wanneer de app weer naar voren komt
        protected override void OnResume()
        {
            base.OnResume(); // Roep de basisimplementatie aan
            // Herstel de verbinding als de app weer naar voren komt
            InitializeMqttConnection();
        }
    }
}