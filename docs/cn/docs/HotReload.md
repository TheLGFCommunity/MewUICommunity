# 热重载

本文档介绍 MewUI 应用中使用 C# 标记时的**可选热重载流程**。
热重载通过重新执行已注册的生成回调来重建**窗口**内容。

---

## 1. 启用热重载（应用程序集）

在应用程序集中添加 MetadataUpdateHandler（仅限 DEBUG 配置）：

```csharp
#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(
    typeof(Aprillz.MewUI.HotReload.MewUiMetadataUpdateHandler))]
#endif
```

可将其放置在 `Program.cs` 或专用的 `AssemblyInfo.cs` 中。

---

## 2. 注册生成回调

热重载仅会重建那些已注册生成回调的**窗口**。
C# 标记帮助方法 `OnBuild(...)` 用于设置回调，同时也会执行初始生成。

```csharp
var window = new Window()
    .OnBuild(w =>
    {
        w.Title("Hot Reload Demo");
        w.Content(new StackPanel()
            .Spacing(8)
            .Children(
                new TextBlock().Text("Edit code and Hot Reload."),
                new Button().Content("Click")
            ));
    });
```

---

## 3. 完整示例

```csharp
#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(
    typeof(Aprillz.MewUI.HotReload.MewUiMetadataUpdateHandler))]
#endif

using Aprillz.MewUI;
using Aprillz.MewUI.Markup;
using Aprillz.MewUI.Controls;

// 1) 注册生成回调（代码更改 + 热重载后，DateTime 会更新）
var window = new Window()
    .OnBuild(w => w
        .Title("Hot Reload Demo")
        .Content(new StackPanel()
            .Spacing(8)
            .Children(
                new TextBlock().Text($"Now: {DateTime.Now}"),
                new Button().Content("Click")
            )));

// 2) 选择平台/后端并运行
Application.Create()
    .UseWin32()
    .UseDirect2D()
    .Run(window);
```

---

## 4. 说明

- 热重载是**可选的**。未提供生成回调则不会重载。
- 重建在 UI 线程上运行，只会重新调用你的生成回调。
- 如需保留状态，请将其保存在自己的视图模型或服务中。
- 热重载**在 NativeAOT 下不受支持**。
- 可通过 `MewUiHotReload.RequestReload()` 手动触发热重载。