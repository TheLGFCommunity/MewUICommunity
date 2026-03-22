namespace Aprillz.MewUI;

/// <summary>
/// Centralized UI strings for localization.
/// Default values are English. Override via static setters at application startup to provide translations.
/// </summary>
public static class MewUIStrings
{
    // MessageBox
    public static string OK { get; set; } = "OK";
    public static string Cancel { get; set; } = "Cancel";
    public static string Yes { get; set; } = "Yes";
    public static string No { get; set; } = "No";

    // BusyIndicator
    public static string Abort { get; set; } = "Abort";
    public static string AbortConfirmation { get; set; } = "Are you sure you want to abort this operation?";
    public static string Aborting { get; set; } = "Aborting...";
}
