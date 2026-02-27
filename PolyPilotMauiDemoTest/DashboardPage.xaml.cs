using PolyPilotMauiDemoTest.Localization;

namespace PolyPilotMauiDemoTest;

public partial class DashboardPage : ContentPage
{
    private int _count;

    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LocalizationManager.Instance.PropertyChanged += OnLocaleChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        LocalizationManager.Instance.PropertyChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_count > 0)
        {
            var key = _count == 1 ? "MainPage_Clicked" : "MainPage_ClickedMultiple";
            CounterBtn.Text = string.Format(LocalizationManager.Instance[key], _count);
        }
    }

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        _count++;
        var key = _count == 1 ? "MainPage_Clicked" : "MainPage_ClickedMultiple";
        CounterBtn.Text = string.Format(LocalizationManager.Instance[key], _count);
        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}
