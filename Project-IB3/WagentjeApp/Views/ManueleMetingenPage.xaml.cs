using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace WagentjeApp.Views
{
    public partial class ManueleMetingenPage : ContentPage
    {
        ObservableCollection<string> manualMeasurements;

        public ManueleMetingenPage()
        {
            InitializeComponent();
            Application.Current.UserAppTheme = AppTheme.Dark;

            manualMeasurements = new ObservableCollection<string>();
            ManualMeasurementsListView.ItemsSource = manualMeasurements;
        }

        private void OnAddMeasurementClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ManualMeasurementEntry.Text))
            {
                manualMeasurements.Add(ManualMeasurementEntry.Text);
                ManualMeasurementEntry.Text = string.Empty; // Clear entry
            }
            else
            {
                DisplayAlert("Error", "Voer een geldige meting in.", "OK");
            }
        }
    }
}
