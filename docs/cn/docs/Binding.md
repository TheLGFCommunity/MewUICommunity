# 数据绑定指南

MewUI 的数据绑定系统采用基于委托、无需反射的方式，与 Native AOT 兼容。

---

## 1. 核心概念

### 无需反射的绑定

与 WPF/WinUI 不同，MewUI 不使用反射：

| WPF 方法 | MewUI 方法 |
|--------------|----------------|
| `{Binding PropertyName}` | `.BindText(vm.Name)` 或 `.Bind(property, source)` |
| `INotifyPropertyChanged` | `ObservableValue<T>` |
| PropertyPath 字符串 | 直接属性引用 |

优势：
- **Native AOT 兼容**：可安全进行裁剪/AOT 编译
- **编译时验证**：避免属性名称拼写错误
- **IntelliSense 支持**：提供自动补全
- **重构安全**：自动反映重命名操作

### 绑定模式

```csharp
public enum BindingMode
{
    OneWay,   // 仅从源 → 控件
    TwoWay,  // 源 ↔ 控件双向绑定
}
```

默认模式由属性决定：输入属性（如 `TextBox.TextProperty`）默认为 `TwoWay`，显示属性（如 `Label.TextProperty`）默认为 `OneWay`。

---

## 2. ObservableValue\<T>

一种响应式值容器，当值发生变化时会自动更新 UI。

### 基本用法

```csharp
var name = new ObservableValue<string>("Default");
var count = new ObservableValue<int>(0);
var isEnabled = new ObservableValue<bool>(true);

// 读取/写入
string current = name.Value;
name.Value = "New Value";

// 变更通知
name.Changed += () => Console.WriteLine("Name changed!");
```

### Coerce（值约束）

```csharp
var percent = new ObservableValue<double>(50, v => Math.Clamp(v, 0, 100));
percent.Value = 150;  // → 100
percent.Value = -10;  // → 0

var text = new ObservableValue<string>("", v => v?.Trim() ?? "");
```

---

## 3. 绑定 API

MewUI 提供三个级别的绑定：

### 3.1 流畅扩展方法（推荐）

针对常用属性，提供控件级别的高层便捷方法。

```csharp
var name = new ObservableValue<string>("");
var count = new ObservableValue<int>(0);
var isChecked = new ObservableValue<bool>(false);

// 文本绑定（TextBox 为双向，Label 为单向）
new TextBox().BindText(name)
new Label().BindText(name)

// 带转换的绑定
new Label().BindText(count, c => $"Count: {c}")

// CheckBox / ToggleSwitch
new CheckBox().BindIsChecked(isChecked)

// Slider / ProgressBar
new Slider().BindValue(volume)

// 可见性 / 启用状态
new Button().BindIsVisible(isVisible).BindIsEnabled(isEnabled)
```

### 3.2 泛型 Bind\<T>（MewProperty 绑定）

将任意 `MewProperty<T>` 绑定到 `ObservableValue<T>`。可用于任何 `MewObject`。

```csharp
// 直接类型绑定
element.Bind(Control.BackgroundProperty, colorSource)

// 带转换的绑定
element.Bind(Control.BackgroundProperty, temperatureSource,
    convert: temp => temp > 30 ? Color.Red : Color.Blue)

// 双向转换
textBox.Bind(TextBase.TextProperty, intSource,
    convert: i => i.ToString(),
    convertBack: s => int.TryParse(s, out var v) ? v : 0)
```

### 3.3 SetBinding（底层 API）

供流畅方法调用的底层 API。用于自定义控件或高级场景。

```csharp
// ObservableValue 绑定
element.SetBinding(property, source, mode: BindingMode.TwoWay);

// 带转换的绑定
element.SetBinding(property, source, convert, convertBack, mode);

// MewObject 到 MewObject 的属性绑定
// 将此对象上的属性绑定到另一个 MewObject 上的属性。
// 在样式（目标）层级进行更新 —— 本地值仍然优先。
element.SetBinding(TextBlock.TextProperty, otherElement, Window.TitleProperty);
```

---

## 4. 各控件的绑定方法

### Label

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindText(ObservableValue<string>)` | 单向 | 文本绑定 |
| `BindText<T>(ObservableValue<T>, Func<T, string>)` | 单向 | 带转换的绑定 |

### TextBox / MultiLineTextBox

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindText(ObservableValue<string>)` | 双向 | 文本输入绑定 |

### Button

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindContent(ObservableValue<string>)` | 单向 | 按钮文本绑定 |
| `BindContent<T>(ObservableValue<T>, Func<T, string>)` | 单向 | 带转换的绑定 |

### CheckBox / RadioButton / ToggleSwitch

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindIsChecked(ObservableValue<bool>)` | 双向 | 选中状态绑定 |

### ListBox / ComboBox

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindSelectedIndex(ObservableValue<int>)` | 双向 | 选中索引绑定 |

### Slider

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindValue(ObservableValue<double>)` | 双向 | 值绑定 |

### ProgressBar

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindValue(ObservableValue<double>)` | 单向 | 进度值绑定 |

### UIElement（通用）

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `BindIsVisible(ObservableValue<bool>)` | 单向 | 可见性绑定 |
| `BindIsEnabled(ObservableValue<bool>)` | 单向 | 启用状态绑定 |

### 泛型（任意 MewProperty）

| 方法 | 方向 | 说明 |
|--------|-----------|-------------|
| `Bind<TElement, T>(MewProperty<T>, ObservableValue<T>)` | 默认 | 直接属性绑定 |
| `Bind<TElement, TProp, TSource>(MewProperty<TProp>, ObservableValue<TSource>, convert, convertBack?)` | 默认 | 带转换的属性绑定 |

---

## 5. ViewModel 模式

### 基本 ViewModel

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
        // ... 登录逻辑
    }
}
```

### UI 绑定

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

## 6. 计算值

组合多个 ObservableValue 来生成派生值：

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

### 可复用模式

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

## 7. 内存管理

### 自动清理

当控件被释放时（例如窗口关闭），绑定会自动取消订阅：

```csharp
var textBox = new TextBox().BindText(vm.Name);
// 释放时会自动取消绑定订阅
```

### 手动清理

```csharp
var counter = new ObservableValue<int>(0);
void OnChanged() => Console.WriteLine(counter.Value);

counter.Subscribe(OnChanged);
counter.Unsubscribe(OnChanged);  // 手动取消订阅
```

---

## 8. 最佳实践

### 在 ViewModel 中使用 ObservableValue

```csharp
// 良好做法 —— 可绑定
class ViewModel
{
    public ObservableValue<string> Name { get; } = new("");
}

// 错误做法 —— 不可绑定
class ViewModel
{
    public string Name { get; set; }
}
```

### 使用 Coerce 进行验证

```csharp
var age = new ObservableValue<int>(0, v => Math.Clamp(v, 0, 150));
```

### 将显示逻辑保留在 UI 层

```csharp
// 良好做法 —— 在绑定时进行转换
new Label().BindText(vm.Price, p => $"${p:N0}")

// 错误做法 —— 在 ViewModel 中格式化
class ViewModel { public ObservableValue<string> FormattedPrice { get; } }
```

### 对非标准属性使用 Bind\<T>

```csharp
// 常用属性的流畅简写
new TextBox().BindText(vm.Name)

// 针对任意 MewProperty 的泛型 Bind
new Border().Bind(Control.BackgroundProperty, vm.StatusColor)
```
