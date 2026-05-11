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
    /// Format string similar to StringFormat, but used for when the binding value is negative.
    /// Binding value will be used as the argument, e.g. "▼ {0} below"
    /// </summary>
    public string? TranslateNegative { get; set; }

    /// <summary>
    /// Format string similar to StringFormat, but used for when the binding value is positive.
    /// Binding value will be used as the argument, e.g. "▲ {0} above"
    /// </summary>
    public string? TranslatePositive { get; set; }

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
    /// Default value used when the binding value is <see langword="null"/> during data binding.
    /// </summary>
    public object? TargetNullValue { get; set; }

    /// <summary>
    /// Default value used when the binding value is not found.
    /// </summary>
    public object? FallbackValue { get; set; }

    /// <summary>
    /// Localized string resource used when the binding value is <see langword="null"/> during data binding.
    /// e.g. "User name missing!"
    /// </summary>
    public string? TargetNullText { get; set; }

    /// <summary>
    /// Localized string resource used when the binding value is not found.
    /// e.g. "Value not found!"
    /// </summary>
    public string? FallbackText { get; set; }

    /// <summary>
    /// Specifies the strategy to use when converting a value that may not be directly convertible.
    /// </summary>
    /// <remarks>
    /// Used to indicate how conversion operations should handle cases where the input value cannot be converted as expected.
    /// The options allow for returning a default value, a null value, or a specified fallback value, depending on the desired behavior.
    /// </remarks>
    [Flags]
    private enum ConvertValue
    {
        DefaultValue = 0,
        NullValue = 1,
        FallbackValue = 2,
        NullText = 4,
        FallbackText = 8
    }

    /// <inheritdoc/>
    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
    }

    BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
    {
        //Check FallbackValue and FallbackText to determine the FallbackValue binding value to return in the converter bindings
        ConvertValue? fallbackValue = (FallbackValue is not null, FallbackText is not null) switch
        {
            (true, true) => ConvertValue.FallbackValue | ConvertValue.FallbackText,
            (true, false) => ConvertValue.FallbackValue,
            (false, true) => ConvertValue.FallbackText,
            (false, false) => null
        };

        //Check TargetNullValue and TargetNullText to determine the TargetNullValue binding value to return in the converter bindings
        ConvertValue? targetNullValue = (TargetNullValue is not null, TargetNullText is not null) switch
        {
            (true, true) => ConvertValue.NullValue | ConvertValue.NullText,
            (true, false) => ConvertValue.NullValue,
            (false, true) => ConvertValue.NullText,
            (false, false) => null
        };

        return new MultiBinding()
        {
            StringFormat = StringFormat,
            Converter = this,
            Mode = Mode,
            Bindings = new Collection<BindingBase>
            {
                new Binding(Path, Mode, Converter, ConverterParameter, source: Source)
                {
                    FallbackValue = fallbackValue,
                    TargetNullValue = targetNullValue,
                },
                new Binding(nameof(LocalizationResourceManager.CurrentCulture), BindingMode.OneWay, source:LocalizationResourceManager.Current)
            }
        };
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        //Init
        var value = values is { Length: > 0 } ? values[0] : null;
        var resourceManager = LocalizationResourceManager.Current;

        if (value is null)
            return TargetNullText is null ? string.Empty : resourceManager[TargetNullText];

        if (value is ConvertValue convertValue)
        {
            if ((convertValue & ConvertValue.NullText) != 0)
            {
                return (convertValue & ConvertValue.NullValue) != 0 ?
                    resourceManager[TargetNullText!, TargetNullValue!] :
                    resourceManager[TargetNullText!];
            }

            if ((convertValue & ConvertValue.NullValue) != 0)
            {
                value = TargetNullValue!;
            }
            else if ((convertValue & ConvertValue.FallbackText) != 0)
            {
                return (convertValue & ConvertValue.FallbackValue) != 0 ?
                    resourceManager[FallbackText!, FallbackValue!] :
                    resourceManager[FallbackText!];
            }
            else if ((convertValue & ConvertValue.FallbackValue) != 0)
            {
                value = FallbackValue!;
            }
        }

        var numInfo = GetNumericInfo(value);
        if (!string.IsNullOrWhiteSpace(TranslateZero) && numInfo.IsZero)
            return resourceManager[TranslateZero, value];

        if (!string.IsNullOrWhiteSpace(TranslateOne) && numInfo.IsOne)
            return resourceManager[TranslateOne, value];

        if (!string.IsNullOrWhiteSpace(TranslateNegative) && numInfo.IsNegative)
            return resourceManager[TranslateNegative, value];

        if (!string.IsNullOrWhiteSpace(TranslatePositive) && numInfo.IsPositive)
            return resourceManager[TranslatePositive, value];

        if (!string.IsNullOrWhiteSpace(TranslateTrue) && value is true)
            return resourceManager[TranslateTrue];

        if (!string.IsNullOrWhiteSpace(TranslateFalse) && value is false)
            return resourceManager[TranslateFalse];

        if (!string.IsNullOrWhiteSpace(TranslateFormat))
            return resourceManager[TranslateFormat, value];

        if (TranslateValue)
            return resourceManager[value.ToString() ?? string.Empty];

        return value;
    }

    private static (bool IsZero, bool IsOne, bool IsNegative, bool IsPositive) GetNumericInfo(object obj) => obj switch
    {
        int i => (i == 0, i == 1, i < 0, i > 0),
        double d => (d == 0.0, d == 1.0, d < 0, d > 0),
        decimal dec => (dec == 0m, dec == 1m, dec < 0, dec > 0),
        float f => (f == 0f, f == 1f, f < 0, f > 0),
        long l => (l == 0, l == 1, l < 0, l > 0),
        sbyte sb => (sb == 0, sb == 1, sb < 0, sb > 0),
        short s => (s == 0, s == 1, s < 0, s > 0),
        _ => default // Returns (false, false, false, false) for non-numbers
    };

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}