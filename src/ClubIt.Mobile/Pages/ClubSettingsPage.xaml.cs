using ClubIt.Mobile.Core.ViewModels;

namespace ClubIt.Mobile.Pages;

public partial class ClubSettingsPage : ContentPage
{
	private readonly ClubSettingsViewModel _viewModel;

	public ClubSettingsPage(ClubSettingsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadAsync();
	}
}
