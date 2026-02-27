using System.Globalization;
using PolyPilotMauiDemoTest.Localization;

namespace PolyPilotMauiDemoTest
{
    public partial class App : Application
    {
        private const string ThemePreferenceKey = "app_theme";
        private const string LanguagePreferenceKey = "app_language";

        public App()
        {
            InitializeComponent();
            ApplySavedPreferences();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        private void ApplySavedPreferences()
        {
            // Restore persisted theme
            var themeIndex = Preferences.Default.Get(ThemePreferenceKey, 2);
            UserAppTheme = themeIndex switch
            {
                0 => AppTheme.Light,
                1 => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };

            // Restore persisted language
            var savedLang = Preferences.Default.Get(LanguagePreferenceKey, "en");
            LocalizationManager.Instance.SetCulture(new CultureInfo(savedLang));
        }
    }
}