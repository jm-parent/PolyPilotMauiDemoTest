using PolyPilotMauiDemoTest.ViewModels;

namespace PolyPilotMauiDemoTest;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SettingsViewModel vm)
            vm.Subscribe();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is SettingsViewModel vm)
            vm.Unsubscribe();
    }
}
