using PolyPilotMauiDemoTest.Localization;

namespace PolyPilotMauiDemoTest
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;
            var key = count == 1 ? "MainPage_Clicked" : "MainPage_ClickedMultiple";
            CounterBtn.Text = string.Format(LocalizationManager.Instance[key], count);
            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}
