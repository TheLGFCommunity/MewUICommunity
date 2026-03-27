# Data Binding Guide

MewUI's data binding system uses a delegate-based, reflection-free approach compatible with Native AOT.

---

## 1. Core Concepts

### Reflection-Free Binding

Unlike WPF/WinUI, MewUI does not use Reflection:

| WPF Approach | MewUI Approach |
|--------------|----------------|
| `{Binding PropertyName}` | `.BindText(vm.Name)` or `.Bind(property, source)` |
| `INotifyPropertyChanged` | `ObservableValue<T>` |
| PropertyPath strings | Direct property references |

Benefits:
- **Native AOT Compatible**: safe for trimming/AOT
- **Compile-time Validation**: prevents property name typos
- **IntelliSense Support**: auto-completion available
- **Refactoring Safe**: automatically reflects renames

### Binding Modes

```csharp
public enum BindingMode
{
    OneWay,   // Source → Control only
    TwoWay,  // Source ↔ Control (bidirectional)
}
```

The default mode is determined by the property: input properties (e.g., `TextBox.TextProperty`) default to `TwoWay`, display properties (e.g., `Label.TextProperty`) default to `OneWay`.

---

## 2. ObservableValue\<T>

A reactive value container that automatically updates the UI when the value changes.

### Basic Usage

```csharp
var name = new ObservableValue<string>("Default");
var count = new ObservableValue<int>(0);
var isEnabled = new ObservableValue<bool>(true);

// Read/Write
string current = name.Value;
name.Value = "New Value";

// Change notification
name.Changed += () => Console.WriteLine("Name changed!");
```

### Coerce (Value Constraints)

```csharp
var percent = new ObservableValue<double>(50, v => Math.Clamp(v, 0, 100));
percent.Value = 150;  // → 100
percent.Value = -10;  // → 0

var text = new ObservableValue<string>("", v => v?.Trim() ?? "");
```

---

## 3. Binding APIs

MewUI provides three levels of binding:

### 3.1 Fluent extension methods (recommended)

High-level, per-control convenience methods for common properties.

```csharp
var name = new ObservableValue<string>("");
var count = new ObservableValue<int>(0);
var isChecked = new ObservableValue<bool>(false);

// Text binding (two-way for TextBox, one-way for Label)
new TextBox().BindText(name)
new Label().BindText(name)

// Conversion binding
new Label().BindText(count, c => $"Count: {c}")

// CheckBox / ToggleSwitch
new CheckBox().BindIsChecked(isChecked)

// Slider / ProgressBar
new Slider().BindValue(volume)

// Visibility / Enabled
new Button().BindIsVisible(isVisible).BindIsEnabled(isEnabled)
```

### 3.2 Generic Bind\<T> (MewProperty binding)

Binds any `MewProperty<T>` to an `ObservableValue<T>`. Works on any `MewObject`.

```csharp
// Direct type binding
element.Bind(Control.BackgroundProperty, colorSource)

// With conversion
element.Bind(Control.BackgroundProperty, temperatureSource,
    convert: temp => temp > 30 ? Color.Red : Color.Blue)

// With two-way conversion
textBox.Bind(TextBase.TextProperty, intSource,
    convert: i => i.ToString(),
    convertBack: s => int.TryParse(s, out var v) ? v : 0)
```

### 3.3 SetBinding (low-level)

The underlying API that fluent methods call. Use for custom controls or advanced scenarios.

```csharp
// ObservableValue binding
element.SetBinding(property, source, mode: BindingMode.TwoWay);

// With conversion
element.SetBinding(property, source, convert, convertBack, mode);

// MewObject-to-MewObject property binding
// Binds a property on this object to a property on another MewObject.
// Updates at the style (target) tier — local values still take precedence.
element.SetBinding(TextBlock.TextProperty, otherElement, Window.TitleProperty);
```

---

## 4. Binding Methods by Control

### Label

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindText(ObservableValue<string>)` | One-Way | Text binding |
| `BindText<T>(ObservableValue<T>, Func<T, string>)` | One-Way | Conversion binding |

### TextBox / MultiLineTextBox

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindText(ObservableValue<string>)` | Two-Way | Text input binding |

### Button

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindContent(ObservableValue<string>)` | One-Way | Button text binding |
| `BindContent<T>(ObservableValue<T>, Func<T, string>)` | One-Way | Conversion binding |

### CheckBox / RadioButton / ToggleSwitch

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindIsChecked(ObservableValue<bool>)` | Two-Way | Checked state binding |

