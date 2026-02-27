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
            // If the counter has been clicked, the {l:Translate} binding was replaced by an
            // imperative assignment. Manually update the text so it stays in the active language.
            if (count > 0)
            {
                var key = count == 1 ? "MainPage_Clicked" : "MainPage_ClickedMultiple";
                CounterBtn.Text = string.Format(LocalizationManager.Instance[key], count);
            }
            // count == 0: the original {l:Translate MainPage_ClickMe} binding is still active
            // and was already updated by LocalizationManager firing PropertyChanged.
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
