# 应用程序和窗口生命周期

本文档总结了 MewUI 中**以启动为重点**的应用程序/窗口生命周期及开发人员体验。
它阐明了“Run 之前”和“Run 之后”之间的界限。

---

## 1. 运行前配置

本节介绍如何在调用 `Application.Run(...)` 之前配置平台主机和图形后端。

MewUI 的目标是避免为核心级别的平台/图形后端选择提供枚举/开关式选择。
相反，每个包都提供注册和选择功能，以保持剪裁/AOT 友好性。

### 1.1 推荐方法

在调用 `Application.Run(...)` 之前注册平台/后端包。
```csharp
using Aprillz.MewUI;
using Aprillz.MewUI.Backends;
using Aprillz.MewUI.PlatformHosts;

// 在运行时检测操作系统，并仅注册对该操作系统有效的平台/后端。
// （在此示例中：Windows=Win32，Linux=X11，macOS 计划中。）
if (OperatingSystem.IsWindows())
{
    Win32Platform.Register();
    Direct2DBackend.Register(); // 或者 GdiBackend.Register() / OpenGLWin32Backend.Register()
}
else if (OperatingSystem.IsLinux())
{
    X11Platform.Register();
    OpenGLX11Backend.Register();
}
else if (OperatingSystem.IsMacOS())
{
    // 待办：一旦 macOS 平台主机/后端实现，在此处注册
    throw new PlatformNotSupportedException("macOS 平台主机尚未实现。");
}
else
{
    throw new PlatformNotSupportedException("不支持的操作系统。");
}

Application.Run(mainWindow);
```

### 1.2 单目标应用：Application.Create() 链式调用
如果您的应用固定于**一个平台 + 一个图形后端**（例如，仅限 Windows），那么 `Application.Create()` 链式调用是最简单的方法。

假设：
- 您的项目**引用**了平台/后端包（因此像 `.UseWin32()` 这样的扩展方法可用）。
- 构建和包引用已经固定；您不是在运行时选择操作系统。

```csharp
using Aprillz.MewUI;
using Aprillz.MewUI.Backends;
using Aprillz.MewUI.PlatformHosts;

Application.Create()
    .UseWin32()
    .UseDirect2D()
    .Run(mainWindow);
```

### 1.3 多目标应用：固定链式调用
除了在运行时进行操作系统分支，您还可以**通过 csproj 条件（通常是 RID/CI 发布）定义符号**，然后**使用 `#if` 固定链式调用**。
这对于剪裁/分发也很方便，因为您可以构建包引用，使每个构建只包含其所需的内容。

#### 1.3.1 在 csproj 中定义符号（示例）
```xml
<PropertyGroup>
  <TargetFrameworks>net10.0-windows;net10.0</TargetFrameworks>
  <!-- 假设 CI/发布通过以下方式注入 RID：dotnet publish -r ... -->
  <RuntimeIdentifiers>win-x64;linux-x64;osx-x64;osx-arm64</RuntimeIdentifiers>
</PropertyGroup>

<!-- 开发运行（RID 通常为空）：使用运行时操作系统分支路径 -->
<PropertyGroup Condition="'$(RuntimeIdentifier)' == ''">
  <DefineConstants>$(DefineConstants);DEV</DefineConstants>
</PropertyGroup>

<!-- 发布/CI（RID 已设置）：根据 RID 固定操作系统符号 -->
<PropertyGroup Condition="'$(RuntimeIdentifier)' != '' and $(RuntimeIdentifier.StartsWith('win-'))">
  <DefineConstants>$(DefineConstants);WINDOWS</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="'$(RuntimeIdentifier)' != '' and $(RuntimeIdentifier.StartsWith('linux-'))">
  <DefineConstants>$(DefineConstants);LINUX</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="'$(RuntimeIdentifier)' != '' and $(RuntimeIdentifier.StartsWith('osx-'))">
  <DefineConstants>$(DefineConstants);MACOS</DefineConstants>
</PropertyGroup>
```

