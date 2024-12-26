using System.Collections.ObjectModel; // Voor het gebruik van ObservableCollection
using WagentjeApp.Services; // Voor Service-gerelateerde klassen zoals MqttService
using WagentjeApp.Models;  // Voor Model-gerelateerde klassen zoals Measurement

namespace WagentjeApp.Views
{
    public partial class LiveMetingenPage : ContentPage
    {
        private ObservableCollection<Measurement> _measurements; // Lijst om live metingen op te slaan voor de UI

        // Constructor voor de LiveMetingenPage
        public LiveMetingenPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker

            _measurements = new ObservableCollection<Measurement>(); // Initialiseer de ObservableCollection voor metingen

            // Stel de ItemsSource van de ListView in op de ObservableCollection
            MeasurementsListView.ItemsSource = _measurements;

            // Start het proces voor het ontvangen van live metingen
            StartLiveMeasurements();
        }

        // Asynchrone methode om live metingen te starten
        private async void StartLiveMeasurements()
        {
            while (true) // Oneindige lus om continu metingen te ontvangen
            {
                try
                {
                    // Verkrijg de laatste meting van de MQTT-service
                    var savedMeasurement = await MqttService.Instance.GetLatestMeasurementAsync();

                    // Voeg de ontvangen meting toe aan de ObservableCollection op de hoofdthread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _measurements.Add(savedMeasurement); // Voeg de nieuwe meting toe

                        // Controleer of het aantal metingen groter is dan 100
                        if (_measurements.Count > 100)
                        {
                            _measurements.RemoveAt(0); // Verwijder de oudste meting om de lijst te beperken
                        }
                    });

                }
                catch (Exception ex)
                {
                    // Toon een foutmelding als het ophalen van de meting mislukt
                    await DisplayAlert("Error", "Couldn't get result: " + ex.Message, "OK");
                }
            }
        }
    }
}