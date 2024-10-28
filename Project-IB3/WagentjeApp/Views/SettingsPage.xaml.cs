using Microsoft.Maui.Controls;
using WagentjeApp.Services;

namespace WagentjeApp.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent(); 
            MqttIpEntry.Text = MqttService.Instance.GetIpAddress(); 
        }

        private void OnSaveButtonClicked(object sender, EventArgs e)
        {
            string newIpAddress = MqttIpEntry.Text;

            MqttService.Instance.SetIpAddress(newIpAddress);
            DisplayAlert("Success", "IP-adres opgeslagen!", "OK");
        }
    }
}
