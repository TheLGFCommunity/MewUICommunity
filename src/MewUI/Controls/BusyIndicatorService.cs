using Aprillz.MewUI.Rendering;

namespace Aprillz.MewUI.Controls;

/// <summary>
/// Represents an active busy indicator. Dispose to dismiss.
/// </summary>
public interface IBusyIndicator : IDisposable
{
    /// <summary>
    /// Gets a cancellation token that is cancelled when the user confirms abort.
    /// Always returns <see cref="CancellationToken.None"/> if the indicator is not cancellable.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Updates the progress message displayed below the progress ring.
    /// </summary>
    void NotifyProgress(string message);
}

/// <summary>
/// Overlay service for showing busy indicators with optional abort support.
/// </summary>
public sealed class BusyIndicatorService : IOverlayService
{
    private readonly OverlayLayer _layer;

    internal BusyIndicatorService(OverlayLayer layer)
    {
        _layer = layer;
    }

    /// <summary>
    /// Creates and shows a busy indicator.
    /// Dispose the returned <see cref="IBusyIndicator"/> to dismiss it.
    /// </summary>
    /// <param name="message">Initial progress message.</param>
    /// <param name="cancellable">If <c>true</c>, an Abort button is shown.</param>
    public IBusyIndicator Create(string? message = null, bool cancellable = false)
    {
        return new BusyIndicatorHandle(_layer, message, cancellable);
    }
}

internal sealed class BusyIndicatorHandle : IBusyIndicator
{
    private readonly OverlayLayer _layer;
    private readonly BusyIndicatorPresenter _presenter;
    private readonly CancellationTokenSource? _cts;
    private bool _disposed;

    internal BusyIndicatorHandle(OverlayLayer layer, string? message, bool cancellable)
    {
        _layer = layer;
        _cts = cancellable ? new CancellationTokenSource() : null;
        _presenter = new BusyIndicatorPresenter(message, cancellable, _cts);
        _layer.Add(_presenter);
        _presenter.FadeIn();
    }

    public CancellationToken CancellationToken => _cts?.Token ?? CancellationToken.None;

    public void NotifyProgress(string message)
    {
        if (_disposed) return;
        _presenter.UpdateMessage(message);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _presenter.FadeOut(() =>
        {
            _layer.Remove(_presenter);
            _presenter.StopAnimation();
            _cts?.Dispose();
        });
    }
}

/// <summary>
/// Internal presenter control for busy indicator.
/// Grid layout: Row 0 (*) = bottom-aligned ring + abort, Row 1 (Auto) = spacing, Row 2 (*) = top-aligned message.
/// </summary>
internal sealed class BusyIndicatorPresenter : ContentControl
{
    private const int FadeDurationMs = 200;

    private readonly ProgressRing _ring;
    private readonly Label _messageLabel;
    private readonly CancellationTokenSource? _cts;
    private readonly bool _cancellable;
    private Animation.AnimationClock? _fadeClock;
    private double _opacity;

    // Abort UI elements — only created when cancellable
    private readonly Label? _abortLabel;
    private readonly Label? _confirmLabel;
    private readonly Label? _yesLabel;
    private readonly Label? _noLabel;
    private readonly Label? _abortingLabel;
    private readonly StackPanel? _normalPanel;
    private readonly StackPanel? _confirmPanel;
    private readonly StackPanel? _abortArea;

    private enum AbortState { Normal, Confirming, Aborting }
    private AbortState _abortState = AbortState.Normal;

    public double RingSize { get; set => field = Math.Max(32, Math.Min(256, value)); } = 64;

