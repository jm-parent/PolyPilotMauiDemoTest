using System.Collections.ObjectModel;
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
    private bool _isSubscribed;

    public SettingsViewModel()
    {
        _selectedThemeIndex = Preferences.Default.Get(ThemePreferenceKey, 2); // Default: System
        var savedLang = Preferences.Default.Get(LanguagePreferenceKey, "en");
        _selectedLanguageIndex = savedLang == "fr" ? 1 : 0;

        // Populate the collection once; subsequent updates are done in-place.
        foreach (var item in BuildThemeItems())
            ThemeOptions.Add(item);
    }

    // ObservableCollection kept as a stable reference — items are updated in-place
    // so the Picker never sees a new ItemsSource and never resets SelectedIndex.
    public ObservableCollection<string> ThemeOptions { get; } = new();

    /// <summary>
    /// Language options. Names are shown in their own language (not translated).
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

    /// <summary>Called by the page in OnAppearing to begin listening for locale changes.</summary>
    public void Subscribe()
    {
        if (_isSubscribed) return;
        LocalizationManager.Instance.PropertyChanged += OnLocalizationChanged;
        _isSubscribed = true;
    }

    /// <summary>Called by the page in OnDisappearing to stop listening — prevents memory leaks.</summary>
    public void Unsubscribe()
    {
        if (!_isSubscribed) return;
        LocalizationManager.Instance.PropertyChanged -= OnLocalizationChanged;
        _isSubscribed = false;
    }

    // Named handler so it can be unsubscribed precisely.
    private void OnLocalizationChanged(object? sender, PropertyChangedEventArgs e)
        => RefreshThemeOptionsInPlace();

    private void RefreshThemeOptionsInPlace()
    {
        var items = BuildThemeItems();
        // Update each item in-place: ObservableCollection fires Replace notifications
        // which lets the Picker update its displayed text without resetting SelectedIndex.
        for (int i = 0; i < items.Length; i++)
            ThemeOptions[i] = items[i];
    }

    private static string[] BuildThemeItems() =>
    [
        LocalizationManager.Instance["Settings_Theme_Light"],
        LocalizationManager.Instance["Settings_Theme_Dark"],
        LocalizationManager.Instance["Settings_Theme_System"]
    ];

    private static void ApplyTheme(int index)
    {
        // Guard: only valid indices produce a meaningful theme.
        if (index < 0) return;

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
