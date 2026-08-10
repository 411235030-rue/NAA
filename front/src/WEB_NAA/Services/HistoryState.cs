namespace WEB_NAA.Services;

public sealed class HistoryState
{
    public event Action? RefreshRequested;
    public event Action<HistoryRecord?>? SelectionChanged;

    public HistoryRecord? SelectedHistory { get; private set; }

    public void RequestRefresh()
    {
        RefreshRequested?.Invoke();
    }

    public void Select(HistoryRecord history)
    {
        SelectedHistory = history;
        SelectionChanged?.Invoke(history);
    }

    public void ClearSelection()
    {
        SelectedHistory = null;
        SelectionChanged?.Invoke(null);
    }
}
