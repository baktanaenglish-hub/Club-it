using ClubIt.Mobile.Core.Services;
using ClubIt.Mobile.Core.ViewModels;
using ClubIt.Mobile.Pages;
using ClubIt.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace ClubIt.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		var services = builder.Services;
		services.AddSingleton(TimeProvider.System);
		services.AddSingleton<ISessionStore, SecureSessionStore>();
		services.AddSingleton<INavigationService, ShellNavigationService>();
		services.AddSingleton<IDialogService, DialogService>();
		services.AddSingleton<IUrlLauncher, UrlLauncher>();
		services.AddSingleton<IServerSettings, PreferencesServerSettings>();
		services.AddSingleton(sp => new ClubItApiClient(
			// A free-tier server that went to sleep needs up to about a minute to wake up.
			new HttpClient { Timeout = TimeSpan.FromSeconds(90) },
			sp.GetRequiredService<ISessionStore>(),
			sp.GetRequiredService<IServerSettings>()));
		services.AddSingleton<IClubItApi>(sp => sp.GetRequiredService<ClubItApiClient>());

		services.AddTransient<LoginViewModel>();
		services.AddTransient<SearchViewModel>();
		services.AddTransient<BenefitDetailsViewModel>();
		services.AddTransient<EntertainmentViewModel>();
		services.AddTransient<BenefitEditViewModel>();
		services.AddTransient<ClubSettingsViewModel>();
		services.AddTransient<MerchantsViewModel>();

		services.AddTransient<LoginPage>();
		services.AddTransient<SearchPage>();
		services.AddTransient<BenefitDetailsPage>();
		services.AddTransient<EntertainmentPage>();
		services.AddTransient<BenefitEditPage>();
		services.AddTransient<ClubSettingsPage>();
		services.AddTransient<MerchantsPage>();

		services.AddSingleton<AppShell>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
