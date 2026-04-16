# 主题

本文档介绍 MewUI 的主题。

---

## 1. 主题输入

在 MewUI 中，`Theme` 由以下四个输入派生而来：

- `ThemeVariant`：`System` / `Light` / `Dark`
- `Accent`（或 `Color`）：主题色
- `ThemeSeed`：浅色/深色变体的基础颜色种子
- `ThemeMetrics`：“外观与感觉”的度量值，例如尺寸、内边距、笔划和字体

以下所有示例均在调用 `Run(...)` **之前**配置**默认值**。

### 1.1 ThemeVariant

此输入用于选择浅色/深色模式，或在使用 `System` 时跟随操作系统设置。

```csharp
using Aprillz.MewUI;

// 默认为 ThemeVariant.System。
// 如果你接受 System 设置，可以省略此项。
ThemeManager.Default = ThemeVariant.System;
// ThemeManager.Default = ThemeVariant.Light;
// ThemeManager.Default = ThemeVariant.Dark;
```

### 1.2 Accent

此输入用于提供主题色。你可以使用内置的 `Accent.*` 预设，或提供自定义的 `Color`。

```csharp
using Aprillz.MewUI;

ThemeManager.DefaultAccent = Accent.Blue;
```

注意：自定义 `Color` 通常更多地用于**运行时更改**（参见第 3.2 节）。

### 1.3 ThemeSeed

此输入为浅色和深色变体提供基础颜色种子。

`ThemeSeed` 上常用的属性：
- `WindowBackground`：窗口背景
- `WindowText`：默认文本颜色
- `ControlBackground`：控件背景
- `ButtonFace`：默认按钮背景
- `ButtonDisabledBackground`：禁用按钮背景

```csharp
using Aprillz.MewUI;

ThemeManager.DefaultLightSeed = ThemeSeed.DefaultLight;
ThemeManager.DefaultDarkSeed  = ThemeSeed.DefaultDark;
```

### 1.4 ThemeMetrics

此输入提供全局 UI 度量值，例如基础控件尺寸、内边距、圆角半径和字体。

`ThemeMetrics` 上常用的属性：
- `FontFamily`、`FontSize`、`FontWeight`
- `BaseControlHeight`
- `ControlCornerRadius`
- `ItemPadding`
- `ScrollBarThickness`、`ScrollBarHitThickness`、`ScrollBarMinThumbLength`
- `ScrollWheelStep`、`ScrollBarSmallChange`、`ScrollBarLargeChange`

```csharp
using Aprillz.MewUI;

ThemeManager.DefaultMetrics = ThemeMetrics.Default with
{
    ControlCornerRadius = 6,
    FontSize = 13,
    FontFamily = "Noto Sans"
};
```

---

## 2. 启动时的主题设置

推荐顺序：
1) 首先配置 `ThemeManager.Default*`
2) 构建 UI
3) 调用 `Application.Run(...)`

### 2.1 ThemeSeed 自定义示例

```csharp
using Aprillz.MewUI;

// 你无需重新分配所有默认值。
// 只需覆盖你要更改的部分。

ThemeManager.DefaultLightSeed = ThemeSeed.DefaultLight with
{
    WindowText = Color.FromRgb(20, 20, 20)
};

ThemeManager.DefaultDarkSeed = ThemeSeed.DefaultDark with
{
    WindowText = Color.FromRgb(240, 240, 240)
};

var mainWindow = new Window()
    .Title("Theme Seed Demo")
    .Content(new TextBlock().Text("Hello, MewUI").Bold());

Application.Run(mainWindow);
```

### 2.2 通过 ApplicationBuilder 应用设置

涵盖主题：
- 通过生成器 `UseTheme/UseAccent/UseSeed/UseMetrics` 应用主题输入
- （可选）当引用相应包时，链式选择平台/后端（例如 `UseWin32/UseDirect2D`）

注意：
- 该生成器会在 `Run(...)` 之前将这些值应用到 `ThemeManager.Default*`。

```csharp
using Aprillz.MewUI;
using Aprillz.MewUI.Backends;
using Aprillz.MewUI.PlatformHosts;

var mainWindow = new Window()
    .Title("Theme + Builder")
    .Content(new TextBlock().Text("Hello"));

Application.Create()
    .UseMetrics(ThemeMetrics.Default with { ControlCornerRadius = 6, FontSize = 13, FontFamily = "Noto Sans" })
    .UseSeed(
        ThemeSeed.DefaultLight with { WindowText = Color.FromRgb(20, 20, 20) },
        ThemeSeed.DefaultDark  with { WindowText = Color.FromRgb(240, 240, 240) })
    // 如果需要，也可以配置模式/主题色（System/Blue 是默认值，因此可以省略）
    // .UseTheme(ThemeVariant.System)
    // .UseAccent(Accent.Blue)
    .UseWin32()
    .UseDirect2D()
    .Run(mainWindow);
```

---

## 3. 运行时主题更改

通常支持以下两种运行时更改：

- 切换 `ThemeVariant`
- 切换 `Accent`

### 3.1 切换 ThemeVariant

```csharp
Application.Current.SetTheme(ThemeVariant.Dark);
// Application.Current.SetTheme(ThemeVariant.Light);
// Application.Current.SetTheme(ThemeVariant.System);
```

### 3.2 切换 Accent

```csharp
Application.Current.SetAccent(Accent.Green);

// 自定义颜色
Application.Current.SetAccent(new Color(0xFF, 0x22, 0x88, 0xFF));
```

---

## 4. 主题更改回调

当主题更改（主题变体切换、`System` 模式下的操作系统主题更改或依赖属性更新）时，你可能希望根据当前主题重新应用某些属性。

为此，请使用 `WithTheme((theme, control) => ...)`。

```csharp
var accentButton = new Button()
    .Text("Accent Button")
    .WithTheme((theme, c) =>
    {
        c.Background(theme.Palette.Accent);
        c.Foreground(theme.Palette.AccentText);
    });
```

---

## 5. 主题更改事件

```csharp
Application.Current.ThemeChanged += (oldTheme, newTheme) =>
{
    // 日志记录 / 持久化等操作
};
```