    internal BusyIndicatorPresenter(string? message, bool cancellable, CancellationTokenSource? cts)
    {
        _cancellable = cancellable;
        _cts = cts;

        _ring = new ProgressRing
        {
            IsActive = true,
            Width = RingSize,
            Height = RingSize,
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        _ring.WithTheme((t, c) => c.Foreground = t.Palette.Accent);

        _messageLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(8, 4),
            Margin = new Thickness(0, 12, 0, 0),
            IsVisible = message != null,
        };
        _messageLabel.WithTheme((t, c) =>
        {
            c.Foreground = t.Palette.WindowText;
            c.Background = t.Palette.ControlBackground;
        });

        if (message != null)
            _messageLabel.Text = message;

        // Center stack: ring + abort + message, all vertically stacked
        var centerStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        centerStack.Vertical().Spacing(4);
        centerStack.Add(_ring);

        if (cancellable)
        {
            // Normal state: "Abort" label
            _abortLabel = new Label { Text = MewUIStrings.Abort };
            _abortLabel.WithTheme((t, c) => c.Foreground = t.Palette.WindowText);
            _abortLabel.MouseDown += OnAbortClicked;

            _normalPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            _normalPanel.Horizontal();
            _normalPanel.Add(_abortLabel);

            // Confirming state: "Are you sure...?  YES  NO"
            _confirmLabel = new Label { Text = MewUIStrings.AbortConfirmation };
            _confirmLabel.WithTheme((t, c) => c.Foreground = t.Palette.WindowText);

            _yesLabel = new Label { Text = MewUIStrings.Yes };
            _yesLabel.WithTheme((t, c) => c.Foreground = t.Palette.Accent);
            _yesLabel.MouseDown += OnYesClicked;

            _noLabel = new Label { Text = MewUIStrings.No };
            _noLabel.WithTheme((t, c) => c.Foreground = t.Palette.WindowText);
            _noLabel.MouseDown += OnNoClicked;

            _confirmPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, IsVisible = false };
            _confirmPanel.Horizontal().Spacing(8);
            _confirmPanel.Add(_confirmLabel);
            _confirmPanel.Add(_yesLabel);
            _confirmPanel.Add(_noLabel);

            // Aborting state label
            _abortingLabel = new Label { Text = MewUIStrings.Aborting, IsVisible = false };
            _abortingLabel.WithTheme((t, c) => c.Foreground = t.Palette.WindowText);

            // Container that switches between the three states
            _abortArea = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            _abortArea.Add(_normalPanel);
            _abortArea.Add(_confirmPanel);
            _abortArea.Add(_abortingLabel);

            centerStack.Add(_abortArea);
        }

        centerStack.Add(_messageLabel);

        // Build grid: *,Auto,* — centers everything vertically
        var grid = new Grid();
        grid.Rows("2*,Auto,*");

        centerStack.Row(1);
        grid.Add(centerStack);

        Content = grid;
        IsHitTestVisible = true; // block input to controls behind the overlay
    }

    internal void FadeIn()
    {
        _fadeClock?.Stop();
        _fadeClock = new Animation.AnimationClock(TimeSpan.FromMilliseconds(FadeDurationMs), Animation.Easing.EaseOutCubic);
        _fadeClock.TickCallback = progress =>
        {
            _opacity = (float)progress;
            InvalidateVisual();
        };
        _fadeClock.Start();
    }

    internal void FadeOut(Action onCompleted)
    {
        _fadeClock?.Stop();
        _fadeClock = new Animation.AnimationClock(TimeSpan.FromMilliseconds(FadeDurationMs), Animation.Easing.EaseInCubic);
        _fadeClock.TickCallback = progress =>
        {
            _opacity = 1.0 - progress;
            InvalidateVisual();
        };
        _fadeClock.CompletedCallback = onCompleted;
        _fadeClock.Start();
    }

    protected override void OnRender(IGraphicsContext context)
    {
        if (_opacity <= 0) return;

        context.Save();
        context.GlobalAlpha *= (float)_opacity;

        // Dim the entire window with a semi-transparent background
        var bg = Theme.Palette.ControlBackground;
        context.FillRectangle(Bounds, Color.FromArgb(Theme.IsDark ? (byte)192 : (byte)160, bg.R, bg.G, bg.B)); // ~25% opacity

        base.OnRender(context);
        context.Restore();
    }

    protected override void RenderSubtree(IGraphicsContext context)
    {
        if (_opacity <= 0) return;
        base.RenderSubtree(context);
    }

    internal void UpdateMessage(string message)
    {
        _messageLabel.Text = message;
        _messageLabel.IsVisible = true;
    }

    internal void StopAnimation()
    {
        _ring.IsActive = false;
    }

    private void SetAbortState(AbortState state)
    {
        if (!_cancellable) return;
        _abortState = state;

        _normalPanel!.IsVisible = state == AbortState.Normal;
        _confirmPanel!.IsVisible = state == AbortState.Confirming;
        _abortingLabel!.IsVisible = state == AbortState.Aborting;
    }

    private void OnAbortClicked(MouseEventArgs e)
    {
        SetAbortState(AbortState.Confirming);
        e.Handled = true;
    }

    private void OnYesClicked(MouseEventArgs e)
    {
        SetAbortState(AbortState.Aborting);
        _cts?.Cancel();
        e.Handled = true;
    }

    private void OnNoClicked(MouseEventArgs e)
    {
        SetAbortState(AbortState.Normal);
        e.Handled = true;
    }
}
