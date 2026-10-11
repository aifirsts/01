using System.Windows.Controls;
using SOCpit.App.ViewModels;

namespace SOCpit.App.Views;

public partial class EvidenceView : UserControl
{
    public EvidenceView()
    {
        InitializeComponent();
    }

    public EvidenceView(EvidenceViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
