using Microsoft.Maui.Controls.Xaml;

namespace PolyPilotMauiDemoTest.Localization;

/// <summary>
/// XAML markup extension for localized strings.
/// Usage: Text="{l:Translate SomeKey}"  or  Text="{l:Translate Key=SomeKey}"
/// Automatically updates when the app language changes.
/// </summary>
[ContentProperty(nameof(Key))]
public class TranslateExtension : IMarkupExtension<BindingBase>
{
    public string Key { get; set; } = string.Empty;

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        return new Binding
        {
            Mode = BindingMode.OneWay,
            Path = $"[{Key}]",
            Source = LocalizationManager.Instance
        };
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => ProvideValue(serviceProvider);
}
