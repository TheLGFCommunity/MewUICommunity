# 渲染循环概念

本文档总结了 MewUI 在近期更改后的渲染循环模型。这是一份面向内部的后端/平台行为及调度指南。

---

## 1. 目标

- 通过解耦渲染与消息处理，保持 UI 的响应性。
- 同时支持：
  - **按需渲染**（UI 失效触发渲染）
  - **持续渲染**（动画 / 最大 FPS 场景）
- 允许后端级别的垂直同步控制，并在 Win32、X11、D2D、OpenGL 和 GDI 之间实现一致的行为。

---

## 2. 模式

### 2.1 按需渲染（默认）

- `Window.Invalidate()` / `RequestRender()` 将窗口标记为需要渲染。
- 平台宿主等待渲染请求或操作系统消息。
- 收到信号时，仅 **失效的窗口** 进行渲染。
- 这类似于 WPF 风格的“合并”渲染，多个失效请求会合并处理。

### 2.2 持续渲染

- 宿主重复渲染 **每个窗口**，即使没有失效请求。
- 渲染循环可通过 `TargetFps`（可选）进行限流。
  `TargetFps = 0` 表示“无限制”。
- 此模式适用于动画和性能分析（最大 FPS）。

---

## 3. RenderLoopSettings

循环通过 `Application.Current.RenderLoopSettings` 进行配置：

- `Mode` → `OnRequest` 或 `Continuous`
- `TargetFps` → 帧率上限（0 = 无限制）
- `VSyncEnabled` → 后端呈现/交换行为

这些设置由平台宿主和图形后端读取。

---

## 4. 后端行为

### 4.1 Direct2D

- 使用 DXGI 呈现选项。
- `VSyncEnabled = false` → `PresentOptions = IMMEDIATELY`
- `VSyncEnabled = true` → 默认呈现（垂直同步由 DXGI/DWM 控制）

### 4.2 OpenGL

- 使用平台交换间隔（WGL/GLX）。
- `VSyncEnabled = false` → `SwapInterval = 0`
- `VSyncEnabled = true` → 默认垂直同步（通常为 1）

### 4.3 GDI

- GDI 没有垂直同步；`VSyncEnabled` 不会改变其行为。
- 渲染仍然参与按需和持续的调度。

---

## 5. 渲染与消息循环

- **按需渲染**：当设置渲染请求标志时触发渲染。
- **持续渲染**：每次循环迭代时执行渲染（并受限流控制）。
- 在这两种模式下，每次循环仍会处理操作系统消息。

平台宿主避免直接依赖 `WM_PAINT`，而是使用 `RenderIfNeeded` 或 `RenderNow` 进行绘制，以避免引发 `WM_PAINT` 风暴。

---

## 6. FPS 与诊断

- `Window.FrameRendered` 在每个渲染帧结束时触发。
- Sample 和 Gallery 使用此事件计算并显示 FPS。
- 在持续渲染模式下，`FrameRendered` 应在每帧更新。

---

## 7. 设计说明

- 该循环将渲染 **与失效分离**，以支持最大 FPS 模式。
- 在按需渲染模式下，渲染请求会被 **合并**，以避免重复工作。
- 持续渲染模式即使没有失效请求也会进行渲染，以便动画能够推进。

---

## 8. 未来扩展

- 保留/合成渲染层可以接入同一循环。
- 动画调度可以对齐到 `TargetFps` 以获得稳定的节奏。
- 以后可以添加按窗口调度（例如，仅在持续渲染模式下处理活动窗口）。
