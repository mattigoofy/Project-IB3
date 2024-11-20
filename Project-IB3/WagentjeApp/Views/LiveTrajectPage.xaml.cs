using Microsoft.Maui.Controls;
using WagentjeApp.Services;
using WagentjeApp.Models;
using System.Timers;

namespace WagentjeApp.Views
{
    public partial class LiveTrajectPage : ContentPage
    {
        private System.Timers.Timer _commandTimer;

        public LiveTrajectPage()
        {
            InitializeComponent();
            _commandTimer = new System.Timers.Timer(200); // Interval in milliseconden
            _commandTimer.Elapsed += OnTimerElapsed;
        }

        private string _currentCommand;

        private  void OnForwardPressed(object sender, EventArgs e)
        {
            //SendCommandAsync("Vooruit", 5);
            //SendCommandAsync("Vooruit", 1);
            StartCommand("Vooruit");
        }

        private void OnBackwardPressed(object sender, EventArgs e)
        {
            SendCommandAsync("Achteruit", 1);
            //StartCommand("Achteruit");
        }

        private void OnLeftPressed(object sender, EventArgs e)
        {
            SendCommandAsync("Links", 1);
            //StartCommand("Links");
        }

        private void OnRightPressed(object sender, EventArgs e)
        {
            SendCommandAsync("Rechts", 1);
            //StartCommand("Rechts");
        }
        private void OnTurnLeftPressed(object sender, EventArgs e)
        {
            StartCommand("Links_draaien");
        }

        private void OnTurnRightPressed(object sender, EventArgs e)
        {
            StartCommand("Rechts_draaien");
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            StopCommand();
        }

        private void StartCommand(string commandName)
        {
            _currentCommand = commandName;
            _commandTimer.Start(); // Start de timer
            SendCommandAsync(commandName, 1); // Directe actie voor snelle respons
        }

        private void StopCommand()
        {
            _commandTimer.Stop(); // Stop de timer
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentCommand))
            {
                await SendCommandAsync(_currentCommand, 1);
            }
        }

        private async Task SendCommandAsync(string commandName, int duration)
        {
            var currentUser = MqttService.Instance.GetCurrentUser();
            int userId = currentUser.UserId;
            var command = new TrajectCommand(commandName, duration, commandName);
            await MqttService.Instance.ExecuteCommandAsync(command, userId);
        }
    }
}
