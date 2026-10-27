using ClubIt.Mobile.Core.Services;

namespace ClubIt.Mobile;

public partial class App : Application
{
	private readonly AppShell _shell;
	private readonly ISessionStore _session;
	private readonly INavigationService _navigation;

	public App(AppShell shell, ISessionStore session, INavigationService navigation, ClubItApiClient api)
	{
		InitializeComponent();
		_shell = shell;
		_session = session;
		_navigation = navigation;

		api.SessionExpired += async (_, _) =>
		{
			await _session.ClearAsync();
			await _navigation.ShowLoginAsync();
		};
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// The whole UI is Hebrew, laid out right-to-left.
		var window = new Window(_shell) { FlowDirection = FlowDirection.RightToLeft };
		window.Created += async (_, _) =>
		{
			await _session.LoadAsync();
			if (_session.IsSignedIn)
			{
				await _navigation.ShowMainAsync();
			}
		};
		return window;
	}
}
