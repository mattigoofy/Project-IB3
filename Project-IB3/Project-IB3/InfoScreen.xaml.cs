using System.Collections.ObjectModel;

namespace Project_IB3;

public partial class InfoScreen : ContentPage
{
    // ObservableCollection to hold the dynamic data
    public ObservableCollection<DataItem> DataList { get; set; }

    // Counter to track the index for each new item
    private int currentIndex = 1;


    public InfoScreen()
	{
		InitializeComponent();

        // Initialize the data collection
        DataList = new ObservableCollection<DataItem>();

        // Set the BindingContext to the current page
        BindingContext = this;
    }

    private async void mainPageButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }


    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }
    
    // Event handler for the "Add Data" button click
    private void OnLiveSwitchToggled(object sender, EventArgs e)
    {
        // Add a new item to the collection
        DataList.Insert(0, new DataItem
        {
            Index = currentIndex,
            Distance = $"{new Random().Next(1, 10)}",
            Time = $"{new Random().Next(1, 60)}"
        });

        // Increment the index counter
        currentIndex++;
    }
}

// Define a class to represent each row in the collection
public class DataItem
{
    public int Index { get; set; }
    public string Distance { get; set; }
    public string Time { get; set; }
}
