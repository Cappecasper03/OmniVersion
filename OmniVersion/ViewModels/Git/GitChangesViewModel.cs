using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LibGit2Sharp;

namespace OmniVersion.ViewModels.Git;

public partial class GitChangesViewModel : ViewModelBase
{
    [ObservableProperty]
    private Repository? _repository;

    [ObservableProperty]
    private GitFileTree? _unstagedFileTree;

    [ObservableProperty]
    private GitFileTreeViewModel? _selectedUnstaged;

    [ObservableProperty]
    private string _unstagedCount = "(0)";

    [ObservableProperty]
    private GitFileTree? _stagedFileTree;

    [ObservableProperty]
    private GitFileTreeViewModel? _selectedStaged;

    [ObservableProperty]
    private string _stagedCount = "(0)";

    private CancellationTokenSource UpdateFileTreesCancellationSource { get; } = new();


    public void StartUpdating()
    {
        Task task = UpdateFileTreesAsync(TimeSpan.FromSeconds(1), UpdateFileTreesCancellationSource.Token);
    }

    public void StopUpdating()
    {
        UpdateFileTreesCancellationSource.Cancel();
    }

    private async Task UpdateFileTreesAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (Repository == null)
                continue;

            RepositoryStatus         status   = Repository.RetrieveStatus();
            IEnumerable<StatusEntry> unstaged = status.Where(x => x.State.ToString().Contains("InWorkdir", StringComparison.OrdinalIgnoreCase)).ToArray();
            IEnumerable<StatusEntry> staged   = status.Where(x => x.State.ToString().Contains("InIndex",   StringComparison.OrdinalIgnoreCase)).ToArray();


            ObservableCollection<GitFileTreeViewModel> unstagedNodes = BuildFileTree(unstaged);
            GitFileTreeViewModel.SortFileTree(unstagedNodes);
            var unstagedSortedNodes = new ObservableCollection<GitFileTreeViewModel>(unstagedNodes.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

            ObservableCollection<GitFileTreeViewModel> stagedNodes = BuildFileTree(staged);
            GitFileTreeViewModel.SortFileTree(stagedNodes);
            var stagedSortedNodes = new ObservableCollection<GitFileTreeViewModel>(stagedNodes.OrderBy(x => x.SortOrder).ThenBy(x => x.Name));

            Dispatcher.UIThread.Post(() =>
            {
                UnstagedFileTree = new GitFileTree(unstagedSortedNodes);
                UnstagedCount    = $"({unstaged.Count()})";

                StagedFileTree = new GitFileTree(stagedSortedNodes);
                StagedCount    = $"({staged.Count()})";
            });

            await Task.Delay(delay, cancellationToken);
        }
    }

    private static ObservableCollection<GitFileTreeViewModel> BuildFileTree(IEnumerable<StatusEntry> entries)
    {
        ObservableCollection<GitFileTreeViewModel> rootNodes      = [];
        Dictionary<string, GitFileTreeViewModel>   directoryNodes = [];

        foreach (StatusEntry entry in entries)
        {
            string[]                                   directoryParts  = entry.FilePath.Split('/');
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
                    parentNode = new GitFileTreeViewModel
                    {
                        Name      = part,
                        SortOrder = 0
                    };
                    directoryNodes[currentPath] = parentNode;
                    currentChildren.Add(parentNode);
                }

                currentChildren = parentNode.Children;
            }

            GitFileTreeViewModel fileNode = new GitFileTreeViewModel
            {
                Name      = directoryParts.Last(),
                Status    = entry.State,
                SortOrder = 1
            };

            currentChildren.Add(fileNode);
        }

        return rootNodes;
    }
}