using ClubIt.Mobile.Core.ViewModels;

namespace ClubIt.Mobile.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
