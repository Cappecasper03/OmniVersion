using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LibGit2Sharp;

namespace OmniVersion.ViewModels.Git;

public partial class GitHistoryViewModel : ViewModelBase
{
    [ObservableProperty]
    private Repository? _repository;

    [ObservableProperty]
    private Commit? _selectedCommit;

    [ObservableProperty]
    private GitFileTree? _fileTreeChanges;

    [ObservableProperty]
    private GitFileTree? _fileTree;

    partial void OnSelectedCommitChanged(Commit? value)
    {
        if (value == null)
            return;

        UpdateFileTreeChanges(value);
        UpdateFileTree(value);
    }

    private void UpdateFileTreeChanges(Commit commit)
    {
        if (Repository == null)
            return;

        Task.Run(() =>
        {
            Commit?     parent  = commit.Parents.FirstOrDefault();
            TreeChanges changes = Repository.Diff.Compare<TreeChanges>(parent?.Tree, commit.Tree);

            ObservableCollection<GitFileTreeViewModel> rootNodes      = [];
            Dictionary<string, GitFileTreeViewModel>   directoryNodes = [];

            foreach (TreeEntryChanges change in changes)
            {
                string[]                                   directoryParts  = change.Path.Split('/');
                ObservableCollection<GitFileTreeViewModel> currentChildren = rootNodes;
                string                                     currentPath     = string.Empty;

                foreach (string part in directoryParts)
                {
                    if (part == directoryParts.Last())
                        break;

                    currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                    GitFileTreeViewModel? parentNode;
                    if (!directoryNodes.TryGetValue(currentPath, out parentNode))
                    {
                        parentNode                  = new GitFileTreeViewModel { Name = part, SortOrder = 0 };
                        directoryNodes[currentPath] = parentNode;
                        currentChildren.Add(parentNode);
                    }

                    currentChildren = parentNode.Children;
                }

                GitFileTreeViewModel fileNode = new GitFileTreeViewModel
                {
                    Name      = directoryParts.Last(),
                    Change    = change.Status,
                    SortOrder = change.Mode == Mode.GitLink ? 2 : 1
                };

                currentChildren.Add(fileNode);
            }

            GitFileTreeViewModel.SortFileTree(rootNodes);
            var sortedNodes = new ObservableCollection<GitFileTreeViewModel>(rootNodes.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));
            Dispatcher.UIThread.Post(() => { FileTreeChanges = new GitFileTree(sortedNodes); });
        });
    }

    private void UpdateFileTree(Commit commit)
    {
        if (Repository == null)
            return;

        Task.Run(() =>
        {
            ObservableCollection<GitFileTreeViewModel> rootNodes = BuildFileTreeRecursive(commit.Tree);
            GitFileTreeViewModel.SortFileTree(rootNodes);
            var sortedNodes = new ObservableCollection<GitFileTreeViewModel>(rootNodes.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));
            Dispatcher.UIThread.Post(() => { FileTree = new GitFileTree(sortedNodes); });
        });
    }

    private static ObservableCollection<GitFileTreeViewModel> BuildFileTreeRecursive(Tree? tree)
    {
        if (tree == null)
            return [];

        ObservableCollection<GitFileTreeViewModel> nodes = [];
        foreach (TreeEntry entry in tree)
        {
            switch (entry.TargetType)
            {
                case TreeEntryTargetType.Tree:
                {
                    GitFileTreeViewModel node = new GitFileTreeViewModel { Name = entry.Name, SortOrder = 0 };

                    foreach (GitFileTreeViewModel child in BuildFileTreeRecursive(entry.Target as Tree))
                        node.Children.Add(child);

                    nodes.Add(node);
                    break;
                }
                case TreeEntryTargetType.Blob:
                {
                    nodes.Add(new GitFileTreeViewModel { Name = entry.Name, SortOrder = 1 });
                    break;
                }
                case TreeEntryTargetType.GitLink:
                {
                    nodes.Add(new GitFileTreeViewModel { Name = entry.Name, SortOrder = 2 });
                    break;
                }
            }
        }

        return nodes;
    }
}