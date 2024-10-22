using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;
using WagentjeApp.Services;

namespace WagentjeApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;

            try
            {
                // Zorg ervoor dat de MQTT-service is verbonden
                await MqttService.Instance.ConnectAsync();

                // Probeer in te loggen
                bool isLoggedIn = await MqttService.Instance.LoginAsync(username, password);

                if (isLoggedIn)
                {
                    // Navigeren naar MainPage na succesvolle login
                    Application.Current.MainPage = new NavigationPage(new MainPage());
                }
                else
                {
                    await DisplayAlert("Error", "Ongeldige inloggegevens", "OK");
                }
            }
            catch (Exception ex)
            {
                // Toon een foutmelding als er iets misgaat
                await DisplayAlert("Error", $"Er is een fout opgetreden: {ex.Message}", "OK");
            }
        }

        private async void OnRegisterButtonClicked(object sender, EventArgs e)
        {
            // Navigeer naar de registratiepagina
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}
