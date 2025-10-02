using Avalonia.Controls;
using OmniVersion.ViewModels.Git;

namespace OmniVersion.Views.Git;

public partial class GitRepositoryView : UserControl
{
    public GitRepositoryView()
    {
        InitializeComponent();

        DataContext = new GitRepositoryViewModel();
    }
}