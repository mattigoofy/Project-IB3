namespace WagentjeApp.Views
{
    using WagentjeApp.Models;  // Voor Model-gerelateerde klassen zoals Measurement
    using WagentjeApp.Services;  // Voor Service-gerelateerde klassen zoals MqttService

    public partial class AlleMetingenPage : ContentPage
    {
        private List<Measurement> _measurements; // Lijst om metingen op te slaan voor de UI

        // Constructor voor de AlleMetingenPage
        public AlleMetingenPage()
        {
            InitializeComponent(); // Initialiseer de componenten van de pagina
            Application.Current.UserAppTheme = AppTheme.Dark; // Stel het thema van de applicatie in op donker

            // Definieer de tijdstempels voor het laden van metingen
            DateTime startTimestamp = new DateTime(2024, 1, 1, 0, 0, 0);
            DateTime endTimestamp = new DateTime(2025, 12, 31, 0, 0, 0);
            LoadMeasurements(startTimestamp, endTimestamp, 0, int.MaxValue); // Laad metingen binnen het gedefinieerde bereik
        }

        // Asynchrone methode om metingen te laden
        private async void LoadMeasurements(DateTime startTimestamp, DateTime endTimestamp, int startValue, int endValue)
        {
            try
            {
                // Verkrijg de huidige gebruiker
                var currentUser = MqttService.Instance.GetCurrentUser();
                int userId = currentUser.UserId; // Verkrijg de gebruikers-ID

                // Laad de metingen van de service
                var savedMeasurementsFromService = await MqttService.Instance.LoadMeasurementsAsync(startTimestamp, endTimestamp, startValue, endValue);

                // Zet de geladen metingen om in een lijst van Measurement-objecten
                _measurements = savedMeasurementsFromService.Select(t => new Measurement
                {
                    Id = t.Id,
                    Value = t.Value,
                    Timestamp = t.Timestamp
                }).ToList();

                // Stel de ItemsSource van de ListView in op de geladen metingen
                MeasurementsListView.ItemsSource = _measurements;
            }
            catch (Exception ex)
            {
                // Toon een foutmelding als het laden van metingen mislukt
                await DisplayAlert("Error", $"Failed to load measurements: {ex.Message}", "OK");
            }
        }

        // Methode die wordt aangeroepen wanneer de filterknop wordt ingedrukt
        private void OnFilterClicked(object sender, EventArgs e)
        {
            // Verkrijg de geselecteerde datums en tijden van de pickers
            DateTime startDate = StartDatePicker.Date;
            DateTime endDate = EndDatePicker.Date;
            DateTime startTimestamp = startDate.Add(StartTimePicker.Time); // Voeg de tijd toe aan de startdatum
            DateTime endTimestamp = endDate.Add(EndTimePicker.Time); // Voeg de tijd toe aan de einddatum

            // Probeer de minimum- en maximumwaarden te parseren
            int.TryParse(MinValueEntry.Text, out int minValue);
            int.TryParse(MaxValueEntry.Text, out int maxValue);

            // Laad de metingen met de opgegeven filters
            LoadMeasurements(startTimestamp, endTimestamp, minValue, maxValue);
        }

        // Methode die wordt aangeroepen wanneer de resetknop wordt ingedrukt
        private void OnResetClicked(object sender, EventArgs e)
        {
            // Reset de tijdstempels naar de standaardwaarden
            DateTime startTimestamp = new DateTime(2024, 1, 1, 0, 0, 0);
            DateTime endTimestamp = new DateTime(2025, 12, 31, 0, 0, 0);
            LoadMeasurements(startTimestamp, endTimestamp, 0, int.MaxValue); // Laad metingen zonder filters
        }
    }
}