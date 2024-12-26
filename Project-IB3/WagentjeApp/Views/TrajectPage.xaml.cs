using WagentjeApp.Models;  // Voor Model-gerelateerde klassen zoals TrajectCommand en Traject
using WagentjeApp.Services;  // Voor Service-gerelateerde klassen zoals MqttService

namespace WagentjeApp.Views
{
    public partial class TrajectPage : ContentPage
    {
        private List<string> _commands; // Lijst om commando's op te slaan
        private List<Traject> _savedTrajects; // Lijst om opgeslagen trajecten op te slaan
        private int moveSpeed = 50; // Snelheid van de beweging
        private bool editting = false; // Status om te controleren of we een traject bewerken
        private int editting_id; // ID van het traject dat we bewerken

        // Constructor voor de TrajectPage
        public TrajectPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker

            _commands = new List<string>(); // Initialiseer de lijst voor commando's
            _savedTrajects = new List<Traject>(); // Initialiseer de lijst voor opgeslagen trajecten
            CommandsListView.ItemsSource = _commands; // Stel de ItemsSource van de ListView in op de commando's
            LoadSavedTrajects(); // Laad opgeslagen trajecten
        }

        // Asynchrone methode om opgeslagen trajecten te laden
        private async void LoadSavedTrajects()
        {
            var currentUser = MqttService.Instance.GetCurrentUser(); // Verkrijg de huidige gebruiker
            int userId = currentUser.UserId; // Verkrijg de gebruikers-ID
            var savedTrajectsFromService = await MqttService.Instance.LoadTrajectsAsync(userId); // Laad trajecten van de service

            // Zet de geladen trajecten om in een lijst van Traject-objecten
            _savedTrajects = savedTrajectsFromService.Select(t => new Traject
            {
                Id = t.Id,
                Name = t.Name,
                Commands = t.Commands.Select(c => new TrajectCommand(c.Action, c.Duration, c.Name, c.Speed)).ToList()
            }).ToList();

            // Stel de ItemsSource van de ListView in op de namen van de opgeslagen trajecten
            SavedTrajectsListView.ItemsSource = _savedTrajects.Select(t => t.Name);
        }

        // Methode die wordt aangeroepen wanneer de "Voeg Commando Toe" knop wordt ingedrukt
        private void OnAddCommandButtonClicked(object sender, EventArgs e)
        {
            string selectedCommand = CommandPicker.SelectedItem as string; // Verkrijg het geselecteerde commando
            if (double.TryParse(DurationEntry.Text, out double duration) && !string.IsNullOrEmpty(selectedCommand))
            {
                // Voeg het commando toe aan de lijst
                _commands.Add($"{selectedCommand} - {duration} seconden - {moveSpeed}%");
                CommandsListView.ItemsSource = null; // Vernieuw de ListView
                CommandsListView.ItemsSource = _commands; // Stel de nieuwe ItemsSource in

                // Wis invoer
                DurationEntry.Text = string.Empty;
                CommandPicker.SelectedIndex = -1;
                SliderValueLabel.Text = "50"; // Reset de slider label
                ValueSlider.Value = 50; // Reset de slider waarde
            }
            else
            {
                // Toon een foutmelding als de invoer ongeldig is
                DisplayAlert("Error", "Voer een geldige duur in.", "OK");
            }
        }

