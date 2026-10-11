using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class UsersView : UserControl
{
    public UsersView()
    {
        InitializeComponent();
    }

    public UsersView(UsersViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
