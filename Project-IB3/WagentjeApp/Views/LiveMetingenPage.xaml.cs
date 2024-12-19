using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WagentjeApp.Services;
using WagentjeApp.Models;  // Voor Model-gerelateerde klassen zoals Measurement

namespace WagentjeApp.Views
{
    public partial class LiveMetingenPage : ContentPage
    {
        // ObservableCollection to hold measurements
        //private ObservableCollection<string> measurements;
        //private int amountOfMeasurements = 0;
        private ObservableCollection<Measurement> _measurements; // Gebruik Models.Measurement om op te slaan in de UI

        public LiveMetingenPage()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;

            _measurements = new ObservableCollection<Measurement>();

            // Bind the collection to the ListView
            MeasurementsListView.ItemsSource = _measurements;

            // Start de live metingen
            StartLiveMeasurements();
        }

        private async void StartLiveMeasurements()
        {
            while (true)
            {
                //// Krijg de laatste meting van MQTT
                //var measurement = await MqttService.Instance.GetLatestMeasurementAsync();

                //// Voeg de meting toe aan de ObservableCollection
                //measurements.Add($"Measurement: {measurement.Value}"); // Format de meting als string

                //await Task.Delay(1000); // Even wachten voor de volgende meting

                // Krijg de laatste meting van MQTT
                try
                {
                    var savedMeasurement = await MqttService.Instance.GetLatestMeasurementAsync();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _measurements.Add(savedMeasurement);

                        // Optionally, limit the size of the collection (e.g., last 100 measurements)
                        if (_measurements.Count > 100)
                        {
                            _measurements.RemoveAt(0); // Remove the oldest measurement
                        }
                    });

                } catch (Exception ex)
                {
                    DisplayAlert("Error", "Couldn't get result", "OK");
                }

                // Voeg de meting toe aan de ObservableCollection op de hoofdthread

                // await Task.Delay(1000); // Even wachten voor de volgende meting
            }
        }
    }
}