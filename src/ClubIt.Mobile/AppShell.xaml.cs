using ClubIt.Mobile.Core.Services;
using ClubIt.Mobile.Pages;

namespace ClubIt.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Pages pushed on top of the tabs.
		Routing.RegisterRoute(Routes.BenefitDetails, typeof(BenefitDetailsPage));
		Routing.RegisterRoute(Routes.BenefitEdit, typeof(BenefitEditPage));
		Routing.RegisterRoute(Routes.Merchants, typeof(MerchantsPage));
	}
}
