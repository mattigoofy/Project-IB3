using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WagentjeApp.Services;

namespace WagentjeApp.Views
{
    public partial class LiveMetingenPage : ContentPage
    {
        // ObservableCollection to hold measurements
        ObservableCollection<string> measurements;

        public LiveMetingenPage()
        {
            InitializeComponent();
            measurements = new ObservableCollection<string>();
            MeasurementsListView.ItemsSource = measurements;

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
                var measurement = await MqttService.Instance.GetLatestMeasurementAsync();

                // Voeg de meting toe aan de ObservableCollection op de hoofdthread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    measurements.Add($"Measurement: {measurement.Value}");
                });

                await Task.Delay(1000); // Even wachten voor de volgende meting
            }
        }
    }
}