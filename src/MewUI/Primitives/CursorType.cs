namespace Aprillz.MewUI;

/// <summary>
/// Specifies the type of cursor to display.
/// </summary>
public enum CursorType
{
    /// <summary>No cursor override; inherits from parent or uses the platform default.</summary>
    None,
    /// <summary>Standard arrow cursor.</summary>
    Arrow,
    /// <summary>I-beam cursor for text selection.</summary>
    IBeam,
    /// <summary>Wait / busy cursor.</summary>
    Wait,
    /// <summary>Crosshair cursor.</summary>
    Cross,
    /// <summary>Up-arrow cursor.</summary>
    UpArrow,
    /// <summary>Northwest-southeast resize cursor.</summary>
    SizeNWSE,
    /// <summary>Northeast-southwest resize cursor.</summary>
    SizeNESW,
    /// <summary>Horizontal (west-east) resize cursor.</summary>
    SizeWE,
    /// <summary>Vertical (north-south) resize cursor.</summary>
    SizeNS,
    /// <summary>Four-directional move cursor.</summary>
    SizeAll,
    /// <summary>Not-allowed / forbidden cursor.</summary>
    No,
    /// <summary>Hand / pointer cursor for links.</summary>
    Hand,
    /// <summary>Help cursor (arrow with question mark).</summary>
    Help,
}
