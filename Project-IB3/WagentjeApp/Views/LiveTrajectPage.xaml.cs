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
            //SendCommandAsync("Vooruit", 5);
            //SendCommandAsync("Vooruit", 1);
            StartCommand("Vooruit");
        }

        private void OnBackwardPressed(object sender, EventArgs e)
        {
            //SendCommandAsync("Achteruit", 1);
            StartCommand("Achteruit");
        }

        private void OnLeftPressed(object sender, EventArgs e)
        {
            //SendCommandAsync("Links", 1);
            StartCommand("Links");
        }

        private void OnRightPressed(object sender, EventArgs e)
        {
            //SendCommandAsync("Rechts", 1);
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
            // Update the label with the current slider value
            SliderValueLabel.Text = $"{e.NewValue:F0}"; // Display as a whole number
            moveSpeed = (int)e.NewValue;
        }

        private void OnButtonReleased(object sender, EventArgs e)
        {
            //StopCommand();
            if (isButtonHeld)
            {
                isButtonHeld = false;
                _commandTimer.Stop(); // Stop sending MQTT messages
            }
            _currentCommand = null;
        }

        private void StartCommand(string commandName)
        {
            _currentCommand = commandName;
            //_commandTimer.Start(); // Start de timer
            //SendCommandAsync(commandName, 1); // Directe actie voor snelle respons
            if (!isButtonHeld)
            {
                isButtonHeld = true;
                _commandTimer.Start(); // Start sending MQTT messages
                SendCommandAsync(commandName, precision); // Send the first message immediately
            }
        }

        //private void StopCommand()
        //{
        //    _commandTimer.Stop(); // Stop de timer
        //}

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            //if (!string.IsNullOrEmpty(_currentCommand))
            //{
            //    await c
            //}
            if (isButtonHeld && !string.IsNullOrEmpty(_currentCommand))
            {
                //isButtonHeld = false;
                //_commandTimer.Stop();
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
