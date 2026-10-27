using ClubIt.Mobile.Core.Services;
using ClubIt.Mobile.Core.ViewModels;

namespace ClubIt.Mobile.Pages;

public partial class BenefitDetailsPage : ContentPage, IQueryAttributable
{
	private readonly BenefitDetailsViewModel _viewModel;
	private int? _benefitId;

	public BenefitDetailsPage(BenefitDetailsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(Routes.IdParameter, out var id))
		{
			_benefitId = Convert.ToInt32(id);
		}
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Reload every time, so changes made on the edit screen show up when returning.
		if (_benefitId is { } id)
		{
			await _viewModel.LoadAsync(id);
		}
	}
}
