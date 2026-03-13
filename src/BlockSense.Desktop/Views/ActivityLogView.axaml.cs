using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Services.Interfaces;
using BlockSense.Desktop.Utilities.Formatting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

/// <summary>
/// A bottom-sheet view that displays a paginated, filterable list of activity-log entries
/// for the current user.
/// </summary>
public partial class ActivityLogView : UserControl
{
    private const int PageSize = 20;

    private readonly IActivityLogService _activityLogService;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<ActivityLogView> _logger;

    private List<ActivityLogDto> _allLogs = [];
    private List<ActivityLogDto> _filteredLogs = [];

    private string _activeTypeFilter = "all";
    private ulong _newestKnownId = 0;
    private int _currentPageNumber = 1;

    private CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>Gets the total number of pages for the current filtered result set.</summary>
    private int TotalFilteredPages => Math.Max(1, (int)Math.Ceiling(_filteredLogs.Count / (double)PageSize));

    /// <summary>
    /// Raised when the user requests this view to be closed.
    /// </summary>
    public event Func<Task>? CloseRequested;

    /// <summary>
    /// Initialises a new instance of <see cref="ActivityLogView"/>.
    /// </summary>
    public ActivityLogView()
    {
        _activityLogService = App.ServiceProvider.GetRequiredService<IActivityLogService>();
        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>();
        _logger = App.ServiceProvider.GetRequiredService<ILogger<ActivityLogView>>();

        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        DetachedFromVisualTree += OnDetachedFromVisualTree;
    }

    /// <summary>
    /// Resets to page 1 and performs a full log reload. Safe to call externally.
    /// </summary>
    public async void LoadAsync()
    {
        _currentPageNumber = 1;
        await FetchAllLogsAndRenderAsync();
    }

    private void OnAttachedToVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        CloseActivityLogButton.Click += OnCloseActivityLogButtonClicked;
        RefreshButton.Click += OnRefreshButtonClicked;
        PrevPageButton.Click += OnPrevPageButtonClicked;
        NextPageButton.Click += OnNextPageButtonClicked;
        ChipAll.PointerPressed += OnChipAllPressed;
        ChipUser.PointerPressed += OnChipUserPressed;
        ChipSystem.PointerPressed += OnChipSystemPressed;
        ChipCron.PointerPressed += OnChipCronPressed;

