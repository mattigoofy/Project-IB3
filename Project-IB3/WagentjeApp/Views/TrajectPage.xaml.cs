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
        private int moveSpeed = 50;
        private bool editting = false;
        private int editting_id;

        public TrajectPage()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;

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
                Commands = t.Commands.Select(c => new TrajectCommand(c.Action, c.Duration, c.Name, c.Speed)).ToList()
            }).ToList();

            SavedTrajectsListView.ItemsSource = _savedTrajects.Select(t => t.Name);
        }

        private void OnAddCommandButtonClicked(object sender, EventArgs e)
        {
            string selectedCommand = CommandPicker.SelectedItem as string;
            if (double.TryParse(DurationEntry.Text, out double duration) && !string.IsNullOrEmpty(selectedCommand))
            {
                _commands.Add($"{selectedCommand} - {duration} seconden - {moveSpeed}%");
                CommandsListView.ItemsSource = null;
                CommandsListView.ItemsSource = _commands;
                // Wis invoer
                DurationEntry.Text = string.Empty;
                CommandPicker.SelectedIndex = -1;
                SliderValueLabel.Text = "50";
                ValueSlider.Value = 50;
            }
            else
            {
                DisplayAlert("Error", "Voer een geldige duur in.", "OK");
            }

        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            SliderValueLabel.Text = $"{e.NewValue:F0}";
            moveSpeed = (int)e.NewValue;
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
                    if (parts.Length == 3 && double.TryParse(parts[1].Replace(" seconden", ""), out double duration))
                    {
                        string actionName = parts[0];
                        string speed = parts[2].Replace("%", "");
                        TrajectNameEntry.Text = string.Empty;
                        return new TrajectCommand(actionName, duration, actionName, Int32.Parse(speed) );
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

                    await MqttService.Instance.SaveTrajectAsync(traject, userId, editting, editting_id);
                    await DisplayAlert("Succes", "Traject opgeslagen!", "OK");
                    _commands.Clear();
                    CommandsListView.ItemsSource = null;

                    LoadSavedTrajects();
                }
            }
            editting = false;
        }

        private void OnDeleteCommandButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var command = button?.CommandParameter as string;

            if (!string.IsNullOrEmpty(command) && _commands.Contains(command))
            {
                _commands.Remove(command);
                CommandsListView.ItemsSource = null; // Refresh the ListView
                CommandsListView.ItemsSource = _commands;
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
        private void OnEditTrajectButtonClicked(object sender, EventArgs e)
        {
            if (SavedTrajectsListView.SelectedItem != null)
            {
                string selectedTrajectName = SavedTrajectsListView.SelectedItem as string;
                var traject = _savedTrajects.FirstOrDefault(t => t.Name == selectedTrajectName);

                if (traject != null)
                {
                    TrajectNameEntry.Text = traject.Name;
                    _commands.Clear();
                    foreach (var command in traject.Commands)
                    {
                        _commands.Add($"{command.Action} - {command.Duration} seconden - {command.Speed}%");
                    }

                    CommandsListView.ItemsSource = null;
                    CommandsListView.ItemsSource = _commands;

                    DisplayAlert("Edit Mode", "Het traject is geladen voor bewerking.", "OK");
                    editting = true;
                    editting_id = traject.Id;
                }
                else
                {
                    DisplayAlert("Fout", "Het geselecteerde traject kon niet worden geladen.", "OK");
                }
            }
            else
            {
                DisplayAlert("Fout", "Selecteer eerst een traject om te bewerken.", "OK");
            }
        }

    }
}
