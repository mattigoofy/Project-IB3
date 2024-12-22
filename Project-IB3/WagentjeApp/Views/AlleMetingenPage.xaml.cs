namespace WagentjeApp.Views;

using WagentjeApp.Models;  // Voor Model-gerelateerde klassen zoals Measurement
using WagentjeApp.Services;  // Voor Service-gerelateerde klassen zoals MqttService

public partial class AlleMetingenPage : ContentPage
{
    private List<Measurement> _measurements; // Gebruik Models.Measurement om op te slaan in de UI

    public AlleMetingenPage()
    {
        InitializeComponent();
        Application.Current.UserAppTheme = AppTheme.Dark;

        DateTime startTimestamp = new DateTime(2024, 1, 1, 0, 0, 0);
        DateTime endTimestamp = new DateTime(2025, 12, 31, 0, 0, 0);
        LoadMeasurements(startTimestamp, endTimestamp, 0, int.MaxValue);
    }

    private async void LoadMeasurements(DateTime startTimestamp, DateTime endTimestamp, int startValue, int endValue)
    {
        try
        {
            var currentUser = MqttService.Instance.GetCurrentUser();
            int userId = currentUser.UserId;

            var savedMeasurementsFromService = await MqttService.Instance.LoadMeasurementsAsync(startTimestamp, endTimestamp, startValue, endValue);

            _measurements = savedMeasurementsFromService.Select(t => new Measurement
            {
                Id = t.Id,
                Value = t.Value,
                Timestamp = t.Timestamp
            }).ToList();

            MeasurementsListView.ItemsSource = _measurements;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load measurements: {ex.Message}", "OK");
        }
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        DateTime startDate = StartDatePicker.Date;
        DateTime endDate = EndDatePicker.Date;
        DateTime startTimestamp= startDate.Add(StartTimePicker.Time);
        DateTime endTimestamp= endDate.Add(EndTimePicker.Time);

        int.TryParse(MinValueEntry.Text, out int minValue);
        int.TryParse(MaxValueEntry.Text, out int maxValue);

        LoadMeasurements(startTimestamp, endTimestamp, minValue, maxValue);
    }

    private void OnResetClicked(object sender, EventArgs e)
    {
        DateTime startTimestamp = new DateTime(2024, 1, 1, 0, 0, 0);
        DateTime endTimestamp = new DateTime(2025, 12, 31, 0, 0, 0);
        LoadMeasurements(startTimestamp, endTimestamp, 0, int.MaxValue);
    }
}
