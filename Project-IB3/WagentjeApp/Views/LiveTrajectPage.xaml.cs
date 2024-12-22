using Microsoft.Maui.Controls;
using WagentjeApp.Services;
using WagentjeApp.Models;
using System.Timers;

namespace WagentjeApp.Views
{
    public partial class LiveTrajectPage : ContentPage
    {
        private System.Timers.Timer _commandTimer;
        private int moveSpeed = 50;
        private bool isButtonHeld = false;
        private double precision = 0.3;

        public LiveTrajectPage()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;

            _commandTimer = new System.Timers.Timer(precision * 1000); // Interval in milliseconden
            _commandTimer.Elapsed += OnTimerElapsed;
        }

        private string _currentCommand;

        private  void OnForwardPressed(object sender, EventArgs e)
        {
            StartCommand("Vooruit");
        }

        private void OnBackwardPressed(object sender, EventArgs e)
        {
            StartCommand("Achteruit");
        }

        private void OnLeftPressed(object sender, EventArgs e)
        {
            StartCommand("Links");
        }

        private void OnRightPressed(object sender, EventArgs e)
        {
            StartCommand("Rechts");
        }
        private void OnTurnLeftPressed(object sender, EventArgs e)
        {
            StartCommand("Links_draaien");
        }

        private void OnTurnRightPressed(object sender, EventArgs e)
        {
            StartCommand("Rechts_draaien");
        }
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            SliderValueLabel.Text = $"{e.NewValue:F0}";
            moveSpeed = (int)e.NewValue;
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            if (isButtonHeld)
            {
                isButtonHeld = false;
                _commandTimer.Stop();
            }
            _currentCommand = null;
        }

        private void StartCommand(string commandName)
        {
            _currentCommand = commandName;
            if (!isButtonHeld)
            {
                isButtonHeld = true;
                _commandTimer.Start();
                SendCommandAsync(commandName, precision);
            }
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (isButtonHeld && !string.IsNullOrEmpty(_currentCommand))
            {
                SendCommandAsync(_currentCommand, precision);
            }
        }

        private async Task SendCommandAsync(string commandName, double duration)
        {
            var currentUser = MqttService.Instance.GetCurrentUser();
            int userId = currentUser.UserId;
            var command = new TrajectCommand(commandName, duration, commandName, moveSpeed);
            await MqttService.Instance.ExecuteCommandAsync(command, userId);
        }
    }
}
