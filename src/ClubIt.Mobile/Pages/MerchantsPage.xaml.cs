using ClubIt.Mobile.Core.ViewModels;

namespace ClubIt.Mobile.Pages;

public partial class MerchantsPage : ContentPage
{
	private readonly MerchantsViewModel _viewModel;

	public MerchantsPage(MerchantsViewModel viewModel)
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
