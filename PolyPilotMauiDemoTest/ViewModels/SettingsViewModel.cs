using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using PolyPilotMauiDemoTest.Localization;

namespace PolyPilotMauiDemoTest.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private const string ThemePreferenceKey = "app_theme";
    private const string LanguagePreferenceKey = "app_language";

    private int _selectedThemeIndex;
    private int _selectedLanguageIndex;
    private List<string> _themeOptions = [];

    public SettingsViewModel()
    {
        _selectedThemeIndex = Preferences.Default.Get(ThemePreferenceKey, 2); // Default: System
        var savedLang = Preferences.Default.Get(LanguagePreferenceKey, "en");
        _selectedLanguageIndex = savedLang == "fr" ? 1 : 0;

        RefreshThemeOptions();

        // Refresh the localized theme option labels whenever the culture changes.
        LocalizationManager.Instance.PropertyChanged += (_, _) =>
        {
            RefreshThemeOptions();
        };
    }

    public List<string> ThemeOptions
    {
        get => _themeOptions;
        private set { _themeOptions = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Language options. Names are intentionally shown in their own language (not translated).
    /// </summary>
    public List<string> LanguageOptions { get; } = ["English", "Français"];

    public int SelectedThemeIndex
    {
        get => _selectedThemeIndex;
        set
        {
            if (_selectedThemeIndex == value) return;
            _selectedThemeIndex = value;
            OnPropertyChanged();
            ApplyTheme(value);
        }
    }

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set
        {
            if (_selectedLanguageIndex == value) return;
            _selectedLanguageIndex = value;
            OnPropertyChanged();
            ApplyLanguage(value);
        }
    }

    private void RefreshThemeOptions()
    {
        ThemeOptions =
        [
            LocalizationManager.Instance["Settings_Theme_Light"],
            LocalizationManager.Instance["Settings_Theme_Dark"],
            LocalizationManager.Instance["Settings_Theme_System"]
        ];
    }

    private static void ApplyTheme(int index)
    {
        var theme = index switch
        {
            0 => AppTheme.Light,
            1 => AppTheme.Dark,
            _ => AppTheme.Unspecified // System
        };

        if (Application.Current is not null)
            Application.Current.UserAppTheme = theme;

        Preferences.Default.Set(ThemePreferenceKey, index);
    }

    private static void ApplyLanguage(int index)
    {
        var cultureName = index == 1 ? "fr" : "en";
        LocalizationManager.Instance.SetCulture(new CultureInfo(cultureName));
        Preferences.Default.Set(LanguagePreferenceKey, cultureName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
