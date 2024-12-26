using WagentjeApp.Services; // Voor Service-gerelateerde klassen zoals MqttService
using WagentjeApp.Models; // Voor Model-gerelateerde klassen zoals TrajectCommand
using System.Timers; // Voor het gebruik van de Timer

namespace WagentjeApp.Views
{
    public partial class LiveTrajectPage : ContentPage
    {
        private System.Timers.Timer _commandTimer; // Timer voor het verzenden van commando's
        private int moveSpeed = 50; // Snelheid van de beweging
        private bool isButtonHeld = false; // Status om te controleren of een knop ingedrukt is
        private double precision = 0.3; // Precisie voor de timer
        private string _currentCommand; // Huidig commando dat wordt verzonden

        // Constructor voor de LiveTrajectPage
        public LiveTrajectPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker

            // Initialiseer de timer met een interval in milliseconden
            _commandTimer = new System.Timers.Timer(precision * 1000);
            _commandTimer.Elapsed += OnTimerElapsed; // Voeg een event handler toe voor de timer
        }

        // Methode die wordt aangeroepen wanneer de "Vooruit" knop wordt ingedrukt
        private void OnForwardPressed(object sender, EventArgs e)
        {
            StartCommand("Vooruit"); // Start het commando "Vooruit"
        }

        // Methode die wordt aangeroepen wanneer de "Achteruit" knop wordt ingedrukt
        private void OnBackwardPressed(object sender, EventArgs e)
        {
            StartCommand("Achteruit"); // Start het commando "Achteruit"
        }

        // Methode die wordt aangeroepen wanneer de "Links" knop wordt ingedrukt
        private void OnLeftPressed(object sender, EventArgs e)
        {
            StartCommand("Links"); // Start het commando "Links"
        }

        // Methode die wordt aangeroepen wanneer de "Rechts" knop wordt ingedrukt
        private void OnRightPressed(object sender, EventArgs e)
        {
            StartCommand("Rechts"); // Start het commando "Rechts"
        }

        // Methode die wordt aangeroepen wanneer de "Links draaien" knop wordt ingedrukt
        private void OnTurnLeftPressed(object sender, EventArgs e)
        {
            StartCommand("Links_draaien"); // Start het commando "Links_draaien"
        }

        // Methode die wordt aangeroepen wanneer de "Rechts draaien" knop wordt ingedrukt
        private void OnTurnRightPressed(object sender, EventArgs e)
        {
            StartCommand("Rechts_draaien"); // Start het commando "Rechts_draaien"
        }

        // Methode die wordt aangeroepen wanneer de waarde van de slider verandert
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            SliderValueLabel.Text = $"{e.NewValue:F0}"; // Toon de nieuwe waarde van de slider
            moveSpeed = (int)e.NewValue; // Update de bewegingssnelheid
        }

        // Methode die wordt aangeroepen wanneer de knop wordt losgelaten
        private void OnButtonReleased(object sender, EventArgs e)
        {
            if (isButtonHeld)
            {
                isButtonHeld = false; // Zet de status van de knop op niet ingedrukt
                _commandTimer.Stop(); // Stop de timer
            }
            _currentCommand = null; // Reset het huidige commando
        }

        // Start het verzenden van een commando
        private void StartCommand(string commandName)
        {
            _currentCommand = commandName; // Stel het huidige commando in
            if (!isButtonHeld) // Controleer of de knop niet al ingedrukt is
            {
                isButtonHeld = true; // Zet de status van de knop op ingedrukt
                _commandTimer.Start(); // Start de timer
                SendCommandAsync(commandName, precision); // Verzend het commando
            }
        }

        // Methode die wordt aangeroepen wanneer de timer verstrijkt
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (isButtonHeld && !string.IsNullOrEmpty(_currentCommand)) // Controleer of de knop ingedrukt is en er een commando is
            {
                SendCommandAsync(_currentCommand, precision); // Verzend het huidige commando
            }
        }

        // Asynchrone methode om een commando te verzenden
        private async Task SendCommandAsync(string commandName, double duration)
        {
            var currentUser = MqttService.Instance.GetCurrentUser(); // Verkrijg de huidige gebruiker
            int userId = currentUser.UserId; // Verkrijg de gebruikers-ID
            var command = new TrajectCommand(commandName, duration, commandName, moveSpeed); // Maak een nieuw TrajectCommand object aan
            await MqttService.Instance.ExecuteCommandAsync(command, userId); // Voer het commando uit via de MQTT-service
        }
    }
}