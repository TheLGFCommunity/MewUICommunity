# 安装与包结构

本文档介绍 MewUI 的 NuGet 包结构、安装方式以及发布时的后端选择。

---

## 1. 包结构

MewUI 组织为**元包**（便捷组合）和**独立包**（细粒度组装）。

```
Aprillz.MewUI                  ← 全功能元包（所有平台 + 所有后端）
├─ Aprillz.MewUI.Core          ← 核心（控件、布局、标记、绑定）
├─ Aprillz.MewUI.Platform.*    ← 平台宿主
│   ├─ .Platform.Win32
│   ├─ .Platform.X11
│   └─ .Platform.MacOS
└─ Aprillz.MewUI.Backend.*     ← 渲染后端
    ├─ .Backend.Direct2D        （Windows）
    ├─ .Backend.Gdi             （Windows）
    ├─ .Backend.MewVG.Win32     （Windows，NanoVG/OpenGL）
    ├─ .Backend.MewVG.X11       （Linux，NanoVG/OpenGL）
    └─ .Backend.MewVG.MacOS     （macOS，NanoVG/Metal）
```

单独管理的包（不包含在元包中）：
- `Aprillz.MewUI.Svg` — SVG 解析与渲染
- `Aprillz.MewUI.Win32.WebView2` — WebView2 集成（仅 Windows）

---

## 2. 安装

### 2.1 快速开始 —— 元包

大多数情况下，只需添加一个平台元包即可。

| 目标平台 | 包 | 包含内容 |
|----------------|---------|----------|
| **Windows** | `Aprillz.MewUI.Windows` | Core + Win32 + Direct2D + GDI + MewVG |
| **Linux** | `Aprillz.MewUI.Linux` | Core + X11 + MewVG |
| **macOS** | `Aprillz.MewUI.MacOS` | Core + MacOS + MewVG |
| **跨平台** | `Aprillz.MewUI` | 所有平台 + 所有后端 |

```bash
# Windows 应用
dotnet add package Aprillz.MewUI.Windows

# 跨平台应用
dotnet add package Aprillz.MewUI
```

### 2.2 独立包

可以不使用元包，仅引用所需的包。

```xml
<ItemGroup>
  <PackageReference Include="Aprillz.MewUI.Core" Version="0.10.3" />
  <PackageReference Include="Aprillz.MewUI.Platform.Win32" Version="0.10.3" />
  <PackageReference Include="Aprillz.MewUI.Backend.Gdi" Version="0.10.3" />
</ItemGroup>
```

### 2.3 附加包

单独添加 SVG 或 WebView2 支持。

```bash
dotnet add package Aprillz.MewUI.Svg
dotnet add package Aprillz.MewUI.Win32.WebView2
```

---

## 3. 发布时的后端选择

### 3.1 概述

元包包含了目标平台的所有后端。
在发布时，可使用 `MewUIBackend` 属性仅保留一个后端。
如果未指定，则发布输出中会包含所有后端。

### 3.2 CLI 命令行

```bash
# 仅 Direct2D
dotnet publish -r win-x64 -p:MewUIBackend=Direct2D

# 仅 GDI（轻量级）
dotnet publish -r win-x64 -p:MewUIBackend=Gdi

# 仅 MewVG
dotnet publish -r win-x64 -p:MewUIBackend=MewVG
```

### 3.3 在 csproj 中配置

```xml
<PropertyGroup>
  <MewUIBackend>Direct2D</MewUIBackend>
</PropertyGroup>
```

### 3.4 在发布配置文件中配置

```xml
<!-- Properties/PublishProfiles/Win-Direct2D.pubxml -->
<Project>
  <PropertyGroup>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <MewUIBackend>Direct2D</MewUIBackend>
  </PropertyGroup>
</Project>
```

```bash
dotnet publish -p:PublishProfile=Win-Direct2D
```

### 3.5 MewUIBackend 取值说明

| 值 | 保留 | 移除 |
|-------|------|---------|
| `Direct2D` | Backend.Direct2D | Backend.Gdi、Backend.MewVG.Win32 |
| `Gdi` | Backend.Gdi | Backend.Direct2D、Backend.MewVG.Win32 |
| `MewVG` | Backend.MewVG.* | Backend.Direct2D、Backend.Gdi |
| *（未设置）* | 全部 | — |

> Linux 和 macOS 仅包含 MewVG 后端，因此无需使用 `MewUIBackend`。

---

## 4. 跨平台发布（Aprillz.MewUI）

全功能元包（`Aprillz.MewUI`）包含每个平台的程序集，
但 `dotnet publish -r <rid>` **会自动排除非目标平台的程序集**。

| RID | 保留 | 自动移除 |
|-----|------|-------------|
| `win-x64` | Core、Win32、Direct2D、Gdi、MewVG.Win32 | X11、MacOS、MewVG.X11、MewVG.MacOS |
| `linux-x64` | Core、X11、MewVG.X11 | Win32、MacOS、Direct2D、Gdi、MewVG.Win32、MewVG.MacOS |
| `osx-arm64` | Core、MacOS、MewVG.MacOS | Win32、X11、Direct2D、Gdi、MewVG.Win32、MewVG.X11 |

RID 筛选与 `MewUIBackend` 筛选可组合使用。

```bash
# Windows + 仅 Direct2D
dotnet publish -r win-x64 -p:MewUIBackend=Direct2D
```

---

## 5. 渲染后端指南

| 后端 | 平台 | 说明 |
|---------|----------|-------|
| **Direct2D** | Windows | GPU 加速，高质量文本渲染。Windows 上推荐使用的默认后端 |
| **GDI** | Windows | 基于 CPU，超轻量，依赖最少 |
| **MewVG** | Windows、Linux、macOS | NanoVG 托管移植版。使用 OpenGL（Win32/X11）或 Metal（macOS） |

有关在应用代码中注册后端的内容，请参阅[应用程序生命周期](ApplicationLifecycle.md)。

---

## 6. 基于文件的应用（.NET 10+）

在 .NET 10 基于文件的应用中，使用 `#:package` 指令引用包。

```csharp
#:sdk Microsoft.NET.Sdk
#:property OutputType=Exe
#:property TargetFramework=net10.0

#:package Aprillz.MewUI@0.10.3

using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

// ...
Application.Run(window);
```

---

## 7. 版本兼容性

- 所有 MewUI 包（Core、Platform.*、Backend.*、元包）均使用**相同的版本号**发布。
- 引用单个元包会自动对齐所有依赖项的版本。
- 组合使用独立包时，请确保**所有包使用相同的版本**。
