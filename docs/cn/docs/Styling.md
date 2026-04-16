# 样式

本文档介绍 MewUI 的样式系统 —— 一种代码优先、对 AOT 编译友好的、可复用且能感知状态的可视化定制方法。

---

## 1. 概述

MewUI 的样式系统遵循以下原则构建：

- **代码优先**：样式是带有类型化设置器的 C# 对象，而不是 XML 或 CSS
- **对 AOT 友好**：无反射 —— 使用泛型接口、类型化委托和静态 Lambda 表达式
- **声明式**：基于状态的视觉表现通过 `StateTrigger` 定义，而不是命令式事件处理程序
- **可组合**：样式通过 `BasedOn` 继承其他样式；容器通过 `StyleSheet` 传播样式

### 值解析顺序

```
本地值（control.Background = ...）
  ↓ 如果未设置
动画值（过渡动画进行中）
  ↓ 如果没有动画
触发器值（StateTrigger 匹配）
  ↓ 如果没有触发器匹配
样式基础设置器
  ↓ 如果没有样式设置器
继承值（父级链）
  ↓ 如果没有继承
默认值
```

### 样式解析顺序

```
当设置了 StyleName 时：
  沿父级链（从自身开始）在每个 StyleSheet 中按名称查找
    → 然后查找 Application.StyleSheet
      → 如果未找到，则继续下面的过程

当未设置 StyleName 或未找到名称时：
  沿父级链在每个 StyleSheet 中查找基于类型的规则
    → 如果未找到，则继续下面的过程

DefaultStyles（主题默认样式）    （最低优先级）
```

---

## 2. Style

`Style` 为控件类型定义基本属性值、条件状态触发器以及过渡动画。

### 2.1 基本样式

```csharp
var flatButtonStyle = new Style(typeof(Button))
{
    Setters =
    [
        Setter.Create(Control.BackgroundProperty, Color.Transparent),
        Setter.Create(Control.BorderThicknessProperty, 0.0),
    ],
};
```

### 2.2 主题感知的设置器

设置器可以使用 `Func<Theme, T>` 根据当前主题动态解析值。样式实例创建一次并共享 —— 主题更改时无需重新创建。

```csharp
var accentButton = new Style(typeof(Button))
{
    Setters =
    [
        Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.Accent),
        Setter.Create(Control.ForegroundProperty, (Theme t) => t.Palette.AccentText),
        Setter.Create(Control.BorderBrushProperty, (Theme t) => t.Palette.Accent),
    ],
};
```

### 2.3 StateTrigger

当控件的视觉状态匹配时，触发器有条件地应用设置器。对于同一属性，它们会覆盖基础设置器的值。

```csharp
var accentButton = new Style(typeof(Button))
{
    Setters =
    [
        Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.Accent),
        Setter.Create(Control.ForegroundProperty, (Theme t) => t.Palette.AccentText),
    ],
    Triggers =
    [
        new StateTrigger
        {
            Match = VisualStateFlags.Hot,
            Setters = [Setter.Create(Control.BackgroundProperty,
                (Theme t) => t.Palette.Accent.Lerp(t.Palette.WindowBackground, 0.15))],
        },
        new StateTrigger
        {
            Match = VisualStateFlags.Pressed,
            Setters = [Setter.Create(Control.BackgroundProperty,
                (Theme t) => t.Palette.Accent.Lerp(t.Palette.WindowBackground, 0.25))],
        },
        new StateTrigger
        {
            Match = VisualStateFlags.None,
            Exclude = VisualStateFlags.Enabled,
            Setters = [
                Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.ButtonDisabledBackground),
                Setter.Create(Control.ForegroundProperty, (Theme t) => t.Palette.DisabledText),
            ],
        },
    ],
};
```

可用标志：`Enabled`、`Hot`、`Focused`、`Pressed`、`Checked`、`Indeterminate`、`Active`、`Selected`、`ReadOnly`。

### 2.4 过渡动画

过渡动画用于状态之间（例如悬停颜色渐变）的属性变化。

