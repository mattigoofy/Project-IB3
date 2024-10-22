namespace Project_IB3;

public partial class Login : ContentPage
{
	public Login()
	{
		InitializeComponent();
	}

    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        //string email = emailEntry.Text;
        //string password = passwordEntry.Text;
        string email = "blabla@gmail.com";
        string password = "abc123";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        } else if (email == "blabla@gmail.com" && password == "abc123")
        {
            emailEntry.Text = "";
            passwordEntry.Text = "";
            await Navigation.PushAsync(new MainPage());
            return;
        } else
        {
            await DisplayAlert("WRONG", "Incorrect credentials", "Sorry");
        }

    }
}