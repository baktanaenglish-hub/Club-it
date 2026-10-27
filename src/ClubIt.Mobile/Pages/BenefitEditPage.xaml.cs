using ClubIt.Mobile.Core.Services;
using ClubIt.Mobile.Core.ViewModels;

namespace ClubIt.Mobile.Pages;

/// <summary>
/// Used both as the "Add" tab (no id) and as the edit screen pushed from a benefit card (with an id).
/// </summary>
public partial class BenefitEditPage : ContentPage, IQueryAttributable
{
	private readonly BenefitEditViewModel _viewModel;
	private int? _benefitId;
	private bool _loaded;

	public BenefitEditPage(BenefitEditViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(Routes.IdParameter, out var id))
		{
			_benefitId = Convert.ToInt32(id);
			_loaded = false;
		}
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Load once; returning from the merchants screen must not discard what was typed.
		if (!_loaded)
		{
			_loaded = true;
			await _viewModel.LoadAsync(_benefitId);
		}
	}
}
