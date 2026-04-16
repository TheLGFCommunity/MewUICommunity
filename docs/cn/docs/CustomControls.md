# 自定义控件

## 概述

- 本文档是用于在 MewUI 中构建自定义控件的**开发人员参考**。
- 布局使用 **DIP**，呈现需要**像素对齐的几何图形**。
- 下面的示例是一个完整的 `NumericUpDown` 控件，其中的注释从 CustomControl 的角度解释了**每个部分负责的功能**。

---

## 详细说明

### <a id="scope"></a>范围和约定

- 大小以 **DIP** 为单位进行计算，呈现时进行**像素对齐**。
- 测量/排列必须仅在**逻辑坐标 (DIP)** 中进行操作；像素对齐在 Render 中应用。
- 使用 `GetDpi()` / `context.DpiScale` 来响应 DPI 变化。
- **切勿在测量期间进行像素数学计算。** 将像素对齐混合到 Measure 中会导致布局不匹配。

### <a id="measure"></a>大小计算 (MeasureContent)

- `MeasureContent` 是**所需大小的唯一来源**。
- 此阶段仅计算控件需要多少空间；它不决定放置位置。
- 使用显示字符串（已应用格式）测量文本。
- 最终大小包括 `Padding`、镶边（按钮区域）和 `GetBorderVisualInset()`。
- 如果控件应与主题的基准大小对齐，请将 `DefaultMinHeight` 设置为 `Theme.Metrics.BaseControlHeight`。
- 在这种情况下，`MeasureContent` 可以返回自然的内容高度；框架会应用 `MinHeight`。
- `Format` 和 `Value` 更改可能会改变文本宽度，因此需要**使测量失效**。
- 即使有缓存，**当输入（字体、DPI、字符串、换行策略）更改时也应使其失效**。

示例：
```csharp
protected override double DefaultMinHeight => Theme.Metrics.BaseControlHeight;

protected override Size MeasureContent(Size available)
{
    var textHeight = /* 测量文本高度 */;
    double height = textHeight + Padding.VerticalThickness;
    return new Size(Width, height);
}
```

### <a id="arrange"></a>内部布局 (ArrangeContent)

- 此示例未重写 `ArrangeContent`；它使用最终的 `Bounds` 计算内部布局。
- 具有子控件的控件必须在此处计算子控件矩形并为每个子控件调用 `Arrange`。
- Arrange 定义了子控件在分配的空间内**放置的位置**。
- 始终假定 **Measure 得出的 DesiredSize** 和**实际的 Bounds** 可能不同。

### <a id="render"></a>呈现 (OnRender)

- `GetSnappedBorderBounds` 和 `LayoutRounding.SnapBoundsRectToPixels` 确保像素对齐。
- 呈现顺序：**背景 → 边框 → 内容**。
- 结构化代码，使布局数学计算和呈现共享相同的矩形。
- Render **不得**重新计算测量；它仅使用最终的 `Bounds`。

### <a id="state"></a>状态和输入

- 交互状态（悬停/按下）是控件的**内部状态**。
- 在 MouseDown 时捕获，在 MouseUp 时释放，以确保输入一致性。
- 命中测试逻辑必须使用与呈现**相同的分割几何图形**。
- 状态更改时，正确选择 **InvalidateVisual** 与 **InvalidateMeasure**。
- 使用 **`IsEffectivelyEnabled`** 而不是 `IsEnabled` 来控制输入。
  - 如果父控件被禁用，即使子控件的 `IsEnabled == true`，它也必须忽略输入。
  - 因此，输入处理、视觉状态和颜色决策应遵循 `IsEffectivelyEnabled`。

### <a id="theme"></a>主题和指标

- 颜色和大小来自 `Theme.Palette.*` 和 `Theme.Metrics.*`。
- 主题更改会使文本测量缓存失效。
- 主题更改可能会影响**字体、大小和填充规则**，因此重新测量更安全。

### <a id="utils"></a>实用工具方法（状态、边框和 DIP）

