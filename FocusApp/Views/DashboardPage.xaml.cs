using FocusApp.ViewModels;

namespace FocusApp.Views;

public partial class DashboardPage : ContentPage
{
	public DashboardPage(DashboardViewModel viewModel)
	{
        InitializeComponent();
		BindingContext = viewModel;
	}
}