### ListBox / ComboBox

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindSelectedIndex(ObservableValue<int>)` | Two-Way | Selection index binding |

### Slider

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindValue(ObservableValue<double>)` | Two-Way | Value binding |

### ProgressBar

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindValue(ObservableValue<double>)` | One-Way | Progress value binding |

### UIElement (Common)

| Method | Direction | Description |
|--------|-----------|-------------|
| `BindIsVisible(ObservableValue<bool>)` | One-Way | Visibility binding |
| `BindIsEnabled(ObservableValue<bool>)` | One-Way | Enabled state binding |

### Generic (Any MewProperty)

| Method | Direction | Description |
|--------|-----------|-------------|
| `Bind<TElement, T>(MewProperty<T>, ObservableValue<T>)` | Default | Direct property binding |
| `Bind<TElement, TProp, TSource>(MewProperty<TProp>, ObservableValue<TSource>, convert, convertBack?)` | Default | Conversion property binding |

---

## 5. ViewModel Pattern

### Basic ViewModel

```csharp
class LoginViewModel
{
    public ObservableValue<string> Username { get; } = new("");
    public ObservableValue<string> Password { get; } = new("");
    public ObservableValue<bool> RememberMe { get; } = new(false);
    public ObservableValue<string> ErrorMessage { get; } = new("");
    public ObservableValue<bool> IsLoading { get; } = new(false);

    public void Login()
    {
        if (string.IsNullOrEmpty(Username.Value))
        {
            ErrorMessage.Value = "Username is required";
            return;
        }
        IsLoading.Value = true;
        // ... login logic
    }
}
```

### UI Binding

```csharp
var vm = new LoginViewModel();

new StackPanel()
    .Vertical()
    .Spacing(8)
    .Children(
        new TextBox()
            .Placeholder("Username")
            .BindText(vm.Username),

        new TextBox()
            .Placeholder("Password")
            .BindText(vm.Password),

        new CheckBox()
            .Content("Remember me")
            .BindIsChecked(vm.RememberMe),

        new Label()
            .Foreground(Color.FromRgb(200, 60, 60))
            .BindText(vm.ErrorMessage),

        new Button()
            .Content("Login")
            .OnCanClick(() => !vm.IsLoading.Value)
            .OnClick(() => vm.Login())
    )
```

---

## 6. Computed Values

Combine multiple ObservableValues to create derived values:

```csharp
var firstName = new ObservableValue<string>("");
var lastName = new ObservableValue<string>("");

new Label()
    .Apply(label =>
    {
        void Update() => label.Text = $"{firstName.Value} {lastName.Value}".Trim();
        firstName.Changed += Update;
        lastName.Changed += Update;
        Update();
    })
```

### Reusable pattern

```csharp
public static Label BindFullName(this Label label,
    ObservableValue<string> firstName,
    ObservableValue<string> lastName)
{
    void Update() => label.Text = $"{firstName.Value} {lastName.Value}".Trim();
    firstName.Changed += Update;
    lastName.Changed += Update;
    Update();
    return label;
}

new Label().BindFullName(vm.FirstName, vm.LastName)
```

---

## 7. Memory Management

### Automatic Cleanup

Bindings are automatically cleaned up when controls are disposed (e.g., when the Window closes):

```csharp
var textBox = new TextBox().BindText(vm.Name);
// Binding auto-unsubscribed on disposal
```

### Manual Cleanup

```csharp
var counter = new ObservableValue<int>(0);
void OnChanged() => Console.WriteLine(counter.Value);

counter.Subscribe(OnChanged);
counter.Unsubscribe(OnChanged);  // manual unsubscribe
```

---

## 8. Best Practices

### Use ObservableValue in ViewModels

```csharp
// Good — bindable
class ViewModel
{
    public ObservableValue<string> Name { get; } = new("");
}

// Bad — not bindable
class ViewModel
{
    public string Name { get; set; }
}
```

### Use Coerce for validation

```csharp
var age = new ObservableValue<int>(0, v => Math.Clamp(v, 0, 150));
```

### Keep display logic in UI layer

```csharp
// Good — conversion at binding
new Label().BindText(vm.Price, p => $"${p:N0}")

// Bad — formatting in ViewModel
class ViewModel { public ObservableValue<string> FormattedPrice { get; } }
```

### Use Bind\<T> for non-standard properties

```csharp
// Fluent shorthand for common properties
new TextBox().BindText(vm.Name)

// Generic Bind for any MewProperty
new Border().Bind(Control.BackgroundProperty, vm.StatusColor)
```