        // Methode die wordt aangeroepen wanneer de waarde van de slider verandert
        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            SliderValueLabel.Text = $"{e.NewValue:F0}"; // Update de label met de nieuwe waarde
            moveSpeed = (int)e.NewValue; // Update de bewegingssnelheid
        }

        // Methode die wordt aangeroepen wanneer de "Opslaan Traject" knop wordt ingedrukt
        private async void OnSaveTrajectButtonClicked(object sender, EventArgs e)
        {
            string trajectName = TrajectNameEntry.Text; // Verkrijg de naam van het traject
            if (string.IsNullOrEmpty(trajectName))
            {
                // Toon een foutmelding als de naam niet is ingevuld
                await DisplayAlert("Fout", "Geef een naam op voor het traject.", "OK");
                return;
            }

            if (_commands.Count > 0) // Controleer of er commando's zijn toegevoegd
            {
                var commands = _commands.Select(command =>
                {
                    var parts = command.Split(" - "); // Splits het commando in delen
                    if (parts.Length == 3 && double.TryParse(parts[1].Replace(" seconden", ""), out double duration))
                    {
                        string actionName = parts[0]; // Verkrijg de actie naam
                        string speed = parts[2].Replace("%", ""); // Verkrijg de snelheid
                        TrajectNameEntry.Text = string.Empty; // Wis de invoer voor de trajectnaam
                        return new TrajectCommand(actionName, duration, actionName, Int32.Parse(speed)); // Maak een nieuw TrajectCommand object aan
                    }
                    return null; // Retourneer null als het commando ongeldig is
                }).Where(c => c != null).ToArray(); // Filter null waarden uit

                if (commands.Length > 0) // Controleer of er geldige commando's zijn
                {
                    var currentUser = MqttService.Instance.GetCurrentUser(); // Verkrijg de huidige gebruiker
                    int userId = currentUser.UserId; // Verkrijg de gebruikers-ID
                    var traject = new Traject
                    {
                        Name = trajectName, // Stel de naam van het traject in
                        Commands = commands.ToList() // Stel de lijst van commando's in
                    };

                    await MqttService.Instance.SaveTrajectAsync(traject, userId, editting, editting_id); // Sla het traject op via de service
                    await DisplayAlert("Succes", "Traject opgeslagen!", "OK"); // Toon een succesmelding
                    _commands.Clear(); // Wis de lijst van commando's
                    CommandsListView.ItemsSource = null; // Vernieuw de ListView

                    LoadSavedTrajects(); // Laad de opgeslagen trajecten opnieuw
                }
            }
            editting = false; // Zet de bewerkingsstatus terug naar false
        }

        // Methode die wordt aangeroepen wanneer de "Verwijder" knop wordt ingedrukt
        private void OnDeleteCommandButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button; // Verkrijg de knop die is ingedrukt
            var command = button?.CommandParameter as string; // Verkrijg het commando dat aan de knop is gekoppeld

            if (!string.IsNullOrEmpty(command) && _commands.Contains(command)) // Controleer of het commando geldig is
            {
                _commands.Remove(command); // Verwijder het commando uit de lijst
                CommandsListView.ItemsSource = null; // Vernieuw de ListView
                CommandsListView.ItemsSource = _commands; // Stel de nieuwe ItemsSource in
            }
        }

        // Methode die wordt aangeroepen wanneer de "Verwijder Geselecteerd Traject" knop wordt ingedrukt
        private async void OnDeleteTrajectButtonClicked(object sender, EventArgs e)
        {
            if (SavedTrajectsListView.SelectedItem != null) // Controleer of er een traject is geselecteerd
            {
                string selectedTrajectName = SavedTrajectsListView.SelectedItem as string; // Verkrijg de naam van het geselecteerde traject
                var traject = _savedTrajects.FirstOrDefault(t => t.Name == selectedTrajectName); // Zoek het traject in de opgeslagen trajecten

                if (traject != null) // Controleer of het traject bestaat
                {
                    var currentUser = MqttService.Instance.GetCurrentUser(); // Verkrijg de huidige gebruiker
                    int userId = currentUser.UserId; // Verkrijg de gebruikers-ID
                    await MqttService.Instance.DeleteTrajectAsync(traject.Id, userId); // Verwijder het traject via de service
                    await DisplayAlert("Succes", "Traject verwijderd!", "OK"); // Toon een succesmelding

                    LoadSavedTrajects(); // Laad de opgeslagen trajecten opnieuw
                }
            }
        }

        // Methode die wordt aangeroepen wanneer de "Voer Geselecteerd Traject Uit" knop wordt ingedrukt
        private async void OnExecuteTrajectButtonClicked(object sender, EventArgs e)
        {
            if (SavedTrajectsListView.SelectedItem != null) // Controleer of er een traject is geselecteerd
            {
                string selectedTrajectName = SavedTrajectsListView.SelectedItem as string; // Verkrijg de naam van het geselecteerde traject
                var traject = _savedTrajects.FirstOrDefault(t => t.Name == selectedTrajectName); // Zoek het traject in de opgeslagen trajecten

                if (traject != null) // Controleer of het traject bestaat
                {
                    var currentUser = MqttService.Instance.GetCurrentUser(); // Verkrijg de huidige gebruiker
                    int userId = currentUser.UserId; // Verkrijg de gebruikers-ID
                    await MqttService.Instance.ExecuteTrajectAsync(traject.Id, userId); // Voer het traject uit via de service
                    await DisplayAlert("Succes", "Traject uitgevoerd!", "OK"); // Toon een succesmelding
                }
            }
        }

        // Methode die wordt aangeroepen wanneer de "Bewerk Geselecteerd Trajec" knop wordt ingedrukt
        private void OnEditTrajectButtonClicked(object sender, EventArgs e)
        {
            if (SavedTrajectsListView.SelectedItem != null) // Controleer of er een traject is geselecteerd
            {
                string selectedTrajectName = SavedTrajectsListView.SelectedItem as string; // Verkrijg de naam van het geselecteerde traject
                var traject = _savedTrajects.FirstOrDefault(t => t.Name == selectedTrajectName); // Zoek het traject in de opgeslagen trajecten

                if (traject != null) // Controleer of het traject bestaat
                {
                    TrajectNameEntry.Text = traject.Name; // Stel de invoer voor de trajectnaam in
                    _commands.Clear(); // Wis de lijst van commando's
                    foreach (var command in traject.Commands) // Voeg de commando's van het traject toe aan de lijst
                    {
                        _commands.Add($"{command.Action} - {command.Duration} seconden - {command.Speed}%");
                    }

                    CommandsListView.ItemsSource = null; // Vernieuw de ListView
                    CommandsListView.ItemsSource = _commands; // Stel de nieuwe ItemsSource in

                    DisplayAlert("Edit Mode", "Het traject is geladen voor bewerking.", "OK"); // Toon een melding dat het traject is geladen voor bewerking
                    editting = true; // Zet de bewerkingsstatus op true
                    editting_id = traject.Id; // Sla de ID van het te bewerken traject op
                }
                else
                {
                    DisplayAlert("Fout", "Het geselecteerde traject kon niet worden geladen.", "OK"); // Toon een foutmelding als het traject niet kan worden geladen
                }
            }
            else
            {
                DisplayAlert("Fout", "Selecteer eerst een traject om te bewerken.", "OK"); // Toon een foutmelding als er geen traject is geselecteerd
            }
        }
    }
}