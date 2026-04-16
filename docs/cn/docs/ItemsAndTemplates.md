# 项与模板

本文档介绍 MewUI 中当前实现的项与模板系统。

## 概述

模板用于将项转换为可复用的 `FrameworkElement` 实例。核心流程如下：

1.  为容器生成一次视图。
2.  当视图实现化时，将数据绑定到视图中。
3.  当视图被回收时，重置已跟踪的资源。

这是 `ListBox`、`ComboBox`、`TreeView` 和 `GridView` 等项控件所使用的机制。

## Items 概述

MewUI 的项控件由 `ItemsView` 抽象驱动。在实际操作中：

1.  `Items(...)` 创建或包装一个 `ItemsView`。
2.  控件向 `ItemsView` 请求项数、文本和选择状态。
3.  模板为可见项创建并绑定视图。

`ItemsView` 代表数据端，而模板代表视图端，它们被设计为协同工作。

## 核心类型

### IDataTemplate

`IDataTemplate` 定义了用于生成和绑定视图的协定。

```csharp
public interface IDataTemplate
{
    FrameworkElement Build(TemplateContext context);
    void Bind(FrameworkElement view, object? item, int index, TemplateContext context);
}
```

`IDataTemplate<TItem>` 提供了类型安全的绑定。

```csharp
public interface IDataTemplate<in TItem> : IDataTemplate
{
    void Bind(FrameworkElement view, TItem item, int index, TemplateContext context);
}
```

### DelegateTemplate

`DelegateTemplate<TItem>` 是标准实现。通过它也能很好地理解默认模板行为：简单的生成操作会创建一个 `TextBlock`，而绑定操作则通过 `GetText` 或 `ToString()` 来赋值文本，并在需要时使用 `TemplateContext` 进行快速查找。

```csharp
var template = new DelegateTemplate<Person>(
    build: ctx =>
    {
        // 默认模板形状：一个单一的 TextBlock。
        // 当需要命名访问和复用时，请使用 TemplateContext。
        return new TextBlock().Register(ctx, "Text");
    },
    bind: (view, item, index, ctx) =>
    {
        ctx.Get<TextBlock>("Text").Text = item.Name;
    });
```

如果不需要 `TemplateContext`，也可以直接返回视图：

```csharp
var template = new DelegateTemplate<Person>(
    build: _ => new TextBlock(),
    bind: (view, item, index, _) => ((TextBlock)view).Text = item.Name);
```

### TemplateContext

`TemplateContext` 用于：

1.  注册命名元素以便快速查找。

```csharp
public sealed class TemplateContext : IDisposable
{
    public void Register<T>(string name, T element) where T : UIElement;
    public T Get<T>(string name) where T : UIElement;
    // 内部生命周期管理（不属于公共协定的一部分）。
}
```

常见用法：

```csharp
ctx.Get<TextBlock>("Name").Text = item.Name;
```

## 模板生命周期

1.  创建容器时，`Build` 会被调用一次。
2.  当容器为某个项实现化时，`Bind` 会被调用。
3.  在每次 `Bind` 调用之前以及回收过程中，系统会在内部处理上下文的清理工作。

这使得容器可以安全地复用，而不会泄漏订阅或状态。

## TemplatedItemsHost 与虚拟化

`TemplatedItemsHost` 是项控件内部使用的帮助程序。

职责包括：

1.  使用 `IDataTemplate.Build` 创建容器。
2.  使用 `IDataTemplate.Bind` 绑定项。
3.  在复用容器时重置 `TemplateContext`。
4.  将实际的虚拟化和布局工作委托给 `VirtualizedItemsPresenter`。

这是 `ListBox`、`ComboBox`、`TreeView` 和 `GridView` 共用的通用路径。

## 控件用法

### ListBox

```csharp
new ListBox()
    .Items(people, p => p.Name)
    .ItemTemplate(template);
```

如果未设置 `ItemTemplate`，将使用默认模板（`TextBlock` + `GetText`/`ToString()`）。

```csharp
// 使用默认模板
new ListBox().Items(people, p => p.Name);
```

`Items(...)` 的第二个参数是一个文本选择器。它告诉默认模板为每个项显示什么字符串（由 `TextBlock` 使用）。

### ComboBox

```csharp
new ComboBox()
    .Items(people, p => p.Name)
    .ItemTemplate(template);
```

```csharp
// 使用默认模板
new ComboBox().Items(people, p => p.Name);
```

`Items(...)` 的第二个参数是供默认模板使用的文本选择器。

### TreeView

```csharp
new TreeView()
    .Items(treeItems)
    .ItemTemplate(template);
```

```csharp
// 使用默认模板
new TreeView().Items(treeItems);
```

也可以直接传递分层数据源：

```csharp
new TreeView().Items(
    roots,
    childrenSelector: n => n.Children,
    textSelector: n => n.Name,
    keySelector: n => n.Id);
```

### GridView

GridView 的列使用模板来呈现单元格。

```csharp
var grid = new GridView();
grid.Columns(
    new GridViewColumn<Person>()
        .Header("Name")
        .Width(160)
        .Bind(
            build: ctx => new TextBlock().Register(ctx, "Text"),
            bind: (TextBlock t, Person p, int _, TemplateContext __) => t.Text = p.Name));
```

## 默认模板

如果未提供模板，项控件的行为与上述示例类似：会创建一个 `TextBlock`，并使用 `GetText` 或 `ToString()` 填充其文本。

这保持了行为的一致性，同时允许用户在需要时通过模板进行覆盖。

## 推荐模式

1.  使用 `TemplateContext.Register` 和 `Get` 处理命名元素。
2.  使用 `TemplateContext.Track` 注册订阅和非托管资源。
3.  避免在 `Bind` 期间创建重量级对象；应在 `Build` 中复用对象。
4.  始终假定 `Bind` 可能会在同一容器上被重复调用。

## 简化重载（单一视图）

当模板生成的是单一控件，并且你不需要命名查找或跟踪可释放资源时，可以使用在用户代码中忽略 `TemplateContext` 的重载。系统仍然会在内部创建上下文，但你无需使用它。

```csharp
// 生成一个单一视图并仅绑定项（不使用上下文）。
listBox.ItemTemplate(
    build: _ => new TextBlock(),
    bind: (TextBlock view, Person item) => view.Text = item.Name);
```

这为常见场景简化了 API，同时保留了相同的模板管道。
