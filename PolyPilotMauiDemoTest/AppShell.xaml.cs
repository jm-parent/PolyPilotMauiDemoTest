namespace PolyPilotMauiDemoTest
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("settings", typeof(SettingsPage));
        }

        private async void OnSettingsClicked(object? sender, EventArgs e)
        {
            await GoToAsync("settings");
        }
    }
}
