using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using LibGit2Sharp;

namespace OmniVersion.ViewModels.Git;

public class GitFileTreeViewModel : ViewModelBase
{
    public string                                     Name      { get; init; } = string.Empty;
    public ChangeKind?                                Change    { get; init; }
    public int                                        SortOrder { get; init; }
    public ObservableCollection<GitFileTreeViewModel> Children  { get; set; } = [];
}

public class GitFileTree : HierarchicalTreeDataGridSource<GitFileTreeViewModel>
{
    public GitFileTree(IEnumerable<GitFileTreeViewModel> items)
        : base(items)
    {
        Columns.Add(new TextColumn<GitFileTreeViewModel, ChangeKind?>("Change", x => x.Change));
        Columns.Add(new HierarchicalExpanderColumn<GitFileTreeViewModel>(new TextColumn<GitFileTreeViewModel, string>("Name", x => x.Name), x => x.Children));
    }
}