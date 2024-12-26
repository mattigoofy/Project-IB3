using WagentjeApp.Services; // Voor Service-gerelateerde klassen zoals MqttService

namespace WagentjeApp.Views
{
    public partial class LoginPage : ContentPage
    {
        // Constructor voor de LoginPage
        public LoginPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker
        }

        // Methode die wordt aangeroepen wanneer de login-knop wordt ingedrukt
        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text; // Verkrijg de gebruikersnaam uit de invoer
            string password = PasswordEntry.Text; // Verkrijg het wachtwoord uit de invoer

            try
            {
                // Zorg ervoor dat de MQTT-service is verbonden
                await MqttService.Instance.ConnectAsync();

                // Probeer in te loggen met de opgegeven gebruikersnaam en wachtwoord
                bool isLoggedIn = await MqttService.Instance.LoginAsync(username, password);

                if (isLoggedIn)
                {
                    // Navigeren naar MainPage na succesvolle login
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    // Toon een foutmelding als de inloggegevens ongeldig zijn
                    await DisplayAlert("Error", "Ongeldige inloggegevens", "OK");
                }
            }
            catch (Exception ex)
            {
                // Toon een foutmelding als er iets misgaat tijdens het inloggen
                await DisplayAlert("Error", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }

        // Methode die wordt aangeroepen wanneer de registratie-knop wordt ingedrukt
        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            // Navigeer naar de registratiepagina
            await Navigation.PushAsync(new RegisterPage());
        }

        // Methode die wordt aangeroepen wanneer de instellingen-knop wordt ingedrukt
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            // Navigeer naar de instellingenpagina
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}