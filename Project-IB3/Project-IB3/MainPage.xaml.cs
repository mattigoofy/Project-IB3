//using Android.Widget;

namespace Project_IB3
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Hide the back button when this page appears
            NavigationPage.SetHasBackButton(this, false);
        }

        private void moveForward(Object sender, EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Title", "Movin forwards", "OK");

        }

        private void moveBackwards(Object sender, EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Title", "Movin backwards", "OK");

        }

        private void moveLeftButton_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Title", "Movin left", "OK");
        }

        private void moveRightButton_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage.DisplayAlert("Title", "Movin Right", "OK");
        }

        private async void toInfoScreenButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new InfoScreen());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
        //    count+=5;

        //    if (count == 1)
        //        CounterBtn.Text = $"Clicked {count} time";
        //    else
        //        CounterBtn.Text = $"Clicked {count} times";

        //    SemanticScreenReader.Announce(CounterBtn.Text);
        //}
    }

}