- `GetDpi()` 返回有效 DPI (`uint`)。在将 DIP 转换为设备像素时，使用 `dpiScale = GetDpi() / 96.0`。
- `GetVisualState(...)` 为当前帧创建 `enabled/hot/focused/pressed/active` 的稳定快照。
- `PickAccentBorder(theme, baseBorder, state, hoverMix)` 将该状态映射到边框颜色（聚焦/按下/活动时为重点色；悬停时着色）。
- `DrawBackgroundAndBorder(context, bounds, background, borderBrush, cornerRadiusDip)` 使用当前后端绘制一致的背景 + 边框。
- `GetBorderRenderMetrics(bounds, cornerRadiusDip)` 返回像素对齐的边框粗细和圆角半径，以使呈现与布局匹配。
- `LayoutRounding` 辅助工具可在分数 DPI 下保持几何图形稳定，并避免 1 像素的裁剪伪影：
- `LayoutRounding.SnapBoundsRectToPixels(...)` 用于背景/边框/布局框。
- `LayoutRounding.SnapViewportRectToPixels(...)` 用于视口和裁剪矩形（不会收缩）。
- `LayoutRounding.SnapThicknessToPixels(...)` 用于必须为整数像素的边框粗细。
- `LayoutRounding.ExpandClipByDevicePixels(...)` 用于必须包含最后一行/列的裁剪矩形。

示例：状态驱动的边框 + 像素对齐

```csharp
var dpiScale = GetDpi() / 96.0;
var state = GetVisualState(isPressed: isPressed, isActive: isActive);
var border = PickAccentBorder(Theme, BorderBrush, state, hoverMix: 0.6);

var bounds = LayoutRounding.SnapBoundsRectToPixels(Bounds, dpiScale);
DrawBackgroundAndBorder(context, bounds, Background, border, cornerRadiusDip: 0);
```

### <a id="invalidate"></a>失效规则

- `Format` 更改：`InvalidateMeasure()` + `InvalidateVisual()`
- `Value` 更改：文本宽度可能更改 → `InvalidateMeasure()`
- 悬停/按下状态更改：`InvalidateVisual()`

---

## 完整示例代码

