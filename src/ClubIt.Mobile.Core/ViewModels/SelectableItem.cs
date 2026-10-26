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

public class SelectableClub(ClubIt.Contracts.Clubs.ClubDto club, bool isSelected = false)
    : SelectableItem<ClubIt.Contracts.Clubs.ClubDto>(club, isSelected)
{
    public string Name => Item.Name;
    public string TypeName => ClubSettingsViewModel.ClubTypeName(Item.Type);
}

public class SelectableBranch(ClubIt.Contracts.Merchants.BranchDto branch, bool isSelected = false)
    : SelectableItem<ClubIt.Contracts.Merchants.BranchDto>(branch, isSelected)
{
    public string DisplayName => string.IsNullOrEmpty(Item.City) ? Item.Name : $"{Item.Name} · {Item.City}";
}
