using System.Collections.ObjectModel;
using System.Globalization;

namespace LocalizationResourceManager.Maui;

[ContentProperty(nameof(Path))]
[AcceptEmptyServiceProvider]
public class TranslateBindingExtension : IMarkupExtension<BindingBase>, IMultiValueConverter
{
    /// <inheritdoc/>
    public string Path { get; set; } = ".";

    /// <inheritdoc/>
    public BindingMode Mode { get; set; } = BindingMode.OneWay;

    /// <inheritdoc/>
    public string StringFormat { get; set; } = "{0}";

    /// <inheritdoc/>
    public IValueConverter? Converter { get; set; } = null;

    /// <inheritdoc/>
    public object? ConverterParameter { get; set; } = null;

    /// <inheritdoc/>
    public object? Source { get; set; } = null;

    /// <summary>
    /// Flag whether to translate the binding value directly.
    /// If <see langword="true"/>, the value will be used as a key to look up the translation.
    /// If <see langword="false"/>, the value will be returned as is unless other translation properties are set.
    /// </summary>
    public bool TranslateValue { get; set; } = false;

    /// <summary>
    /// Format string similar to StringFormat, but the format comes from a localized string resource.
    /// Binding value will be used as the argument, e.g. "Clicked {0} times"
    /// </summary>
    public string? TranslateFormat { get; set; }

    /// <summary>
    /// Format string similar to StringFormat, but used for when the binding value is one (1).
    /// Binding value will be used as the argument, e.g. "Clicked {0} time"
    /// </summary>
    public string? TranslateOne { get; set; }

    /// <summary>
    /// Format string similar to StringFormat, but used for when the binding value is zero (0).
    /// Binding value will be used as the argument, e.g. "Click Me"
    /// </summary>
    public string? TranslateZero { get; set; }

    /// <summary>
    /// Localized string resource used for when the binding value is evaluated to <see langword="true"/>.
    /// e.g. "Yes", "On", "Activated"
    /// </summary>
    public string? TranslateTrue { get; set; }

    /// <summary>
    /// Localized string resource used for when the binding value is evaluated to <see langword="false"/>.
    /// e.g. "No", "Off", "Deactivated"
    /// </summary>
    public string? TranslateFalse { get; set; }

    /// <summary>
    /// Localized string resource used when the binding value is <see langword="null"/> during data binding.
    /// e.g. "User name missing!"
    /// </summary>
    public string? TargetNullValue { get; set; }

    /// <summary>
    /// Localized string resource used when the binding value is not found.
    /// e.g. "Value not found!"
    /// </summary>
    public string? FallbackValue { get; set; }

    /// <summary>
    /// Specifies the strategy to use when converting a value that may not be directly convertible.
    /// </summary>
    /// <remarks>
    /// Used to indicate how conversion operations should handle cases where the input value cannot be converted as expected.
    /// The options allow for returning a default value, a null value, or a specified fallback value, depending on the desired behavior.
    /// </remarks>
    private enum ConvertValue
    {
        DefaultValue = 0,
        NullValue = 1,
        FallbackValue = 2,
    }

    /// <inheritdoc/>
    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
    }

    BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
    {
        return new MultiBinding()
        {
            StringFormat = StringFormat,
            Converter = this,
            Mode = Mode,
            Bindings = new Collection<BindingBase>
            {
                new Binding(Path, Mode, Converter, ConverterParameter, source: Source)
                {
                    FallbackValue = FallbackValue is null ? null : ConvertValue.FallbackValue,
                    TargetNullValue = TargetNullValue is null ? null : ConvertValue.NullValue,
                },
                new Binding(nameof(LocalizationResourceManager.CurrentCulture), BindingMode.OneWay, source:LocalizationResourceManager.Current)
            }
        };
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var value = values?.FirstOrDefault();
        if (value is null)
        {
            return string.Empty;
        }

        if (value is ConvertValue.FallbackValue)
        {
            return LocalizationResourceManager.Current[FallbackValue!];
        }

        if (value is ConvertValue.NullValue)
        {
            return LocalizationResourceManager.Current[TargetNullValue!];
        }

        if (!string.IsNullOrWhiteSpace(TranslateZero) && IsZero(value))
        {
            return LocalizationResourceManager.Current[TranslateZero, value];
        }

        if (!string.IsNullOrWhiteSpace(TranslateOne) && IsOne(value))
        {
            return LocalizationResourceManager.Current[TranslateOne, value];
        }

        if (!string.IsNullOrWhiteSpace(TranslateTrue) && IsTrue(value))
        {
            return LocalizationResourceManager.Current[TranslateTrue];
        }

        if (!string.IsNullOrWhiteSpace(TranslateFalse) && IsFalse(value))
        {
            return LocalizationResourceManager.Current[TranslateFalse];
        }

        if (!string.IsNullOrWhiteSpace(TranslateFormat))
        {
            return LocalizationResourceManager.Current[TranslateFormat, value];
        }

        if (TranslateValue)
        {
            return LocalizationResourceManager.Current[$"{value}"];
        }

        return value;
    }

    private static bool IsZero(object value) => (value is int number && number == 0);

    private static bool IsOne(object value) => (value is int number && number == 1);

    private static bool IsTrue(object value) => (value is bool flag && flag);

    private static bool IsFalse(object value) => (value is bool flag && !flag);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}