```csharp
public sealed class NumericUpDown : RangeBase
{
    // 此枚举将交互状态保留在一个位置。
    // 自定义控件应将非公共 UI 状态保留为内部状态，
    // 以便输入处理和呈现共享相同的状态。
    private enum ButtonPart
    {
        None,
        Decrement,
        Increment
    }

    // 显示格式可能会影响所需大小。
    // 在自定义控件中，任何更改呈现文本的状态
    // 都必须视为影响布局。
    private string _format = "0.##";

    // 交互步长不影响布局，但对于输入逻辑是必需的。
    // 将其保留为交互处理程序使用的状态。
    private double _step = 1;

    // 缓存文本测量以减少 Measure 成本。
    // Measure 可能会频繁调用；缓存可稳定布局。
    private TextMeasureCache _measureCache;

    // 视觉状态；它们影响呈现但不影响布局。
    // 将其保留为内部状态并相应地驱动失效。
    private ButtonPart _hoverPart;
    private ButtonPart _pressedPart;

    public NumericUpDown()
    {
        // 建立默认大小规则。
        // 边框参与 Measure；设置一个安全的默认值。
        BorderThickness = 1;

        // 设置默认范围。
        Maximum = 100;

        // 将内容与镶边分开。
        // 自定义控件应保持内容和镶边区域区分开。
        Padding = new Thickness(8, 4, 8, 4);
    }

    // 提供主题默认值以获得一致的样式。
    protected override Color DefaultBackground => Theme.Palette.ControlBackground;
    protected override Color DefaultBorderBrush => Theme.Palette.ControlBorder;
    protected override double DefaultMinHeight => Theme.Metrics.BaseControlHeight;

    // 接受键盘输入的控件必须是可聚焦的。
    public override bool Focusable => true;

    public string Format
    {
        get => _format;
        set
        {
            // 避免冗余失效。
            if (_format == value)
            {
                return;
            }

            // 应用显示更改。
            _format = value;

            // 文本度量现已过时。
            _measureCache.Invalidate();

            // 所需大小可能会更改。
            InvalidateMeasure();

            // 需要视觉更新。
            InvalidateVisual();
        }
    }

    public double Step
    {
        get => _step;
        set
        {
            if (_step.Equals(value))
            {
                return;
            }

            // 更新交互步长。
            _step = value;

            // 当不影响布局或视觉对象时，无需失效。
        }
    }

    protected override void OnThemeChanged(Theme oldTheme, Theme newTheme)
    {
        base.OnThemeChanged(oldTheme, newTheme);

        // 主题更改可能会更改文本度量，因此使缓存失效。
        _measureCache.Invalidate();
    }

    protected override void OnValueChanged(double value, bool fromUser)
    {
        // 显示的文本宽度可能发生变化 → 重新测量。
        _measureCache.Invalidate();
        InvalidateMeasure();

        // 如果需要，可以添加视觉失效。
    }

    protected override Size MeasureContent(Size available)
    {
        // MeasureContent 定义自定义控件的所需大小。
        // 仅以 DIP 为单位计算；像素对齐属于 Render。
        var factory = GetGraphicsFactory();
        var font = GetFont(factory);

        // 测量实际的显示字符串。
        string text = Value.ToString(_format);
        var textSize = _measureCache.Measure(factory, GetDpi(), font, text, TextWrapping.NoWrap, 0);

        // 在大小中包含镶边区域。
        double buttonAreaWidth = GetButtonAreaWidth();

        // 内容 + 填充 + 镶边。
        double width = textSize.Width + Padding.HorizontalThickness + buttonAreaWidth;

        // 使用自然内容高度；MinHeight 强制实施基准大小。
        double height = textSize.Height + Padding.VerticalThickness;

        // 在所需大小中包含边框内边距。
        return new Size(width, height).Inflate(new Thickness(GetBorderVisualInset()));
    }

    protected override void OnRender(IGraphicsContext context)
    {
        // Render 在最终的 Bounds 已知后运行。
        // 使用像素对齐的几何图形以避免抖动。
        var bounds = GetSnappedBorderBounds(Bounds);

        // 样式值来自主题。
        double radius = Theme.Metrics.ControlCornerRadius;

        // 在绘制之前解析依赖于状态的颜色。
        bool isEnabled = IsEffectivelyEnabled;
        Color bg = isEnabled ? Background : Theme.Palette.DisabledControlBackground;
        Color baseBorder = isEnabled ? BorderBrush : Theme.Palette.ControlBorder;
        var state = GetVisualState(isPressed: _pressedPart != ButtonPart.None, isActive: _pressedPart != ButtonPart.None);
        Color border = PickAccentBorder(Theme, baseBorder, state, hoverMix: 0.6);

        // 首先绘制镶边。
        DrawBackgroundAndBorder(context, bounds, bg, border, radius);

        // 根据最终边界计算内部布局。
        var inner = bounds.Deflate(new Thickness(GetBorderVisualInset()));

        // 拆分内容和镶边区域。
        double buttonAreaWidth = Math.Min(GetButtonAreaWidth(), inner.Width);
        var buttonRect = new Rect(inner.Right - buttonAreaWidth, inner.Y, buttonAreaWidth, inner.Height);
        var textRect = new Rect(
            inner.X + Padding.Left,
            inner.Y + Padding.Top,
            Math.Max(0, inner.Width - buttonAreaWidth - Padding.HorizontalThickness),
            Math.Max(0, inner.Height - Padding.VerticalThickness));

        // 也将子矩形对齐到像素。
        textRect = LayoutRounding.SnapBoundsRectToPixels(textRect, context.DpiScale);
        buttonRect = LayoutRounding.SnapBoundsRectToPixels(buttonRect, context.DpiScale);

        // 输入和呈现必须共享相同的分割几何图形。
        var decRect = new Rect(buttonRect.X, buttonRect.Y, buttonRect.Width / 2, buttonRect.Height);
        var incRect = new Rect(buttonRect.X + buttonRect.Width / 2, buttonRect.Y, buttonRect.Width / 2, buttonRect.Height);

        // 根据状态解析每个区域的颜色。
        Color baseButton = Theme.Palette.ButtonFace;
        Color hoverButton = Theme.Palette.ButtonHoverBackground;
        Color pressedButton = Theme.Palette.ButtonPressedBackground;
        Color disabledButton = Theme.Palette.ButtonDisabledBackground;

        Color decBg = !isEnabled
            ? disabledButton
            : _pressedPart == ButtonPart.Decrement ? pressedButton
            : _hoverPart == ButtonPart.Decrement ? hoverButton
            : baseButton;

        Color incBg = !isEnabled
            ? disabledButton
            : _pressedPart == ButtonPart.Increment ? pressedButton
            : _hoverPart == ButtonPart.Increment ? hoverButton
            : baseButton;

        if (buttonRect.Width > 0)
        {
            // 绘制镶边区域。
            context.FillRectangle(decRect, decBg);

            var innerRadius = Math.Max(0, radius - GetBorderVisualInset());
            context.Save();
            context.SetClipRoundedRect(
                LayoutRounding.MakeClipRect(inner, context.DpiScale, rightPx: 0, bottomPx: 0),
                innerRadius,
                innerRadius);
            context.FillRectangle(incRect, incBg);
            context.Restore();

            // 用于清晰度的视觉分隔线。
            var x = decRect.Right;
            context.DrawLine(new Point(x, decRect.Y + 2), new Point(x, decRect.Bottom - 2), Theme.Palette.ControlBorder, 1);

            x = decRect.Left;
            context.DrawLine(new Point(x, decRect.Y), new Point(x, decRect.Bottom), Theme.Palette.ControlBorder, 1);
        }

        // 最后绘制文本，使其位于镶边之上。
        var font = GetFont();
        var textColor = isEnabled ? Foreground : Theme.Palette.DisabledText;
        context.DrawText(Value.ToString(_format), textRect, font, textColor, TextAlignment.Left, TextAlignment.Center, TextWrapping.NoWrap);

        if (buttonRect.Width > 0)
        {
            // 字形大小遵循主题指标。
            var chevronSize = Theme.Metrics.BaseControlHeight / 6;
            Glyph.Draw(context, decRect.Center, chevronSize, textColor, GlyphKind.ChevronDown);
            Glyph.Draw(context, incRect.Center, chevronSize, textColor, GlyphKind.ChevronUp);
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        // 禁用时阻止输入。
        if (!IsEffectivelyEnabled)
        {
            return;
        }

        // 将滚轮输入映射到值更改。
        double delta = e.Delta > 0 ? _step : -_step;
        Value += delta;

        // 值更改必须在视觉上反映出来。
        InvalidateVisual();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        // 输入入口点。
        // 决定接受哪些输入并建立焦点/捕获/状态。
        if (!IsEffectivelyEnabled || e.Button != MouseButton.Left)
        {
            return;
        }

        // 确保键盘焦点用于按键处理。
        Focus();

        // 将命中测试结果存储为状态。
        var part = HitTestButtonPart(e.Position);
        if (part == ButtonPart.None)
        {
            return;
        }

        _pressedPart = part;

        // 捕获保证 MouseUp 传递。
        var root = FindVisualRoot();
        if (root is Window window)
        {
            window.CaptureMouse(this);
        }

        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // 更新悬停状态以提供视觉反馈。
        var part = HitTestButtonPart(e.Position);
        if (_hoverPart != part)
        {
            _hoverPart = part;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeave()
    {
        base.OnMouseLeave();

        // 仅在未捕获时清除悬停状态。
        if (_hoverPart != ButtonPart.None && !IsMouseCaptured)
        {
            _hoverPart = ButtonPart.None;
            InvalidateVisual();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        // 输入退出点。
        // 在此处释放捕获并使状态正常化。
        if (e.Button != MouseButton.Left || _pressedPart == ButtonPart.None)
        {
            return;
        }

        // 释放捕获。
        var root = FindVisualRoot();
        if (root is Window window)
        {
            window.ReleaseMouseCapture();
        }

        // 仅当在同一区域上释放时才提交操作。
        var releasedPart = HitTestButtonPart(e.Position);
        if (releasedPart == _pressedPart && IsEffectivelyEnabled)
        {
            Value += _pressedPart == ButtonPart.Increment ? _step : -_step;
        }

        _pressedPart = ButtonPart.None;
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // 键盘路径独立于鼠标路径。
        // 检查焦点/启用状态，然后根据需要使其失效。
        if (!IsEffectivelyEnabled)
        {
            return;
        }

        if (e.Key == Key.Up)
        {
            Value += _step;
            InvalidateVisual();
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            Value -= _step;
            InvalidateVisual();
            e.Handled = true;
        }
    }

    // 集中定义镶边宽度规则。
    private double GetButtonAreaWidth() => Theme.Metrics.BaseControlHeight * 2;

    private (Rect decRect, Rect incRect) GetButtonRects()
    {
        // 命中测试和呈现必须共享相同的几何图形。
        var inner = GetSnappedBorderBounds(Bounds).Deflate(new Thickness(GetBorderVisualInset()));
        double buttonAreaWidth = Math.Min(GetButtonAreaWidth(), inner.Width);
        var buttonRect = new Rect(inner.Right - buttonAreaWidth, inner.Y, buttonAreaWidth, inner.Height);
        var decRect = new Rect(buttonRect.X, buttonRect.Y, buttonRect.Width / 2, buttonRect.Height);
        var incRect = new Rect(buttonRect.X + buttonRect.Width / 2, buttonRect.Y, buttonRect.Width / 2, buttonRect.Height);
        return (decRect, incRect);
    }

    private ButtonPart HitTestButtonPart(Point position)
    {
        // 在输入处理程序之间重用相同的命中测试逻辑。
        var (decRect, incRect) = GetButtonRects();
        if (decRect.Contains(position))
        {
            return ButtonPart.Decrement;
        }
        if (incRect.Contains(position))
        {
            return ButtonPart.Increment;
        }
        return ButtonPart.None;
    }
}
```
