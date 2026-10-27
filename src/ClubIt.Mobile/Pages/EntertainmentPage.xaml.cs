using ClubIt.Mobile.Core.ViewModels;

namespace ClubIt.Mobile.Pages;

public partial class EntertainmentPage : ContentPage
{
	private readonly EntertainmentViewModel _viewModel;

	public EntertainmentPage(EntertainmentViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Club settings or benefits may have changed in another tab.
		await _viewModel.LoadAsync();
	}
}
