using CommunityToolkit.Mvvm.ComponentModel;

namespace ClubIt.Mobile.Core.ViewModels;

/// <summary>Wraps an item with a check-box state for multi-select lists.</summary>
public partial class SelectableItem<T>(T item, bool isSelected = false) : ObservableObject
{
    public T Item { get; } = item;

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = isSelected;
}

public record Option<T>(T Value, string Text)
{
    public override string ToString() => Text;
}