```csharp
var style = new Style(typeof(Button))
{
    Transitions =
    [
        Transition.Create(Control.BackgroundProperty),
        Transition.Create(Control.BorderBrushProperty),
        Transition.Create(Control.ForegroundProperty),
    ],
    Setters = [...],
    Triggers = [...],
};
```

### 2.5 BasedOn

一个样式可以继承自另一个样式。派生样式的设置器和触发器会覆盖基样式中相同属性的设置。

```csharp
// 继承自默认的 Button 主题样式
var myButton = new Style(typeof(Button))
{
    BasedOn = Style.ForType<Button>(),
    Setters =
    [
        // 仅覆盖你需要的内容 —— 其余部分来自 BasedOn
        Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.Accent),
    ],
};
```

`Style.ForType<T>()` 返回默认的主题样式，无需 Theme 实例。

> **策略**：如果未设置 `BasedOn`，则仅应用此样式中定义的设置器/触发器。框架不会自动与主题样式合并。这与 WPF 行为一致，并使样式行为可预测。

---

## 3. StyleSheet

`StyleSheet` 是一个样式注册表，同时支持命名样式和基于类型的规则。可将其附加到任何 `FrameworkElement`（通常是 `Window`）。它有两个用途：

1. **命名样式**：设置了 `StyleName` 的控件会沿元素树向上查找最近的 `StyleSheet` 来解析其样式。
2. **基于类型的规则**：指定类型的所有后代控件会自动接收该样式，无需在每个控件上单独设置 `StyleName`。

### 3.1 命名样式

```csharp
// 在窗口上定义
window.StyleSheet = new StyleSheet();
window.StyleSheet.Define("accent-button", accentButton);
window.StyleSheet.Define("flat-button", flatButtonStyle);

// 应用于控件
var btn = new Button { StyleName = "accent-button" };
btn.Content("Save");
```

当设置了 `StyleName` 时，MewUI 会从控件自身开始沿父级链向上查找，在每个 `FrameworkElement` 的 `StyleSheet` 中按名称查找。如果父级链中没有 `StyleSheet` 包含该名称，最后会检查 `Application.StyleSheet`。如果仍未找到，则解析过程会回退到基于类型的规则，然后是主题默认样式（`DefaultStyles`）。

### 3.2 基于类型的规则

```csharp
var toolbar = new StackPanel().Horizontal().Spacing(4);
toolbar.StyleSheet = new StyleSheet();
toolbar.StyleSheet.Define<Button>(flatButtonStyle);

// toolbar 内部的所有 Button 都会自动获得 flatButtonStyle
toolbar.Add(new Button().Content("Cut"));
toolbar.Add(new Button().Content("Copy"));
toolbar.Add(new Button().Content("Paste"));
toolbar.Add(new CheckBox().Content("Bold")); // 不受影响 —— 只有 Button 匹配
```

类型匹配会首先检查确切类型，然后检查基类型。`Define<Button>(style)` 适用于 `Button` 及其子类。

### 3.3 嵌套的 StyleSheet

对于相同类型，内部 StyleSheet 会覆盖外部 StyleSheet。不同类型独立向上冒泡。

```csharp
// 外部：所有 Button 都是扁平样式
outerPanel.StyleSheet = new StyleSheet();
outerPanel.StyleSheet.Define<Button>(flatButtonStyle);

// 内部：这里的 Button 使用强调色样式
innerPanel.StyleSheet = new StyleSheet();
innerPanel.StyleSheet.Define<Button>(accentButtonStyle);

// 结果：
// outerPanel > Button → 扁平样式
// innerPanel > Button → 强调色样式
// outerPanel > CheckBox → 不受影响（无类型规则）
```

### 3.4 类型规则中引用命名样式

类型规则可以引用命名样式而不是直接的样式对象：

```csharp
toolbar.StyleSheet = new StyleSheet();
toolbar.StyleSheet.Define<Button>("flat-button"); // 从最近的 StyleSheet 解析
```

---

## 5. 属性值源

每个属性值都有一个决定其优先级的源：

