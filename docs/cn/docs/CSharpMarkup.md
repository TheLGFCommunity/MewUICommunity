# C# 标记指南

MewUI 的 C# 标记提供了一套流畅 API，让你能够使用纯 C# 代码以声明方式生成 UI，无需 XAML。
该方式兼容 Native AOT 编译，且不使用反射。

## 概念

### 为什么选择 C# 标记？

- **Native AOT 兼容**：所有内容在编译时确定，无需反射
- **类型安全**：编译器能捕获错误
- **IntelliSense**：IDE 自动补全支持
- **代码复用**：将 UI 组件提取为常规 C# 方法

### 基本模式

```csharp
new Button()
    .Content("Click Me")
    .Width(100)
    .OnClick(() => Console.WriteLine("Clicked!"))
```

所有扩展方法都返回 `this`，支持方法链式调用。

## 命名规范

### 属性设置
| 模式 | 说明 | 示例 |
|---------|-------------|---------|
| `PropertyName(value)` | 直接设置属性 | `.Width(100)`, `.Text("Hello")` |
| `PropertyName()` | 将 bool 属性设为 true | `.Bold()`, `.IsChecked()` |

### 事件处理
| 模式 | 说明 | 示例 |
|---------|-------------|---------|
| `OnEventName(handler)` | 注册事件处理程序 | `.OnClick(...)`, `.OnTextChanged(...)` |
| `OnCanEventName(func)` | 条件执行（命令模式） | `.OnCanClick(() => isValid)` |

### 数据绑定
| 模式 | 说明 | 示例 |
|---------|-------------|---------|
| `BindPropertyName(source)` | ObservableValue 绑定 | `.BindText(vm.Name)` |
| `BindPropertyName(source, converter)` | 带转换的绑定 | `.BindText(vm.Count, c => $"{c} items")` |

### 快捷方法
常用属性提供了简洁的快捷方法：
- `.Bold()` → `.FontWeight(FontWeight.Bold)`
- `.Horizontal()` → `.Orientation(Orientation.Horizontal)`
- `.Center()` → `.HorizontalAlignment(Center).VerticalAlignment(Center)`

---

## 常用扩展方法

### FluentExtensions（所有引用类型）

| 方法 | 说明 |
|--------|-------------|
| `Ref(out T field)` | 将引用存储到变量中 |

```csharp
new TextBox()
    .Ref(out var nameBox)  // 将引用存储到 nameBox 变量中
    .Text("Hello")
```

---

## 元素扩展方法

所有 UI 元素的基类。

### DockPanel 附加属性

| 方法 | 说明 |
|--------|-------------|
| `DockTo(Dock dock)` | 设置 Dock 位置 |
| `DockLeft()` | 靠左停靠 |
| `DockTop()` | 靠上停靠 |
| `DockRight()` | 靠右停靠 |
| `DockBottom()` | 靠下停靠 |

### Grid 附加属性

| 方法 | 说明 |
|--------|-------------|
| `Row(int row)` | Grid 行位置 |
| `Column(int column)` | Grid 列位置 |
| `RowSpan(int rowSpan)` | 行跨度 |
| `ColumnSpan(int columnSpan)` | 列跨度 |
| `GridPosition(row, column)` | 同时设置行和列 |
| `GridPosition(row, column, rowSpan, columnSpan)` | 设置完整位置信息 |

### Canvas 附加属性

| 方法 | 说明 |
|--------|-------------|
| `CanvasLeft(double left)` | 左侧偏移 |
| `CanvasTop(double top)` | 顶部偏移 |
| `CanvasRight(double right)` | 右侧偏移 |
| `CanvasBottom(double bottom)` | 底部偏移 |
| `CanvasPosition(left, top)` | 设置位置 |

---

## FrameworkElement 扩展方法

所有支持布局的元素的基类。

### 尺寸

| 方法 | 说明 |
|--------|-------------|
| `Width(double)` | 宽度 |
| `Height(double)` | 高度 |
| `Size(width, height)` | 同时设置宽度和高度 |
| `Size(double)` | 正方形尺寸 |
| `MinWidth(double)` | 最小宽度 |
| `MinHeight(double)` | 最小高度 |
| `MaxWidth(double)` | 最大宽度 |
| `MaxHeight(double)` | 最大高度 |

### 边距与内边距

