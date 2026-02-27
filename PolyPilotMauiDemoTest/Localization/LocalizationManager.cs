using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace PolyPilotMauiDemoTest.Localization;

/// <summary>
/// Singleton manager for app localization. Provides dynamic string lookup
/// and fires PropertyChanged when the culture changes so bound UI updates automatically.
/// Usage in XAML: {l:Translate Key=SomeKey}
/// </summary>
public class LocalizationManager : INotifyPropertyChanged
{
    private static readonly Lazy<LocalizationManager> s_instance =
        new(() => new LocalizationManager());

    public static LocalizationManager Instance => s_instance.Value;

    private readonly ResourceManager _resourceManager;

    private LocalizationManager()
    {
        _resourceManager = new ResourceManager(
            "PolyPilotMauiDemoTest.Resources.Strings.AppResources",
            typeof(LocalizationManager).Assembly);
    }

    /// <summary>Returns the localized string for the given key in the current UI culture.</summary>
    public string this[string key]
    {
        get
        {
            try
            {
                return _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
            }
            catch
            {
                return key;
            }
        }
    }

    /// <summary>
    /// Changes the current culture and notifies all bindings to re-evaluate.
    /// </summary>
    public void SetCulture(CultureInfo culture)
    {
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        // Passing null signals all properties have changed, refreshing all {l:Translate} bindings.
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
