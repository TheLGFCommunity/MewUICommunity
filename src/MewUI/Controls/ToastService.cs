namespace Aprillz.MewUI.Controls;

/// <summary>
/// Overlay service for showing toast notifications.
/// Internally manages a <see cref="ToastPresenter"/> on the <see cref="OverlayLayer"/>.
/// </summary>
public sealed class ToastService : IOverlayService
{
    private readonly OverlayLayer _layer;
    private ToastPresenter? _presenter;

    internal ToastService(OverlayLayer layer)
    {
        _layer = layer;
    }

    /// <summary>
    /// Shows a toast notification. Auto-dismisses after a duration based on text length.
    /// </summary>
    public void Show(string text)
    {
        _presenter ??= new ToastPresenter();

        if (!_layer.Contains(_presenter))
            _layer.Add(_presenter);

        var t = text ?? string.Empty;
        _presenter.Show(t, ToastPresenter.ComputeDuration(t));
    }
}
