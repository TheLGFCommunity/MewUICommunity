namespace Aprillz.MewUI.Controls;

/// <summary>
/// Fluent API extension methods for <see cref="StyleSheet"/>.
/// </summary>
public static class StyleSheetExtensions
{
    /// <summary>
    /// Defines a named style and returns the sheet for chaining.
    /// </summary>
    public static StyleSheet With(this StyleSheet sheet, string name, Style style)
    {
        sheet.Define(name, style);
        return sheet;
    }

    /// <summary>
    /// Defines a type-based style rule and returns the sheet for chaining.
    /// </summary>
    public static StyleSheet With<T>(this StyleSheet sheet, Style style) where T : Control
    {
        sheet.Define<T>(style);
        return sheet;
    }
}
