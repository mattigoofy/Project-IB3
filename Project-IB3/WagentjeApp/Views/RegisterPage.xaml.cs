using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using WagentjeApp.Services;

namespace WagentjeApp.Views
{
    public partial class RegisterPage : ContentPage
    {
        public RegisterPage()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Valideer invoer
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                await DisplayAlert("Error", "Vul alle velden in.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
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
                    await DisplayAlert("Succes", "Registratie geslaagd! U kunt nu inloggen.", "OK");
                    // Terug naar de loginpagina
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Registratie mislukt, probeer een andere gebruikersnaam of controleer uw gegevens.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Toon een foutmelding als er iets misgaat
                await DisplayAlert("Error", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }

        private async void OnBackToLoginButtonClicked(object sender, EventArgs e)
        {
            // Ga terug naar de loginpagina
            await Navigation.PopAsync();
        }
    }
}
