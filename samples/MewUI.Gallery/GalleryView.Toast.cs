using Aprillz.MewUI.Controls;

namespace Aprillz.MewUI.Gallery;

partial class GalleryView
{
    private FrameworkElement ToastPage() =>
        CardGrid(
            Card(
                "Toast",
                new StackPanel()
                    .Vertical()
                    .Spacing(8)
                    .Children(
                        new Button()
                            .Content("Show Toast")
                            .OnClick(() => window.ShowToast("Hello, Toast!")),
                        new Button()
                            .Content("Long Message")
                            .OnClick(() => window.ShowToast("This is a longer toast message to test auto-dismiss duration scaling.")),
                        new Button()
                            .Content("Rapid Fire")
                            .OnClick(() => window.ShowToast($"Toast at {DateTime.Now:HH:mm:ss}"))
                    )
            )
        );
}