        LoadAsync();
    }

    private void OnDetachedFromVisualTree(object? sender, Avalonia.VisualTreeAttachmentEventArgs e)
    {
        CloseActivityLogButton.Click -= OnCloseActivityLogButtonClicked;
        RefreshButton.Click -= OnRefreshButtonClicked;
        PrevPageButton.Click -= OnPrevPageButtonClicked;
        NextPageButton.Click -= OnNextPageButtonClicked;
        ChipAll.PointerPressed -= OnChipAllPressed;
        ChipUser.PointerPressed -= OnChipUserPressed;
        ChipSystem.PointerPressed -= OnChipSystemPressed;
        ChipCron.PointerPressed -= OnChipCronPressed;

        _cancellationTokenSource.Cancel();
    }

    private async void OnCloseActivityLogButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (CloseRequested is not null)
            await CloseRequested.Invoke();
    }

    private async void OnRefreshButtonClicked(object? sender, RoutedEventArgs e)
        => await FetchNewLogsAsync();

    private void OnPrevPageButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentPageNumber <= 1)
            return;

        _currentPageNumber--;
        RenderCurrentPage();
    }

    private void OnNextPageButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (_currentPageNumber >= TotalFilteredPages)
            return;

        _currentPageNumber++;
        RenderCurrentPage();
    }

    private void OnChipAllPressed(object? sender, PointerPressedEventArgs e) => SetTypeFilter("all");
    private void OnChipUserPressed(object? sender, PointerPressedEventArgs e) => SetTypeFilter("user");
    private void OnChipSystemPressed(object? sender, PointerPressedEventArgs e) => SetTypeFilter("system");
    private void OnChipCronPressed(object? sender, PointerPressedEventArgs e) => SetTypeFilter("cron");

    private async Task FetchAllLogsAndRenderAsync()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        SetLoadingState(isLoading: true);

        try
        {
            var aggregated = new List<ActivityLogDto>();
            int page = 1;

            while (true)
            {
                var result = await _activityLogService.GetPageAsync(
                    page: page,
                    pageSize: 100,
                    cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                if (result is null)
                {
                    ShowErrorMessage("Failed to load activity logs.");
                    return;
                }

                aggregated.AddRange(result.Entries);

                if (page >= result.TotalPages)
                    break;

                page++;
            }

            _allLogs = [.. aggregated.OrderByDescending(log => log.OccurredAt)];

            if (_allLogs.Count > 0)
            {
                _newestKnownId = _allLogs.Max(log => log.Id);
                _currentUserProvider.SetRecentActivity(_allLogs.Take(3).ToList().AsReadOnly());
            }

            _currentPageNumber = 1;
            ApplyFiltersAndRender();
            RefreshBadge.IsVisible = false;

            _logger.LogDebug("Loaded {Count} activity log entries.", _allLogs.Count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Activity log fetch was cancelled.");
        }
        finally
        {
            SetLoadingState(isLoading: false);
        }
    }

    private async Task FetchNewLogsAsync()
    {
        if (_newestKnownId == 0)
        {
            await FetchAllLogsAndRenderAsync();
            return;
        }

        RefreshButton.IsEnabled = false;

        var newerEntries = await _activityLogService.GetLatestAsync(_newestKnownId);

        if (newerEntries.Count == 0)
        {
            RefreshButton.IsEnabled = true;
            return;
        }

        await FetchAllLogsAndRenderAsync();
    }

    private void SetTypeFilter(string type)
    {
        _activeTypeFilter = type;
        _currentPageNumber = 1;
        UpdateFilterChipStyles();
        ApplyFiltersAndRender();
    }

    private void UpdateFilterChipStyles()
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
            bool isActive = key == _activeTypeFilter;

            chip.Classes.Clear();
            chip.Classes.Add(isActive ? "ActivityFilterChipActive" : "ActivityFilterChip");

            label.Classes.Clear();
            label.Classes.Add(isActive ? "ActivityFilterChipLabelActive" : "ActivityFilterChipLabel");
        }
    }

    private void ApplyFiltersAndRender()
    {
        var query = _allLogs.AsEnumerable();

        if (_activeTypeFilter != "all")
        {
            query = query.Where(log =>
                log.Type.ToString().Equals(_activeTypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        _filteredLogs = [.. query];

        var entryWord = _filteredLogs.Count == 1 ? "entry" : "entries";
        LogCountTextBlock.Text = $"{_filteredLogs.Count:N0} {entryWord}";

        RenderCurrentPage();
    }

    private void RenderCurrentPage()
    {
        LogRowsPanel.Children.Clear();

        var pageEntries = _filteredLogs
            .Skip((_currentPageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        if (pageEntries.Count == 0)
            LogRowsPanel.Children.Add(BuildEmptyPlaceholder());
        else
            foreach (var log in pageEntries)
                LogRowsPanel.Children.Add(BuildLogRow(log));

        UpdatePagerControls();
    }

    private void UpdatePagerControls()
    {
        int total = TotalFilteredPages;

        PrevPageButton.IsEnabled = _currentPageNumber > 1;
        NextPageButton.IsEnabled = _currentPageNumber < total;
        PagerInfoTextBlock.Text = $"Page {_currentPageNumber} of {total}";

        PageNumbersPanel.Children.Clear();

        int windowStart = Math.Max(1, _currentPageNumber - 2);
        int windowEnd = Math.Min(total, windowStart + 4);
        windowStart = Math.Max(1, windowEnd - 4);

        for (int pageNumber = windowStart; pageNumber <= windowEnd; pageNumber++)
        {
            int capturedPage = pageNumber;
            bool isCurrentPage = capturedPage == _currentPageNumber;

            var label = new TextBlock
            {
                Text = capturedPage.ToString(),
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.Medium,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.Parse(isCurrentPage ? "#F5E1C5" : "#6D4C41"))
            };

            var pageButton = new Button
            {
                Classes = { isCurrentPage ? "ActivityPagerButtonActive" : "ActivityPagerButton" },
                Content = label
            };

            pageButton.Click += (_, _) => { _currentPageNumber = capturedPage; RenderCurrentPage(); };
            PageNumbersPanel.Children.Add(pageButton);
        }
    }

    private static Control BuildLogRow(ActivityLogDto log)
    {
        var (backgroundHex, foregroundHex) = ResolveTypeColors(log.Type);

        var dateLabel = new TextBlock
        {
            Classes = { "ActivityRowDate" },
            Text = DateTimeFormatter.ToOrdinalDate(log.OccurredAt)
        };
        Grid.SetColumn(dateLabel, 0);

        var messageLabel = new TextBlock
        {
            Classes = { "ActivityRowAction" },
            Text = log.ActivityMessage,
            Margin = new Avalonia.Thickness(12, 0)
        };
        Grid.SetColumn(messageLabel, 1);

        var typePill = new Border
        {
            Classes = { "ActivityTypePill" },
            Background = new SolidColorBrush(Color.Parse(backgroundHex)),
            Child = new TextBlock
            {
                Classes = { "ActivityTypePillText" },
                Text = log.Type.ToString().ToUpperInvariant(),
                Foreground = new SolidColorBrush(Color.Parse(foregroundHex))
            }
        };
        Grid.SetColumn(typePill, 2);

        var rowGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("160 * 80"),
            Margin = new Avalonia.Thickness(0, 10)
        };
        rowGrid.Children.Add(dateLabel);
        rowGrid.Children.Add(messageLabel);
        rowGrid.Children.Add(typePill);

        var wrapper = new StackPanel { Spacing = 0 };
        wrapper.Children.Add(new Border { Classes = { "ActivityRowDivider" } });
        wrapper.Children.Add(rowGrid);

        return wrapper;
    }

    private static TextBlock BuildEmptyPlaceholder() => new()
    {
        Text = "No activity found.",
        Foreground = new SolidColorBrush(Color.Parse("#9E8572")),
        FontSize = 13,
        FontStyle = Avalonia.Media.FontStyle.Italic,
        Margin = new Avalonia.Thickness(0, 20),
        HorizontalAlignment = HorizontalAlignment.Center
    };

    private void SetLoadingState(bool isLoading)
    {
        if (isLoading)
            LogCountTextBlock.Text = "Loading…";

        RefreshButton.IsEnabled = !isLoading;
    }

    private void ShowErrorMessage(string message)
    {
        _logger.LogError("Activity log error: {Message}", message);

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

    private static (string background, string foreground) ResolveTypeColors(ActivityType type) => type switch
    {
        ActivityType.User => ("#E8F5E9", "#2E7D32"),
        ActivityType.System => ("#E3F2FD", "#1565C0"),
        ActivityType.Cron => ("#FFF8E1", "#F57F17"),
        _ => ("#EDE7DE", "#6D4C41")
    };
}