| 方法 | 说明 |
|--------|-------------|
| `Margin(uniform)` | 统一外边距 |
| `Margin(horizontal, vertical)` | 水平/垂直外边距 |
| `Margin(left, top, right, bottom)` | 分别设置外边距 |
| `Padding(uniform)` | 统一内边距 |
| `Padding(horizontal, vertical)` | 水平/垂直内边距 |
| `Padding(left, top, right, bottom)` | 分别设置内边距 |

### 对齐

| 方法 | 说明 |
|--------|-------------|
| `HorizontalAlignment(alignment)` | 水平对齐 |
| `VerticalAlignment(alignment)` | 垂直对齐 |
| `Center()` | 居中对齐（水平 + 垂直） |
| `CenterHorizontal()` | 水平居中 |
| `CenterVertical()` | 垂直居中 |
| `Left()` | 左对齐 |
| `Right()` | 右对齐 |
| `Top()` | 顶部对齐 |
| `Bottom()` | 底部对齐 |
| `StretchHorizontal()` | 水平拉伸 |
| `StretchVertical()` | 垂直拉伸 |

---

## UIElement 扩展方法

所有处理输入事件的元素的基类。

### 绑定

| 方法 | 说明 |
|--------|-------------|
| `BindIsVisible(ObservableValue<bool>)` | 可见性绑定 |
| `BindIsEnabled(ObservableValue<bool>)` | 启用状态绑定 |

### 焦点事件

| 方法 | 说明 |
|--------|-------------|
| `OnGotFocus(Action)` | 获得焦点时触发 |
| `OnLostFocus(Action)` | 失去焦点时触发 |

### 鼠标事件

| 方法 | 说明 |
|--------|-------------|
| `OnMouseEnter(Action)` | 鼠标进入 |
| `OnMouseLeave(Action)` | 鼠标离开 |
| `OnMouseDown(Action<MouseEventArgs>)` | 鼠标按下 |
| `OnMouseUp(Action<MouseEventArgs>)` | 鼠标释放 |
| `OnMouseMove(Action<MouseEventArgs>)` | 鼠标移动 |
| `OnMouseWheel(Action<MouseWheelEventArgs>)` | 鼠标滚轮 |

### 键盘事件

| 方法 | 说明 |
|--------|-------------|
| `OnKeyDown(Action<KeyEventArgs>)` | 按键按下 |
| `OnKeyUp(Action<KeyEventArgs>)` | 按键释放 |
| `OnTextInput(Action<TextInputEventArgs>)` | 文本输入 |

---

## Control 扩展方法

所有具有视觉样式的控件的基类。

### 颜色

| 方法 | 说明 |
|--------|-------------|
| `Background(Color)` | 背景色 |
| `Foreground(Color)` | 前景色（文本颜色） |
| `BorderBrush(Color)` | 边框颜色 |
| `BorderThickness(double)` | 边框粗细 |

### 字体

| 方法 | 说明 |
|--------|-------------|
| `FontFamily(string)` | 字体名称 |
| `FontSize(double)` | 字号 |
| `FontWeight(FontWeight)` | 字体粗细 |
| `Bold()` | 加粗（快捷方式） |

---

## 各控件扩展方法

### Window

```csharp
new Window()
    .Title("我的应用")
    .Resizable(800, 600)
    .Content(...)
    .OnLoaded(() => ...)
    .OnClosed(() => ...)
```

| 方法 | 说明 |
|--------|-------------|
| `Title(string)` | 窗口标题 |
| `Width(double)` | 窗口宽度 |
| `Height(double)` | 窗口高度 |
| `Size(width, height)` | 窗口尺寸 |
| `Resizable(width, height)` | 可调整大小 |
| `Fixed(width, height)` | 固定大小 |
| `FitContentWidth(maxWidth, fixedHeight)` | 适应内容宽度 |
| `FitContentHeight(fixedWidth, maxHeight)` | 适应内容高度 |
| `FitContentSize(maxWidth, maxHeight)` | 适应内容大小 |
| `Content(Element)` | 窗口内容 |
| `OnLoaded(Action)` | 加载完成时触发 |
| `OnClosed(Action)` | 窗口关闭时触发 |
| `OnActivated(Action)` | 窗口激活时触发 |
| `OnDeactivated(Action)` | 窗口失去激活时触发 |
| `OnSizeChanged(Action<Size>)` | 窗口大小改变时触发 |
| `OnDpiChanged(Action<uint, uint>)` | DPI 改变时触发 |
| `OnThemeChanged(Action<Theme, Theme>)` | 主题改变时触发 |
| `OnFirstFrameRendered(Action)` | 首帧渲染完成时触发 |
| `OnPreviewKeyDown(Action<KeyEventArgs>)` | 按键按下（预览） |
| `OnPreviewKeyUp(Action<KeyEventArgs>)` | 按键释放（预览） |
| `OnPreviewTextInput(Action<TextInputEventArgs>)` | 文本输入（预览） |

