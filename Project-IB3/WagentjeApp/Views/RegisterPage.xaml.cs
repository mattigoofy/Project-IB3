using WagentjeApp.Services; // Voor Service-gerelateerde klassen zoals MqttService

namespace WagentjeApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        // Constructor voor de RegisterPage
        public RegisterPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker
        }

        // Methode die wordt aangeroepen wanneer de registratie-knop wordt ingedrukt
        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            // Verkrijg de waarden van de invoervelden
            string username = UsernameEntry.Text;
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Valideer invoer
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                // Toon een foutmelding als niet alle velden zijn ingevuld
                await DisplayAlert("Error", "Vul alle velden in.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                // Toon een foutmelding als de wachtwoorden niet overeenkomen
                await DisplayAlert("Error", "De wachtwoorden komen niet overeen.", "OK");
                return;
            }

            try
            {
                // Zorg ervoor dat de MQTT-service is verbonden
                await MqttService.Instance.ConnectAsync();

                // Probeer te registreren (stuurt ook email mee nu)
                bool isRegistered = await MqttService.Instance.RegisterAsync(username, password, confirmPassword, email);

                if (isRegistered)
                {
                    // Toon een succesmelding bij succesvolle registratie
                    await DisplayAlert("Succes", "Registratie geslaagd! U kunt nu inloggen.", "OK");
                    // Ga terug naar de loginpagina
                    await Navigation.PopAsync();
                }
                else
                {
                    // Toon een foutmelding als de registratie mislukt
                    await DisplayAlert("Error", "Registratie mislukt, probeer een andere gebruikersnaam of controleer uw gegevens.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Toon een foutmelding als er iets misgaat tijdens het registratieproces
                await DisplayAlert("Error", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }

        // Methode die wordt aangeroepen wanneer de "Terug naar login" knop wordt ingedrukt
        private async void OnBackToLoginButtonClicked(object sender, EventArgs e)
        {
            // Ga terug naar de loginpagina
            await Navigation.PopAsync();
        }
    }
}