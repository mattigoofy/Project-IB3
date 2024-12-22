using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WagentjeApp.Services;
using WagentjeApp.Models;  // Voor Model-gerelateerde klassen zoals Measurement

namespace WagentjeApp.Views
{
    public partial class LiveMetingenPage : ContentPage
    {
        private ObservableCollection<Measurement> _measurements; // Gebruik Models.Measurement om op te slaan in de UI

        public LiveMetingenPage()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;

            _measurements = new ObservableCollection<Measurement>();

            MeasurementsListView.ItemsSource = _measurements;

            StartLiveMeasurements();
        }

        private async void StartLiveMeasurements()
        {
            while (true)
            {
                try
                {
                    var savedMeasurement = await MqttService.Instance.GetLatestMeasurementAsync();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _measurements.Add(savedMeasurement);

                        if (_measurements.Count > 100)
                        {
                            _measurements.RemoveAt(0); // Remove oudste meting
                        }
                    });

                } catch (Exception ex)
                {
                    DisplayAlert("Error", "Couldn't get result", "OK");
                }
            }
        }
    }
}