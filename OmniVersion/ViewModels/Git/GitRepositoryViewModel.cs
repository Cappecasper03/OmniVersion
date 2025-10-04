using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibGit2Sharp;

namespace OmniVersion.ViewModels.Git;

public partial class GitRepositoryViewModel : ViewModelBase
{
    private Repository Repository { get; set; } = new("D:/Github/sourcegit");

    [ObservableProperty]
    private GitHistoryViewModel _gitHistory = new();

    [ObservableProperty]
    private GitChangesViewModel _gitChanges = new();

    [ObservableProperty]
    private bool _isHistoryVisible = true;

    [ObservableProperty]
    private bool _isChangesVisible;

    [ObservableProperty]
    private bool _isStashesVisible;

    public GitRepositoryViewModel()
    {
        GitHistory.Repository = Repository;
        GitChanges.Repository = Repository;
    }

    [RelayCommand]
    private void ShowHistory()
    {
        IsHistoryVisible = true;
        IsChangesVisible = false;
        IsStashesVisible = false;

        GitChanges.StopUpdating();
    }

    [RelayCommand]
    private void ShowChanges()
    {
        IsHistoryVisible = false;
        IsChangesVisible = true;
        IsStashesVisible = false;

        GitChanges.StartUpdating();
    }

    [RelayCommand]
    private void ShowStashes()
    {
        IsHistoryVisible = false;
        IsChangesVisible = false;
        IsStashesVisible = true;

        GitChanges.StopUpdating();
    }
}