### Label

```csharp
new Label()
    .Text("Hello World")
    .Bold()
    .FontSize(16)
```

| 方法 | 说明 |
|--------|-------------|
| `Text(string)` | 文本内容 |
| `TextAlignment(TextAlignment)` | 水平文本对齐 |
| `VerticalTextAlignment(TextAlignment)` | 垂直文本对齐 |
| `TextWrapping(TextWrapping)` | 文本换行 |
| `BindText(ObservableValue<string>)` | 文本绑定 |
| `BindText(source, converter)` | 带转换的绑定 |

### Button

```csharp
new Button()
    .Content("Click Me")
    .OnCanClick(() => isFormValid)
    .OnClick(() => Submit())
```

| 方法 | 说明 |
|--------|-------------|
| `Content(string)` | 按钮文本 |
| `OnClick(Action)` | 单击处理程序 |
| `OnCanClick(Func<bool>)` | 单击条件（命令模式） |
| `BindContent(ObservableValue<string>)` | 内容绑定 |

### TextBox

```csharp
new TextBox()
    .Placeholder("Enter name...")
    .BindText(vm.Name)
```

| 方法 | 说明 |
|--------|-------------|
| `Text(string)` | 文本内容 |
| `Placeholder(string)` | 占位符 |
| `IsReadOnly(bool)` | 只读 |
| `AcceptTab(bool)` | 接受 Tab 键 |
| `OnTextChanged(Action<string>)` | 文本改变处理程序 |
| `BindText(ObservableValue<string>)` | 文本绑定（双向） |

### MultiLineTextBox

```csharp
new MultiLineTextBox()
    .Placeholder("Enter notes...")
    .Wrap(true)
    .Height(100)
```

| 方法 | 说明 |
|--------|-------------|
| `Text(string)` | 文本内容 |
| `Placeholder(string)` | 占位符 |
| `IsReadOnly(bool)` | 只读 |
| `AcceptTab(bool)` | 接受 Tab 键 |
| `Wrap(bool)` | 自动换行 |
| `OnWrapChanged(Action<bool>)` | 换行改变处理程序 |
| `OnTextChanged(Action<string>)` | 文本改变处理程序 |
| `BindText(ObservableValue<string>)` | 文本绑定 |

### CheckBox

```csharp
new CheckBox()
    .Text("Enable feature")
    .BindIsChecked(vm.IsEnabled)
```

| 方法 | 说明 |
|--------|-------------|
| `Text(string)` | 标签文本 |
| `IsChecked(bool)` | 选中状态 |
| `OnCheckedChanged(Action<bool>)` | 选中状态改变处理程序 |
| `BindIsChecked(ObservableValue<bool>)` | 选中状态绑定 |
| `BindWrap(MultiLineTextBox)` | 链接到 Wrap 属性 |

### RadioButton

```csharp
new RadioButton()
    .Text("Option A")
    .GroupName("options")
    .IsChecked(true)
```

| 方法 | 说明 |
|--------|-------------|
| `Text(string)` | 标签文本 |
| `GroupName(string?)` | 组名（同一组内只能选中一项） |
| `IsChecked(bool)` | 选中状态 |
| `OnCheckedChanged(Action<bool>)` | 选中状态改变处理程序 |
| `BindIsChecked(ObservableValue<bool>)` | 选中状态绑定 |

### ToggleSwitch

```csharp
new ToggleSwitch()
    .Text("Dark Mode")
    .BindIsChecked(vm.IsDarkMode)
```

| 方法 | 说明 |
|--------|-------------|
| `Text(string)` | 标签文本 |
| `IsChecked(bool)` | 开关状态 |
| `OnCheckedChanged(Action<bool>)` | 开关状态改变处理程序 |
| `BindIsChecked(ObservableValue<bool>)` | 开关状态绑定 |

### ListBox

```csharp
new ListBox()
    .Items("Apple", "Banana", "Cherry")
    .SelectedIndex(0)
    .Height(120)
```