#### 1.3.2 在 Program.cs 中固定链式调用（示例）
```csharp
using Aprillz.MewUI;
using Aprillz.MewUI.Backends;
using Aprillz.MewUI.PlatformHosts;

Application.Create()

#if WINDOWS || DEV
    .UseWin32()
    .UseDirect2D()
#elif LINUX
    .UseX11()
    .UseOpenGL()
#elif MACOS
    .ThrowPlatformNotSupported("macOS 平台主机尚未实现。")
#else
    .ThrowPlatformNotSupported()
#endif
    .Run(mainWindow);
```

### 1.4 在保持生成器链式调用的同时进行运行时分支
如果必须在运行时进行分支，同时仍希望保持类似链式调用的风格，可以使用一个生成器变量，并在分支后继续链式调用。

### 注意事项
- **只能在 Run 之前配置**：在 Run 之后更改核心应用设置应引发异常或被忽略（该策略在代码中必须保持一致）。
- **基于插件的注册**：平台/后端包提供注册/选择功能。

---

## 2. 应用程序启动流程

### 2.1 Application.Run
当调用 `Application.Run(Window)` 时，流程如下：

1) 设置 `Application.Current`
2) 创建 PlatformHost 并初始化 Dispatcher
3) 注册并显示窗口
4) 进入消息循环

#### 示例：最小设置
```csharp
var window = new Window()
    .Title("Hello")
    .Content(new TextBlock().Text("Hello, MewUI"));

Application.Run(window);
```

### 2.2 主题配置
有关 ThemeVariant/Accent/ThemeSeed/ThemeMetrics 的配置，请参阅：

- [主题文档](Theme.md)

---

## 3. 窗口启动流程

### 3.1 构造窗口
`new Window()` 仅创建对象；**尚未存在本机句柄**。

### 3.2 Show
在 `Window.Show()` 时：
1) 注册到 Application
2) 创建后端资源（WindowHandle）
3) 引发 Loaded 事件
4) 执行第一次布局和渲染

### 3.3 ShowDialogAsync（模式对话框）
`ShowDialogAsync` 将窗口显示为模式对话框，并在其关闭时完成。
当提供 `owner` 参数时，在对话框打开期间，所有者窗口将被禁用（取决于平台）。

```csharp
var dialog = new Window()
    .Title("Dialog")
    .Content(new TextBlock().Text("来自对话框的问候"));

await dialog.ShowDialogAsync(owner: main);
```

#### 示例：多窗口
```csharp
var main = new Window()
    .Title("Main")
    .Content(new TextBlock().Text("主窗口"));

var tools = new Window()
    .Title("Tools")
    .Content(new TextBlock().Text("工具窗口"));

main.OnLoaded(() => tools.Show());
Application.Run(main);
```

---

## 4. RenderLoopSettings

渲染循环行为通过 `Application.Current.RenderLoopSettings` 控制：

- `Mode`：`OnRequest` / `Continuous`
- `TargetFps`：0 表示无限制
- `VSyncEnabled`：控制后端呈现/交换行为

#### 示例：RenderLoop 设置
```csharp
Application.Current.RenderLoopSettings.SetContinuous(true);
Application.Current.RenderLoopSettings.VSyncEnabled = false;
Application.Current.RenderLoopSettings.TargetFps = 0; // 无限制
```

---

## 5. 关闭流程

- `Window.Close()` → 销毁后端 → 从 Application 注销
- 当最后一个窗口关闭时，平台循环可能退出（取决于平台策略）
- `Application.Quit()` 显式终止循环

---

## 6. 异常处理

- UI 线程上的异常将路由到 `Application.DispatcherUnhandledException`
- 默认情况下，未处理的异常被视为致命错误

#### 示例：处理 DispatcherUnhandledException
```csharp
Application.DispatcherUnhandledException += e =>
{
    try
    {
        MessageBox.Show(e.Exception.ToString(), "未处理的 UI 异常");
    }
    catch
    {
        // 忽略
    }
    e.Handled = true;
};
```

---

## 7. 总结

- 核心流程为：**运行前配置 → Run → 消息循环**
- Theme/RenderLoop 应在 Run 之前决定
- 窗口仅在 Show 时获取本机资源