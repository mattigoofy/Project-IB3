using Microsoft.Maui.Controls;
using WagentjeApp.Views; // Zorg ervoor dat je deze hebt toegevoegd
using WagentjeApp.Services;
using System.Threading.Tasks;

namespace WagentjeApp
{
    public partial class App : Application
    {
        private MqttService _mqttService;

        public App()
        {
            InitializeComponent();

            // Initialiseer de MQTT service en verbind met de broker
            _mqttService = MqttService.Instance;  // Singleton-instantie gebruiken

            // Zorg dat de connectie in de achtergrond wordt gemaakt
            InitializeMqttConnection();

            // Set the initial page to LoginPage
            MainPage = new NavigationPage(new LoginPage());
        }

        private async Task InitializeMqttConnection()
        {
            try
            {
                // Verbind met de MQTT-broker
                await _mqttService.ConnectAsync();
                Console.WriteLine("MQTT connection initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize MQTT connection: {ex.Message}");
            }
        }

        // Eventueel kun je ook bij het hervatten van de app opnieuw de verbinding maken
        protected override void OnStart()
        {
            base.OnStart();
            // Verbind eventueel opnieuw bij start (afhankelijk van je requirements)
            InitializeMqttConnection();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Hier kun je de connectie afsluiten als dat nodig is
            // (afhankelijk van hoe lang de app in de achtergrond blijft)
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Verbinding herstellen als de app weer naar voren komt
            InitializeMqttConnection();
        }
    }
}