| 方法 | 说明 |
|--------|-------------|
| `Items(params string[])` | 项列表 |
| `ItemHeight(double)` | 项高度 |
| `ItemPadding(Thickness)` | 项内边距 |
| `SelectedIndex(int)` | 选中索引 |
| `OnSelectionChanged(Action<int>)` | 选中项改变处理程序 |
| `BindSelectedIndex(ObservableValue<int>)` | 选中索引绑定 |

### ComboBox

```csharp
new ComboBox()
    .Items("Small", "Medium", "Large")
    .Placeholder("Select size...")
    .SelectedIndex(1)
```

| 方法 | 说明 |
|--------|-------------|
| `Items(params string[])` | 项列表 |
| `SelectedIndex(int)` | 选中索引 |
| `Placeholder(string)` | 占位符 |
| `OnSelectionChanged(Action<int>)` | 选中项改变处理程序 |
| `BindSelectedIndex(ObservableValue<int>)` | 选中索引绑定 |

### Slider

```csharp
new Slider()
    .Minimum(0)
    .Maximum(100)
    .BindValue(vm.Volume)
```

| 方法 | 说明 |
|--------|-------------|
| `Minimum(double)` | 最小值 |
| `Maximum(double)` | 最大值 |
| `Value(double)` | 当前值 |
| `SmallChange(double)` | 小步长变化量 |
| `OnValueChanged(Action<double>)` | 值改变处理程序 |
| `BindValue(ObservableValue<double>)` | 值绑定 |

### ProgressBar

```csharp
new ProgressBar()
    .Minimum(0)
    .Maximum(100)
    .BindValue(vm.Progress)
```

| 方法 | 说明 |
|--------|-------------|
| `Minimum(double)` | 最小值 |
| `Maximum(double)` | 最大值 |
| `Value(double)` | 当前值 |
| `BindValue(ObservableValue<double>)` | 值绑定 |

### Image

```csharp
new Image()
    .SourceFile("logo.png")
    .Size(64, 64)
    .StretchMode(ImageStretch.Uniform)
```

| 方法 | 说明 |
|--------|-------------|
| `Source(ImageSource?)` | 图像源 |
| `SourceFile(string path)` | 从文件加载 |
| `SourceResource(Assembly, string)` | 从资源加载 |
| `SourceResource<TAnchor>(string)` | 从资源加载（泛型方式） |
| `StretchMode(ImageStretch)` | 拉伸模式 |

### TabControl

```csharp
new TabControl()
    .TabItems(
        new TabItem().Header("Home").Content(...),
        new TabItem().Header("Settings").Content(...)
    )
```

| 方法 | 说明 |
|--------|-------------|
| `TabItems(params TabItem[])` | 选项卡项列表 |
| `Tab(header, content)` | 添加选项卡（字符串标题） |
| `Tab(Element header, content)` | 添加选项卡（元素标题） |
| `SelectedIndex(int)` | 选中选项卡索引 |
| `OnSelectionChanged(Action<int>)` | 选项卡改变处理程序 |
| `VerticalScroll(ScrollMode)` | 垂直滚动 |
| `HorizontalScroll(ScrollMode)` | 水平滚动 |
| `AutoVerticalScroll()` | 自动垂直滚动 |
| `AutoHorizontalScroll()` | 自动水平滚动 |

### TabItem

```csharp
new TabItem()
    .Header("Settings")
    .Content(new StackPanel().Children(...))
    .IsEnabled(true)
```

| 方法 | 说明 |
|--------|-------------|
| `Header(string)` | 标题文本 |
| `Header(Element)` | 标题元素 |
| `Content(Element)` | 选项卡内容 |
| `IsEnabled(bool)` | 启用状态 |

### GroupBox（带标题的内容控件）

```csharp
new GroupBox()
    .Header("Options")
    .Content(new StackPanel().Children(...))
```

| 方法 | 说明 |
|--------|-------------|
| `Header(string)` | 标题文本（粗体样式） |
| `Header(Element)` | 标题元素 |
| `HeaderSpacing(double)` | 标题与内容的间距 |
| `Content(Element)` | 组内容 |

### ScrollViewer

```csharp
new ScrollViewer()
    .AutoVerticalScroll()
    .NoHorizontalScroll()
    .Content(...)
```

