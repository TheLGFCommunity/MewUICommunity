namespace Aprillz.MewUI.Controls;

public sealed class GridViewColumn<TItem>
{
    public string Header { get; set; } = string.Empty;

    public double Width { get; set; }

    /// <summary>
    /// Minimum column width enforced during resize. Default is 0 (no minimum).
    /// </summary>
    public double MinWidth { get; set; }

    /// <summary>
    /// Whether the column can be resized by dragging the header separator. Default is true.
    /// </summary>
    public bool IsResizable { get; set; } = true;

    public IDataTemplate<TItem>? CellTemplate { get; set; }
}
