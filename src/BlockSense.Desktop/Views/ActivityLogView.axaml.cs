using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.Formatting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class ActivityLogView : UserControl
{
    private readonly IActivityLogService _activityLogService;
    private readonly ICurrentUserProvider _currentUserProvider;

    private List<ActivityLogDto> _allLogs = [];
    private List<ActivityLogDto> _filteredLogs = [];

    private string _activeTypeFilter = "all";

    private ulong _newestKnownId = 0;
    private int _currentPageNum = 1;

    private int TotalFilteredPages => Math.Max(1, (int)Math.Ceiling(_filteredLogs.Count / (double)PageSize));

    private const int PageSize = 20;

    private CancellationTokenSource _cts = new();

    public ActivityLogView()
    {
        _activityLogService = App.ServiceProvider.GetRequiredService<IActivityLogService>();
        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>();

        InitializeComponent();
        WireEvents();
        LoadAsync();
    }

    public async void LoadAsync()
    {
        _currentPageNum = 1;
        await FetchAllAndRenderAsync();
    }

    private void WireEvents()
    {
        CloseActivityLogButton.Click += async (_, _) => await RequestClose();
        RefreshButton.Click += async (_, _) => await FetchAfterIdAsync();

        PrevPageButton.Click += (_, _) =>
        {
            if (_currentPageNum > 1) { _currentPageNum--; RenderPage(); }
        };

        NextPageButton.Click += (_, _) =>
        {
            if (_currentPageNum < TotalFilteredPages) { _currentPageNum++; RenderPage(); }
        };

        ChipAll.PointerPressed += (_, _) => SetTypeFilter("all");
        ChipUser.PointerPressed += (_, _) => SetTypeFilter("user");
        ChipSystem.PointerPressed += (_, _) => SetTypeFilter("system");
        ChipCron.PointerPressed += (_, _) => SetTypeFilter("cron");
    }

    // ── Data fetching ─────────────────────────────────────────────────

    private async Task FetchAllAndRenderAsync()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        SetLoadingState(true);

        try
        {
            var allLogs = new List<ActivityLogDto>();
            int page = 1;

            while (true)
            {
                var result = await _activityLogService.GetPageAsync(
                    page: page,
                    pageSize: 100,
                    cancellationToken: token);

                if (token.IsCancellationRequested) return;
                if (result is null) { ShowError("Failed to load activity logs."); return; }

                allLogs.AddRange(result.Entries);

                if (page >= result.TotalPages) break;
                page++;
            }

            _allLogs = [.. allLogs.OrderByDescending(l => l.OccurredAt)];

            if (_allLogs.Count > 0)
            {
                _newestKnownId = _allLogs.Max(l => l.Id);
                _currentUserProvider.SetRecentActivity(_allLogs.Take(3).ToList().AsReadOnly());
            }

            _currentPageNum = 1;
            ApplyFilters();
            RefreshBadge.IsVisible = false;
        }
        catch (OperationCanceledException) { /* superseded */ }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task FetchAfterIdAsync()
    {
        if (_newestKnownId == 0) { await FetchAllAndRenderAsync(); return; }

        RefreshButton.IsEnabled = false;

        var newer = await _activityLogService.GetLatestAsync(_newestKnownId);

        if (newer.Count == 0) { RefreshButton.IsEnabled = true; return; }

        await FetchAllAndRenderAsync();
    }

    // ── Filtering ─────────────────────────────────────────────────────

    private void SetTypeFilter(string type)
    {
        _activeTypeFilter = type;
        _currentPageNum = 1;
        UpdateChipStyles();
        ApplyFilters();
    }

    private void UpdateChipStyles()
    {
        (Border chip, TextBlock label, string key)[] chips =
        [
            (ChipAll,    ChipAllLabel,    "all"),
            (ChipUser,   ChipUserLabel,   "user"),
            (ChipSystem, ChipSystemLabel, "system"),
            (ChipCron,   ChipCronLabel,   "cron"),
        ];

        foreach (var (chip, label, key) in chips)
        {
            bool active = key == _activeTypeFilter;
            chip.Classes.Clear();
            chip.Classes.Add(active ? "FilterChipActive" : "FilterChip");
            label.Classes.Clear();
            label.Classes.Add(active ? "ChipLabelActive" : "ChipLabel");
        }
    }

    private void ApplyFilters()
    {
        var query = _allLogs.AsEnumerable();

        if (_activeTypeFilter != "all")
            query = query.Where(l =>
                l.Type.ToString().Equals(_activeTypeFilter, StringComparison.OrdinalIgnoreCase));

        _filteredLogs = [.. query];

        LogCountTextBlock.Text = $"{_filteredLogs.Count:N0} {(_filteredLogs.Count == 1 ? "entry" : "entries")}";

        RenderPage();
    }

    // ── Rendering ─────────────────────────────────────────────────────

    private void RenderPage()
    {
        LogRowsPanel.Children.Clear();

        var page = _filteredLogs
            .Skip((_currentPageNum - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        if (page.Count == 0)
        {
            LogRowsPanel.Children.Add(new TextBlock
            {
                Text = "No activity found.",
                Foreground = new SolidColorBrush(Color.Parse("#9E8572")),
                FontSize = 13,
                FontStyle = FontStyle.Italic,
                Margin = new Avalonia.Thickness(0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }
        else
        {
            foreach (var log in page)
                LogRowsPanel.Children.Add(CreateRow(log));
        }

        UpdatePager();
    }

    private void UpdatePager()
    {
        int total = TotalFilteredPages;

        PrevPageButton.IsEnabled = _currentPageNum > 1;
        NextPageButton.IsEnabled = _currentPageNum < total;
        PagerInfoTextBlock.Text = $"Page {_currentPageNum} of {total}";

        PageNumbersPanel.Children.Clear();

        int start = Math.Max(1, _currentPageNum - 2);
        int end = Math.Min(total, start + 4);
        start = Math.Max(1, end - 4);

        for (int p = start; p <= end; p++)
        {
            int page = p;
            bool isCurrent = page == _currentPageNum;

            var btn = new Button
            {
                Classes = { isCurrent ? "PagerBtnActive" : "PagerBtn" },
                Content = new TextBlock
                {
                    Text = page.ToString(),
                    FontSize = 12,
                    FontWeight = FontWeight.Medium,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.Parse(isCurrent ? "#F5E1C5" : "#6D4C41"))
                }
            };

            btn.Click += (_, _) => { _currentPageNum = page; RenderPage(); };
            PageNumbersPanel.Children.Add(btn);
        }
    }

    private static Control CreateRow(ActivityLogDto log)
    {
        var wrapper = new StackPanel { Spacing = 0 };
        wrapper.Children.Add(new Border { Classes = { "RowDivider" } });

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("160 * 80"),
            Margin = new Avalonia.Thickness(0, 10)
        };

        var dateBlock = new TextBlock
        {
            Classes = { "RowDate" },
            Text = DateTimeFormatter.ToOrdinalDate(log.OccurredAt)
        };
        Grid.SetColumn(dateBlock, 0);

        var msgBlock = new TextBlock
        {
            Classes = { "RowAction" },
            Text = log.ActivityMessage,
            Margin = new Avalonia.Thickness(12, 0)
        };
        Grid.SetColumn(msgBlock, 1);

        var (bgHex, fgHex) = TypeColors(log.Type);
        var pill = new Border
        {
            Classes = { "TypePill" },
            Background = new SolidColorBrush(Color.Parse(bgHex)),
            Child = new TextBlock
            {
                Classes = { "TypePillText" },
                Text = log.Type.ToString().ToUpperInvariant(),
                Foreground = new SolidColorBrush(Color.Parse(fgHex))
            }
        };
        Grid.SetColumn(pill, 2);

        grid.Children.Add(dateBlock);
        grid.Children.Add(msgBlock);
        grid.Children.Add(pill);
        wrapper.Children.Add(grid);
        return wrapper;
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static (string bg, string fg) TypeColors(ActivityType type) => type switch
    {
        ActivityType.User => ("#E8F5E9", "#2E7D32"),
        ActivityType.System => ("#E3F2FD", "#1565C0"),
        ActivityType.Cron => ("#FFF8E1", "#F57F17"),
        _ => ("#EDE7DE", "#6D4C41")
    };

    private void SetLoadingState(bool loading)
    {
        if (loading) LogCountTextBlock.Text = "Loading…";
        RefreshButton.IsEnabled = !loading;
    }

    private void ShowError(string message)
    {
        LogRowsPanel.Children.Clear();
        LogRowsPanel.Children.Add(new TextBlock
        {
            Text = message,
            Foreground = new SolidColorBrush(Color.Parse("#C62828")),
            FontSize = 13,
            Margin = new Avalonia.Thickness(0, 20),
            HorizontalAlignment = HorizontalAlignment.Center
        });
    }

    public event Func<Task>? CloseRequested;

    private async Task RequestClose()
    {
        if (CloseRequested is not null)
            await CloseRequested.Invoke();
    }
}