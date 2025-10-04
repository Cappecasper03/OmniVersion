using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using LibGit2Sharp;

namespace OmniVersion.ViewModels.Git;

public class GitFileTree : HierarchicalTreeDataGridSource<GitFileTreeViewModel>
{
    public GitFileTree(IEnumerable<GitFileTreeViewModel> items)
        : base(items)
    {
        Columns.Add(new HierarchicalExpanderColumn<GitFileTreeViewModel>(new TextColumn<GitFileTreeViewModel, string>("Name", x => x.Name), x => x.Children));
    }
}

public class GitFileTreeViewModel : ViewModelBase
{
    public string                                     Name      { get; init; } = string.Empty;
    public ChangeKind?                                Change    { get; init; }
    public FileStatus?                                Status    { get; init; }
    public int                                        SortOrder { get; init; }
    public ObservableCollection<GitFileTreeViewModel> Children  { get; private set; } = [];

    public static void SortFileTree(ObservableCollection<GitFileTreeViewModel> fileTree)
    {
        foreach (GitFileTreeViewModel node in fileTree)
        {
            node.Children = new ObservableCollection<GitFileTreeViewModel>(node.Children.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));
            SortFileTree(node.Children);
        }
    }
}