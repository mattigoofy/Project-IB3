using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using WagentjeApp.Models; // Zorg ervoor dat je dit toevoegt voor de TrajectCommand class
using WagentjeApp.Services;

namespace WagentjeApp.Views
{
    public partial class TrajectPage : ContentPage
    {
        private List<string> _commands; // Lijst om de commando's op te slaan

        public TrajectPage()
        {
            InitializeComponent();
            _commands = new List<string>(); // Initialiseer de lijst
            CommandsListView.ItemsSource = _commands; // Koppel de lijst aan de ListView
        }

        private void OnAddCommandButtonClicked(object sender, EventArgs e)
        {
            // Haal de geselecteerde waarde uit de Picker
            string selectedCommand = CommandPicker.SelectedItem as string;

            // Haal de duur op en valideer deze
            if (int.TryParse(DurationEntry.Text, out int duration) && !string.IsNullOrEmpty(selectedCommand))
            {
                // Voeg het commando toe aan de lijst
                _commands.Add($"{selectedCommand} - {duration} seconden");
                CommandsListView.ItemsSource = null; // Reset de binding
                CommandsListView.ItemsSource = _commands; // Update de ListView
                DurationEntry.Text = string.Empty; // Leeg het invoerveld
            }
            else
            {
                DisplayAlert("Error", "Voer een geldige duur in.", "OK");
            }
        }

        private async void OnExecuteTrajectButtonClicked(object sender, EventArgs e)
        {
            if (_commands.Count > 0)
            {
                // Maak een lijst met TrajectCommand objecten
                var commands = _commands.Select(command =>
                {
                    var parts = command.Split(" - ");

                    // Controleer of er voldoende onderdelen zijn en of de actie en duur geldig zijn
                    if (parts.Length == 2 && int.TryParse(parts[1].Replace(" seconden", ""), out int duration))
                    {
                        string actionName = parts[0]; // Haal de naam van het commando op (bijv. "Vooruit")
                        return new TrajectCommand(actionName, duration, actionName); // Geef ook de Name mee
                    }
                    else
                    {
                        // Foutmelding in geval van een probleem
                        DisplayAlert("Error", $"Ongeldig commando: {command}. Zorg ervoor dat het goed is geformatteerd.", "OK");
                        return null; // Teruggeven als er een probleem is
                    }
                }).Where(c => c != null).ToArray(); // Filter eventuele null-waarden uit

                if (commands.Length > 0)
                {
                    int userId = 1; // Stel een gebruikers-ID in (kan dynamisch worden aangepast)

                    // Stuur de traject commando's naar de Raspberry Pi via de MQTT service
                    await MqttService.Instance.SendTrajectAsync(commands, userId);
                    await DisplayAlert("Succes", "Traject uitgevoerd en naar Raspberry Pi verzonden!", "OK");
                }
                else
                {
                    await DisplayAlert("Fout", "Geen geldige commando's om uit te voeren.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Fout", "Geen commando's om uit te voeren.", "OK");
            }
        }


        // Methode om een commando uit de lijst te verwijderen
        private void OnDeleteCommandButtonClicked(object sender, EventArgs e)
        {
            if (CommandsListView.SelectedItem != null)
            {
                string selectedCommand = CommandsListView.SelectedItem as string;
                _commands.Remove(selectedCommand);

                // Update de ListView
                CommandsListView.ItemsSource = null;
                CommandsListView.ItemsSource = _commands;
            }
            else
            {
                DisplayAlert("Error", "Selecteer een commando om te verwijderen.", "OK");
            }
        }
    }
}
