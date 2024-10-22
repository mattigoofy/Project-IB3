using Microsoft.Maui.Controls;
using WagentjeApp.Services;
using WagentjeApp.Models;

namespace WagentjeApp.Views
{
    public partial class LiveTrajectPage : ContentPage
    {
        public LiveTrajectPage()
        {
            InitializeComponent();
        }

        private async void OnForwardClicked(object sender, EventArgs e)
        {
            await SendCommandAsync("Vooruit", 5);
        }

        private async void OnBackwardClicked(object sender, EventArgs e)
        {
            await SendCommandAsync("Achteruit", 5);
        }

        private async void OnLeftClicked(object sender, EventArgs e)
        {
            await SendCommandAsync("Links", 2);
        }

        private async void OnRightClicked(object sender, EventArgs e)
        {
            await SendCommandAsync("Rechts", 2);
        }

        private async Task SendCommandAsync(string commandName, int duration)
        {
            // Zorg ervoor dat je de juiste constructor gebruikt voor TrajectCommand
            var command = new TrajectCommand(commandName, duration, commandName);

            int userId = 1; // Hier moet je de juiste userId verkrijgen, dit kan uit een sessie of een andere bron komen
            await MqttService.Instance.SendTrajectAsync(new[] { command }, userId);
        }
    }
}
