![Aprillz.MewUI](https://raw.githubusercontent.com/aprillz/MewUI/main/assets/logo/logo_h-1280.png)


![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4?logo=dotnet&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-10%2B-0078D4?logo=windows&logoColor=white)
![Linux](https://img.shields.io/badge/Linux-X11-FCC624?logo=linux&logoColor=black)
![macOS](https://img.shields.io/badge/macOS-12%2B-901DBA?logo=Apple&logoColor=white)
![NativeAOT](https://img.shields.io/badge/NativeAOT-Ready-2E7D32)
![许可证：MIT](https://img.shields.io/badge/许可证-MIT-000000)
[![NuGet](https://img.shields.io/nuget/v/Aprillz.MewUI.svg?label=NuGet)](https://www.nuget.org/packages/Aprillz.MewUI/)
[![NuGet 下载量](https://img.shields.io/nuget/dt/Aprillz.MewUI.svg?label=下载量)](https://www.nuget.org/packages/Aprillz.MewUI/)

---

**😺 MewUI** 是一个跨平台、轻量级、代码优先的 .NET GUI 框架，旨在支持 NativeAOT。

### 🧪 实验性原型
> [!IMPORTANT]
> 此项目是一个**概念验证原型**，用于验证想法和探索设计方向。
> 随着它向 **v1.0** 发展，**API、内部架构和运行时行为可能会发生重大变化**。
> 现阶段**不保证**向后兼容性。

### 🤖 AI 辅助开发
> [!NOTE]
> 此项目使用 **AI 提示驱动的工作流**开发。
> 设计和实现通过**迭代提示**执行，**无需直接手动编辑代码**，
> 每个步骤都由开发人员审查和完善。

---

## 🚀 试用
**无需克隆。无需下载。无需项目设置。**
您可以在 **Windows**、**Linux** 或 **macOS** 上使用单个命令**立即运行 MewUI**。（需要 .NET 10 SDK）
> [!TIP]
> 这是**尝试 MewUI 的最快方式**，无需经历通常的存储库和项目设置步骤。
```bash
curl -sL https://raw.githubusercontent.com/aprillz/MewUI/refs/heads/main/samples/FBASample/fba_gallery.cs -o - | dotnet run -
```

> [!WARNING]
> 此命令直接从 GitHub 下载并执行代码。

### 视频
https://github.com/user-attachments/assets/fc2d6ad8-3317-4784-a6e5-a00c68e9ed3b

### 屏幕截图

| 浅色 | 深色 |
|---|---|
| ![浅色（屏幕截图）](https://raw.githubusercontent.com/aprillz/MewUI/main/assets/screenshots/light.png) | ![深色（屏幕截图）](https://raw.githubusercontent.com/aprillz/MewUI/main/assets/screenshots/dark.png) |

---
## ✨ 亮点

- 📦 **NativeAOT + 剪裁** 优先
- 🪶 设计上的**轻量级**（EXE 小、内存占用低、首帧快速）
- 🧩 流畅的 **C# 标记**（无 XAML）

## 🚀 快速入门

- NuGet：https://www.nuget.org/packages/Aprillz.MewUI/
  - `Aprillz.MewUI` 是一个**元包**，它捆绑了核心库、所有平台主机以及所有渲染后端。
  - 特定于平台的包也可用：`Aprillz.MewUI.Windows`、`.Linux`、`.MacOS`
  - 安装：`dotnet add package Aprillz.MewUI`
  - 请参阅：[安装与包](docs/Installation.md)

- 单文件应用（适合 VS Code）
  - 请参阅：[samples/FBASample/fba_calculator.cs](samples/FBASample/fba_calculator.cs)
  - 最小标头（不含 AOT/Trim 选项）：

    ```csharp
    #:sdk Microsoft.NET.Sdk
    #:property OutputType=Exe
    #:property TargetFramework=net10.0

    #:package Aprillz.MewUI@0.10.3

    // ...
    ```

- 运行：`dotnet run your_app.cs`
---
## 🧪 C# 标记一览

- 示例源代码：https://github.com/aprillz/MewUI/blob/main/samples/MewUI.Sample/Program.cs

   ```csharp
    var window = new Window()
        .Title("Hello MewUI")
        .Size(520, 360)
        .Padding(12)
        .Content(
            new StackPanel()
                .Spacing(8)
                .Children(
                    new Label()
                        .Text("Hello, Aprillz.MewUI")
                        .FontSize(18)
                        .Bold(),
                    new Button()
                        .Content("Quit")
                        .OnClick(() => Application.Quit())
                )
        );

    Application.Run(window);
    ```

---
## 🎯 概念

### MewUI 是一个代码优先的 GUI 框架，具有四个优先事项：
- **NativeAOT + 剪裁友好性**
- **小尺寸、快速启动、低内存使用**
- 用于构建 UI 树的**流畅 C# 标记**（无 XAML）
- **AOT 友好的绑定**

### 非目标（设计上）：
- 类似 WPF 的**动画**、**视觉效果**或繁重的合成管道
- 庞大而“包罗万象”的控件目录（保持较小的表面积和可预测性）
- 基于复杂路径的数据绑定
- 完全的 XAML/WPF 兼容性或设计器优先的工作流

---
## ✂️ NativeAOT / 剪裁

- 该库旨在默认情况下对剪裁安全（显式代码路径，无基于反射的绑定）。
- Windows 互操作使用源生成的 P/Invoke (`LibraryImport`) 以实现 NativeAOT 兼容性。
- 在 Linux 上，使用 NativeAOT 构建除了常规的 .NET SDK 之外，还需要 AOT 工作负载（例如，安装 `dotnet-sdk-aot-10.0`）。
- 如果您引入了新的互操作或动态功能，请使用上述剪裁发布配置文件进行验证。

本地检查输出大小：
- 发布：`dotnet publish .\samples\MewUI.Gallery\MewUI.Gallery.csproj -c Release -p:PublishProfile=win-x64-trimmed`
- 检查：`.artifacts\publish\MewUI.Gallery\win-x64-trimmed\`

参考（`Aprillz.MewUI.Gallery.exe` @v0.10.0）
- win-x64：~3,545 KB
- osx-arm64：~2,664 KB
- linux-arm64：~3,939 KB
---
## 🔗 状态与绑定（AOT 友好）

绑定是显式的且基于委托（无反射）：

```csharp
using Aprillz.MewUI.Binding;
using Aprillz.MewUI.Controls;

var percent = new ObservableValue<double>(
    initialValue: 0.25,
    coerce: v => Math.Clamp(v, 0, 1));

var slider = new Slider()
            .BindValue(percent);
var label  = new Label()
            .BindText(percent, v => $"百分比 ({v:P0})");
```

---
## 🧱 控件 / 面板

控件（已实现）：
- `Button`
- `Label`、`Image`
- `TextBox`、`MultiLineTextBox`
- `CheckBox`、`RadioButton`、`ToggleSwitch`
- `ComboBox`、`ListBox`、`TreeView`、`GridView`
- `Slider`、`ProgressBar`、`NumericUpDown`
- `TabControl`、`GroupBox`
- `MenuBar`、`ContextMenu`、`ToolTip`（窗口内弹出）
- `ScrollViewer`
- `Window`、`DispatcherTimer`

面板：
- `Grid`（具有 `Auto`、`*`、像素的行/列）
- `StackPanel`（水平/垂直）
- `DockPanel`（停靠边缘 + 最后一个子项填充）
- `UniformGrid`（均等单元格）
- `WrapPanel`（换行 + 项目大小）
- `SplitPanel`（拖动拆分器）

> 除 `SplitPanel` 外，所有面板均支持 `Spacing`。
---
## 🎨 主题

MewUI 使用 `Theme` 对象（颜色 + 指标）和 `ThemeManager` 来控制默认值和运行时更改。

- 在 `Application.Run(...)` 之前通过 `ThemeManager.Default*` 配置默认值
- 在运行时通过 `Application.Current.SetTheme(...)` / `Application.Current.SetAccent(...)` 更改

请参阅：`docs/Theme.md`

---
## 🖌️ 渲染后端

渲染通过以下方式进行抽象：
- `IGraphicsFactory` / `IGraphicsContext`

后端：

| 后端 | 平台 | 包 |
|---------|----------|---------|
| **Direct2D** | Windows | `Aprillz.MewUI.Backend.Direct2D` |
| **GDI** | Windows | `Aprillz.MewUI.Backend.Gdi` |
| **MewVG** | Windows | `Aprillz.MewUI.Backend.MewVG.Win32` |
| **MewVG** | Linux/X11 | `Aprillz.MewUI.Backend.MewVG.X11` |
| **MewVG** | macOS | `Aprillz.MewUI.Backend.MewVG.MacOS` |

> **MewVG** 是 [NanoVG](https://github.com/memononen/nanovg) 的托管移植，在 Windows/Linux 上使用 OpenGL，在 macOS 上使用 Metal。

后端由引用的后端包注册（Trim/AOT 友好）。在应用代码中，您通常可以：
- 在 `Application.Run(...)` 之前调用 `*Backend.Register()`，或
- 使用构建器链：`Application.Create().Use...().Run(...)`

当使用元包（例如 `Aprillz.MewUI.Windows`）时，您可以在发布时使用 `-p:MewUIBackend=Direct2D` 选择单个后端。有关详细信息，请参阅[安装与包](docs/Installation.md)。
---
## 🪟 平台抽象

窗口和消息循环在平台层之后进行抽象。

目前已实现：
- Windows (`Aprillz.MewUI.Platform.Win32`)
- Linux/X11 (`Aprillz.MewUI.Platform.X11`)
- macOS (`Aprillz.MewUI.Platform.MacOS`)

### Linux 对话框依赖项
在 Linux 上，`MessageBox` 和文件对话框目前通过外部工具实现：
- `zenity`（GNOME/GTK）
- `kdialog`（KDE）

如果 `PATH` 中两者都不可用，MewUI 会抛出：
`PlatformNotSupportedException：未找到支持的 Linux 对话框工具（zenity/kdialog）。`

---
## 📄 文档

- [安装与包](docs/Installation.md)
- [C# 标记](docs/CSharpMarkup.md)
- [绑定](docs/Binding.md)
- [项目与模板](docs/ItemsAndTemplates.md)
- [主题](docs/Theme.md)
- [应用程序生命周期](docs/ApplicationLifecycle.md)
- [布局](docs/Layout.md)
- [渲染循环](docs/RenderLoop.md)
- [热重载](docs/HotReload.md)
- [自定义控件](docs/CustomControls.md)

---
## 🧭 路线图（待办事项）

**平台**
- [ ] Linux/Wayland
- [x] macOS

**工具**
- [x] 热重载
- [ ] 设计时预览