| 源 | 优先级 | 描述 |
|--------|----------|-------------|
| `Local` | 最高 | 直接在控件上设置（例如 `button.Background = Color.Red`） |
| `Trigger` | 高 | 由匹配的 `StateTrigger` 设置 |
| `Style` | 中 | 由 `Style` 基础设置器设置 |
| `Inherited` | 低 | 从父级继承（例如从 `Window` 继承 `Foreground`） |
| `Default` | 最低 | 属性的默认值 |

### 本地值与触发器

当属性具有 `Local` 值时，该属性的触发器和样式设置器将被忽略。这与 WPF 行为一致。

```csharp
var btn = new Button().Content("Red Button");
btn.Background = Color.Red; // 本地值 —— 悬停触发器不会改变这个值
```

### Foreground 继承

`Foreground` 在 `Window` 上设置，并由所有后代继承。各个控件在其基本样式中**不会**设置 `Foreground`。特定控件（Button、TextBox 等）上的禁用触发器会在需要时用 `DisabledText` 覆盖。

---

## 6. 主题集成

样式使用 `Func<Theme, T>` 设置器来自动响应主题变化：

```csharp
// 此样式在浅色和深色主题中都能正常工作，无需重新创建
Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.ButtonFace)
```

当主题更改时：
1. `ResolveAndApplyStyle()` 在每个控件上重新运行
2. 复用相同的 `Style` 实例（样式是静态/共享的）
3. `ResolveValue(newTheme)` 从新调色板生成新颜色
4. 过渡动画平滑地改变颜色

### Style.ForType

由于样式是全局共享的（而非每个主题一个），你可以静态地引用它们：

```csharp
// 无需 Theme 实例
var baseStyle = Style.ForType<Button>();
```

---

## 7. 完整示例

```csharp
// 定义样式（静态、共享、主题感知）
var flatButton = new Style(typeof(Button))
{
    BasedOn = Style.ForType<Button>(),
    Setters =
    [
        Setter.Create(Control.BackgroundProperty,
            (Theme t) => t.Palette.ButtonHoverBackground.WithAlpha(0)),
        Setter.Create(Control.BorderBrushProperty, Color.Transparent),
        Setter.Create(Control.BorderThicknessProperty, 0.0),
    ],
    Triggers =
    [
        new StateTrigger
        {
            Match = VisualStateFlags.Hot,
            Setters = [Setter.Create(Control.BackgroundProperty,
                (Theme t) => t.Palette.ButtonHoverBackground)],
        },
    ],
};

var accentButton = new Style(typeof(Button))
{
    BasedOn = Style.ForType<Button>(),
    Setters =
    [
        Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.Accent),
        Setter.Create(Control.ForegroundProperty, (Theme t) => t.Palette.AccentText),
        Setter.Create(Control.BorderBrushProperty, (Theme t) => t.Palette.Accent),
    ],
    Triggers =
    [
        new StateTrigger
        {
            Match = VisualStateFlags.Hot,
            Setters = [
                Setter.Create(Control.BackgroundProperty,
                    (Theme t) => t.Palette.Accent.Lerp(t.Palette.WindowBackground, 0.15)),
            ],
        },
        new StateTrigger
        {
            Match = VisualStateFlags.Pressed,
            Setters = [
                Setter.Create(Control.BackgroundProperty,
                    (Theme t) => t.Palette.Accent.Lerp(t.Palette.WindowBackground, 0.25)),
            ],
        },
    ],
};

// 在 StyleSheet 中注册
window.StyleSheet = new StyleSheet();
window.StyleSheet.Define("accent", accentButton);

// 通过 StyleSheet 类型规则应用（容器级别）
var toolbar = new StackPanel().Horizontal().Spacing(4);
toolbar.StyleSheet = new StyleSheet();
toolbar.StyleSheet.Define<Button>(flatButton);
toolbar.Add(new Button().Content("Cut"));
toolbar.Add(new Button().Content("Copy"));

// 通过 StyleName 应用（每个元素级别）
var saveBtn = new Button { StyleName = "accent" };
saveBtn.Content("Save");
toolbar.Add(saveBtn);

// 本地覆盖 —— 忽略所有样式触发器
var customBtn = new Button().Content("Custom");
customBtn.Background = Color.FromRgb(200, 60, 60);
toolbar.Add(customBtn);
```
