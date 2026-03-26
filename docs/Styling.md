# Styling

This document describes MewUI's styling system — a code-first, AOT-friendly approach to reusable, state-aware visual customization.

---

## 1. Overview

MewUI's styling system is built around the following principles:

- **Code-first**: styles are C# objects with typed setters, not XML or CSS
- **AOT-friendly**: no reflection — generic interfaces, typed delegates, and static lambdas
- **Declarative**: state-based visuals are defined via `StateTrigger`, not imperative event handlers
- **Composable**: styles extend other styles via `BasedOn`; containers propagate styles via `StyleScope`

### Value resolution order

```
Local value (control.Background = ...)
  ↓  if not set
Animated value (transition in progress)
  ↓  if not animating
Trigger value (StateTrigger match)
  ↓  if no trigger matches
Style base setter
  ↓  if no style setter
Inherited value (parent chain)
  ↓  if not inherited
Default value
```

### Style resolution order

```
StyleName → StyleSheet lookup  (highest priority)
  ↓  if not found
StyleScope → type-matched rule
  ↓  if not found
Theme.GetStyle(type)           (lowest priority)
```

---

## 2. Style

A `Style` defines base property values, state-conditional triggers, and transitions for a control type.

### 2.1 Basic style

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

### 2.2 Theme-aware setters

Setters can use `Func<Theme, T>` to resolve values dynamically based on the current theme. The style instance is created once and shared — no recreation needed on theme change.

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

Triggers conditionally apply setters when the control's visual state matches. They override base setters for the same property.

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

Available flags: `Enabled`, `Hot`, `Focused`, `Pressed`, `Checked`, `Indeterminate`, `Active`, `Selected`, `ReadOnly`.

### 2.4 Transitions

Transitions animate property changes between states (e.g., hover color fade).

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

A style can inherit from another style. The derived style's setters and triggers override the base for the same properties.

```csharp
// Inherit from the default Button theme style
var myButton = new Style(typeof(Button))
{
    BasedOn = Style.ForType<Button>(),
    Setters =
    [
        // Only override what you need — rest comes from BasedOn
        Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.Accent),
    ],
};
```

`Style.ForType<T>()` returns the default theme style without requiring a Theme instance.

> **Policy**: if `BasedOn` is not set, only the setters/triggers defined in this style apply. The framework does not auto-merge with the theme style. This matches WPF behavior and keeps styling predictable.

---

## 3. StyleSheet

`StyleSheet` is a named registry of styles. Attach it to any `FrameworkElement` (typically a `Window`). Controls with `StyleName` set resolve their style from the nearest `StyleSheet` up the element tree.

### 3.1 Defining and applying

```csharp
// Define on a window
window.StyleSheet = new StyleSheet();
window.StyleSheet.Define("accent-button", accentButton);
window.StyleSheet.Define("flat-button", flatButtonStyle);

// Apply to a control
var btn = new Button { StyleName = "accent-button" };
btn.Content("Save");
```

### 3.2 Resolution

When `StyleName` is set, MewUI walks the parent chain to find the nearest `FrameworkElement` with a `StyleSheet`, then looks up the name. If not found, falls through to StyleScope and Theme defaults.

---

## 4. StyleScope

`StyleScope` applies type-matched styles to all descendant controls within a container. Useful for styling a group of controls without setting `StyleName` on each one.

### 4.1 Basic usage

```csharp
var toolbar = new StackPanel().Horizontal().Spacing(4);
toolbar.StyleScope = new StyleScope();
toolbar.StyleScope.Add<Button>(flatButtonStyle);

// All Buttons inside toolbar get flatButtonStyle automatically
toolbar.Add(new Button().Content("Cut"));
toolbar.Add(new Button().Content("Copy"));
toolbar.Add(new Button().Content("Paste"));
toolbar.Add(new CheckBox().Content("Bold")); // unaffected — only Button is scoped
```

### 4.2 Type matching

`StyleScope` matches by exact type first, then base types. `Add<Button>(style)` applies to `Button` and its subclasses.

### 4.3 Nested scopes

Inner scopes override outer scopes for the same type. Different types bubble independently.

```csharp
// Outer: all Buttons are flat
outerPanel.StyleScope = new StyleScope();
outerPanel.StyleScope.Add<Button>(flatButtonStyle);

// Inner: Buttons here are accent instead
innerPanel.StyleScope = new StyleScope();
innerPanel.StyleScope.Add<Button>(accentButtonStyle);

// Result:
// outerPanel > Button → flat
// innerPanel > Button → accent
// outerPanel > CheckBox → unaffected (no scope rule)
```

### 4.4 Named style references

`StyleScope` can reference named styles from `StyleSheet` instead of direct style objects:

```csharp
toolbar.StyleScope = new StyleScope();
toolbar.StyleScope.Add<Button>("flat-button"); // resolved from nearest StyleSheet
```

---

## 5. Property value sources

Each property value has a source that determines its priority:

| Source | Priority | Description |
|--------|----------|-------------|
| `Local` | Highest | Directly set on the control (e.g., `button.Background = Color.Red`) |
| `Trigger` | High | Set by a matching `StateTrigger` |
| `Style` | Medium | Set by a `Style` base setter |
| `Inherited` | Low | Inherited from parent (e.g., `Foreground` from `Window`) |
| `Default` | Lowest | Property's default value |

### Local values and triggers

When a property has a `Local` value, triggers and style setters are ignored for that property. This matches WPF behavior.

```csharp
var btn = new Button().Content("Red Button");
btn.Background = Color.Red; // Local value — hover trigger won't change this
```

### Foreground inheritance

`Foreground` is set on `Window` and inherited by all descendants. Individual controls do **not** set `Foreground` in their base style. Disabled triggers on specific controls (Button, TextBox, etc.) override with `DisabledText` when needed.

---

## 6. Theme integration

Styles use `Func<Theme, T>` setters to react to theme changes automatically:

```csharp
// This style works in both Light and Dark themes without recreation
Setter.Create(Control.BackgroundProperty, (Theme t) => t.Palette.ButtonFace)
```

When the theme changes:
1. `ResolveAndApplyStyle()` re-runs on each control
2. Same `Style` instance is reused (styles are static/shared)
3. `ResolveValue(newTheme)` produces new colors from the new palette
4. Transitions animate the color change smoothly

### Style.ForType

Since styles are shared globally (not per-theme), you can reference them statically:

```csharp
// No Theme instance needed
var baseStyle = Style.ForType<Button>();
```

---

## 7. Complete example

```csharp
// Define styles (static, shared, theme-aware)
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

// Register in StyleSheet
window.StyleSheet = new StyleSheet();
window.StyleSheet.Define("accent", accentButton);

// Apply via StyleScope (container-level)
var toolbar = new StackPanel().Horizontal().Spacing(4);
toolbar.StyleScope = new StyleScope();
toolbar.StyleScope.Add<Button>(flatButton);
toolbar.Add(new Button().Content("Cut"));
toolbar.Add(new Button().Content("Copy"));

// Apply via StyleName (per-element)
var saveBtn = new Button { StyleName = "accent" };
saveBtn.Content("Save");
toolbar.Add(saveBtn);

// Local override — ignores all style triggers
var customBtn = new Button().Content("Custom");
customBtn.Background = Color.FromRgb(200, 60, 60);
toolbar.Add(customBtn);
```
