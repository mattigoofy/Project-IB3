using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using WagentjeApp.Models;  // Voor Model-gerelateerde klassen zoals TrajectCommand en Traject
using WagentjeApp.Services;  // Voor Service-gerelateerde klassen zoals MqttService

namespace WagentjeApp.Views
{
    public partial class TrajectPage : ContentPage
    {
        private List<string> _commands; // Lijst om commando's op te slaan
        private List<Traject> _savedTrajects; // Gebruik Models.Traject om op te slaan in de UI

        public TrajectPage()
        {
            InitializeComponent();
            _commands = new List<string>(); // Initialiseer commandolijst
            _savedTrajects = new List<Traject>(); // Initialiseer opgeslagen trajectenlijst
            CommandsListView.ItemsSource = _commands;
            LoadSavedTrajects();
        }

        private async void LoadSavedTrajects()
        {
            var currentUser = MqttService.Instance.GetCurrentUser();
            int userId = currentUser.UserId;
            var savedTrajectsFromService = await MqttService.Instance.LoadTrajectsAsync(userId);

            _savedTrajects = savedTrajectsFromService.Select(t => new Traject
            {
                Id = t.Id,
                Name = t.Name,
                Commands = t.Commands.Select(c => new TrajectCommand(c.Name, c.Duration, c.Action)).ToList()
            }).ToList();

            SavedTrajectsListView.ItemsSource = _savedTrajects.Select(t => t.Name);
        }

        private void OnAddCommandButtonClicked(object sender, EventArgs e)
        {
            string selectedCommand = CommandPicker.SelectedItem as string;
            if (int.TryParse(DurationEntry.Text, out int duration) && !string.IsNullOrEmpty(selectedCommand))
            {
                _commands.Add($"{selectedCommand} - {duration} seconden");
                CommandsListView.ItemsSource = null;
                CommandsListView.ItemsSource = _commands;
                DurationEntry.Text = string.Empty; // Wis invoer
            }
            else
            {
                DisplayAlert("Error", "Voer een geldige duur in.", "OK");
            }
        }

        private async void OnSaveTrajectButtonClicked(object sender, EventArgs e)
        {
            string trajectName = TrajectNameEntry.Text;
            if (string.IsNullOrEmpty(trajectName))
            {
                await DisplayAlert("Fout", "Geef een naam op voor het traject.", "OK");
                return;
            }

            if (_commands.Count > 0)
            {
                var commands = _commands.Select(command =>
                {
                    var parts = command.Split(" - ");
                    if (parts.Length == 2 && int.TryParse(parts[1].Replace(" seconden", ""), out int duration))
                    {
                        string actionName = parts[0];
                        return new TrajectCommand(actionName, duration, actionName);
                    }
                    return null;
                }).Where(c => c != null).ToArray();

                if (commands.Length > 0)
                {
                    var currentUser = MqttService.Instance.GetCurrentUser();
                    int userId = currentUser.UserId;
                    var traject = new Traject
                    {
                        Name = trajectName,
                        Commands = commands.ToList()
                    };

                    await MqttService.Instance.SaveTrajectAsync(traject, userId);
                    await DisplayAlert("Succes", "Traject opgeslagen!", "OK");
                    _commands.Clear();
                    CommandsListView.ItemsSource = null;

                    LoadSavedTrajects();
                }
            }
        }

        private async void OnDeleteTrajectButtonClicked(object sender, EventArgs e)
        {
            if (SavedTrajectsListView.SelectedItem != null)
            {
                string selectedTrajectName = SavedTrajectsListView.SelectedItem as string;
                var traject = _savedTrajects.FirstOrDefault(t => t.Name == selectedTrajectName);

                if (traject != null)
                {
                    var currentUser = MqttService.Instance.GetCurrentUser();
                    int userId = currentUser.UserId;
                    await MqttService.Instance.DeleteTrajectAsync(traject.Id, userId);
                    await DisplayAlert("Succes", "Traject verwijderd!", "OK");

                    LoadSavedTrajects();
                }
            }
        }

        private async void OnExecuteTrajectButtonClicked(object sender, EventArgs e)
        {
            if (SavedTrajectsListView.SelectedItem != null)
            {
                string selectedTrajectName = SavedTrajectsListView.SelectedItem as string;
                var traject = _savedTrajects.FirstOrDefault(t => t.Name == selectedTrajectName);

                if (traject != null)
                {
                    
                    var currentUser = MqttService.Instance.GetCurrentUser();
                    int userId = currentUser.UserId;
                    await MqttService.Instance.ExecuteTrajectAsync(traject.Id, userId);
                    await DisplayAlert("Succes", "Traject uitgevoerd!", "OK");
                }
            }
        }
    }
}