| 方法 | 说明 |
|--------|-------------|
| `VerticalScroll(ScrollMode)` | 垂直滚动模式 |
| `HorizontalScroll(ScrollMode)` | 水平滚动模式 |
| `Scroll(vertical, horizontal)` | 同时设置滚动模式 |
| `NoVerticalScroll()` | 禁用垂直滚动 |
| `AutoVerticalScroll()` | 自动垂直滚动 |
| `ShowVerticalScroll()` | 始终显示垂直滚动条 |
| `NoHorizontalScroll()` | 禁用水平滚动 |
| `AutoHorizontalScroll()` | 自动水平滚动 |
| `ShowHorizontalScroll()` | 始终显示水平滚动条 |
| `Content(Element)` | 要滚动的内容 |

---

## 面板扩展方法

### Panel（通用）

| 方法 | 说明 |
|--------|-------------|
| `Children(params Element[])` | 添加子元素 |

### StackPanel

```csharp
new StackPanel()
    .Vertical()
    .Spacing(8)
    .Children(
        new Label().Text("First"),
        new Label().Text("Second")
    )
```

| 方法 | 说明 |
|--------|-------------|
| `Orientation(Orientation)` | 方向 |
| `Horizontal()` | 水平方向（快捷方式） |
| `Vertical()` | 垂直方向（快捷方式） |
| `Spacing(double)` | 元素之间的间距 |

### Grid

```csharp
new Grid()
    .Rows("Auto,*,Auto")
    .Columns("100,*")
    .Spacing(8)
    .AutoIndexing()
    .Children(
        new Label().Text("Name:"),
        new TextBox()
    )
```

| 方法 | 说明 |
|--------|-------------|
| `Rows(params GridLength[])` | 行定义 |
| `Rows(string)` | 行定义（字符串格式："Auto,*,2*,100"） |
| `Columns(string)` | 列定义（字符串格式） |
| `Spacing(double)` | 单元格间距 |
| `AutoIndexing(bool)` | 自动索引（行/列自动递增） |

**GridLength 字符串语法：**
- `Auto` - 适应内容
- `*` - 1 星比例
- `2*` - 2 星比例
- `100` - 100 像素

### UniformGrid

```csharp
new UniformGrid()
    .Columns(3)
    .Spacing(8)
    .Children(
        new Button().Content("1"),
        new Button().Content("2"),
        new Button().Content("3")
    )
```

| 方法 | 说明 |
|--------|-------------|
| `Rows(int)` | 行数 |
| `Columns(int)` | 列数 |
| `Spacing(double)` | 单元格间距 |

### WrapPanel

```csharp
new WrapPanel()
    .Orientation(Orientation.Horizontal)
    .Spacing(8)
    .ItemWidth(100)
    .ItemHeight(100)
    .Children(...)
```

| 方法 | 说明 |
|--------|-------------|
| `Orientation(Orientation)` | 方向 |
| `Spacing(double)` | 元素之间的间距 |
| `ItemWidth(double)` | 项宽度 |
| `ItemHeight(double)` | 项高度 |

### DockPanel

```csharp
new DockPanel()
    .LastChildFill()
    .Spacing(8)
    .Children(
        new Label().Text("Header").DockTop(),
        new Label().Text("Footer").DockBottom(),
        new Label().Text("Content")  // 填充剩余空间
    )
```

| 方法 | 说明 |
|--------|-------------|
| `LastChildFill(bool)` | 最后一个子元素填充剩余空间 |
| `Spacing(double)` | 元素之间的间距 |

---

## 命令模式（CanExecute 模式）

你可以使用 Button 的 `OnCanClick` 来实现类似 WPF ICommand 的模式。

```csharp
var text = new ObservableValue<string>("");

new TextBox()
    .BindText(text)
    .OnTextChanged(_ => window.RequerySuggested()),

new Button()
    .Content("Submit")
    .OnCanClick(() => !string.IsNullOrWhiteSpace(text.Value))
    .OnClick(() => Submit(text.Value))
```

### 自动重新求值时机

`CanClick` 会在以下时机自动重新求值：
- **焦点变化** - 当焦点移动时
- **MouseUp** - 当鼠标按键释放时
- **KeyUp** - 当按键释放时

### 手动重新求值

当状态发生变化后需要手动重新求值时：

```csharp
// 在事件处理程序中状态发生变化后
counter.Value++;
window.RequerySuggested();  // 触发 CanClick 重新求值
```

---

## Apply 模式

对于复杂的初始化或当前不支持的属性设置，可使用 `Apply` 模式：

```csharp
public static T Apply<T>(this T obj, Action<T> action)
{
    action(obj);
    return obj;
}

// 用法示例
new TextBox()
    .OnTextChanged(text => Console.WriteLine(text))
    .Apply(tb => tb.MaxLength = 100)
```
