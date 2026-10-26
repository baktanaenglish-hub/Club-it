using ClubIt.Mobile.Core.Services;
using ClubIt.Mobile.Core.ViewModels;
using ClubIt.Mobile.Pages;
using ClubIt.Mobile.Services;
using Microsoft.Extensions.Logging;

namespace ClubIt.Mobile;

public static class MauiProgram
{
	/// <summary>
	/// API address. In debug builds this points at the API running on the development machine
	/// (the Android emulator reaches the host as 10.0.2.2). Change it for real devices and production.
	/// </summary>
	public static Uri ApiBaseAddress { get; } =
#if DEBUG
		DeviceInfo.Platform == DevicePlatform.Android
			? new Uri("http://10.0.2.2:5136/")
			: new Uri("http://localhost:5136/");
#else
		new Uri("https://api.clubit.example/");
#endif

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
		services.AddSingleton(sp => new ClubItApiClient(
			new HttpClient { BaseAddress = ApiBaseAddress, Timeout = TimeSpan.FromSeconds(30) },
			sp.GetRequiredService<ISessionStore>()));
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
