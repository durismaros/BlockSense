using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using BlockSense.Contracts.DTOs.User;
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

    private List<ActivityLogDto> _currentPage = [];
    private ulong _newestKnownId = 0;
    private ulong _totalCount = 0;
    private int _currentPageNum = 1;
    private int _totalPages = 1;

    private const int PageSize = 20;

    private CancellationTokenSource _cts = new();

    public ActivityLogView()
    {
        _activityLogService = App.ServiceProvider.GetRequiredService<IActivityLogService>();
        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>();

        InitializeComponent();
        LoadAsync();
        WireEvents();
    }

    public async void LoadAsync()
    {
        _currentPageNum = 1;
        await FetchAndRenderAsync();
    }

    private void WireEvents()
    {
        CloseActivityLogButton.Click += async (_, _) => await RequestClose();

        PrevPageButton.Click += async (_, _) =>
        {
            if (_currentPageNum > 1) { _currentPageNum--; await FetchAndRenderAsync(); }
        };

        NextPageButton.Click += async (_, _) =>
        {
            if (_currentPageNum < _totalPages) { _currentPageNum++; await FetchAndRenderAsync(); }
        };

        RefreshButton.Click += async (_, _) => await FetchAfterIdAsync();
    }

    private async Task FetchAndRenderAsync()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        SetLoadingState(true);

        try
        {
            var result = await _activityLogService.GetPageAsync(
                page: _currentPageNum,
                pageSize: PageSize,
                cancellationToken: token);

            if (token.IsCancellationRequested) return;

            if (result is null)
            {
                ShowError("Failed to load activity logs.");
                return;
            }

            _currentPage = [.. result.Entries];
            _totalPages = result.TotalPages;
            _totalCount = result.TotalCount;

            if (_currentPageNum == 1 && _currentPage.Count > 0)
            {
                var topId = _currentPage.Max(l => l.Id);

                if (topId > _newestKnownId)
                    _newestKnownId = topId;

                // Keep dashboard in sync
                _currentUserProvider.SetRecentActivity(
                    _currentPage.Take(3).ToList().AsReadOnly());
            }

            RenderPage();
            UpdatePager();

            LogCountTextBlock.Text = $"{_totalCount:N0} {(_totalCount == 1 ? "entry" : "entries")}";
            RefreshBadge.IsVisible = false;
        }
        catch (OperationCanceledException) { /* superseded */ }
        finally
        {

            SetLoadingState(false);
            RefreshButton.IsEnabled = true;
        }
    }

    private async Task FetchAfterIdAsync()
    {
        if (_newestKnownId == 0)
        {
            await FetchAndRenderAsync();
            return;
        }

        RefreshButton.IsEnabled = false;

        var newer = await _activityLogService.GetLatestAsync(_newestKnownId);

        if (newer.Count == 0)
        {
            RefreshButton.IsEnabled = true;
            return;
        }

        _currentPageNum = 1;
        await FetchAndRenderAsync();
    }

    private void RenderPage()
    {
        LogRowsPanel.Children.Clear();

        if (_currentPage.Count == 0)
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
            return;
        }

        foreach (var log in _currentPage)
            LogRowsPanel.Children.Add(CreateRow(log));
    }

    private void UpdatePager()
    {
        PrevPageButton.IsEnabled = _currentPageNum > 1;
        NextPageButton.IsEnabled = _currentPageNum < _totalPages;

        PagerInfoTextBlock.Text = $"Page {_currentPageNum} of {_totalPages}";

        PageNumbersPanel.Children.Clear();

        int start = Math.Max(1, _currentPageNum - 2);
        int end = Math.Min(_totalPages, start + 4);
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
                    Foreground = new SolidColorBrush(
                        Color.Parse(isCurrent ? "#F5E1C5" : "#6D4C41"))
                }
            };

            btn.Click += async (_, _) =>
            {
                _currentPageNum = page;
                RenderPage();
            };

            PageNumbersPanel.Children.Add(btn);
        }
    }

    private void SetLoadingState(bool loading)
    {
        if (loading)
        {
            LogCountTextBlock.Text = "Loading...";
        }

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
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        });
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

        var (bgHex, fgHex) = TypeColors(log.Type.ToString());
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

    private static (string bg, string fg) TypeColors(string type) => type switch
    {
        "user" => ("#E8F5E9", "#2E7D32"),
        "system" => ("#E3F2FD", "#1565C0"),
        "cron" => ("#FFF8E1", "#F57F17"),
        _ => ("#EDE7DE", "#6D4C41")
    };

    public event Func<Task>? CloseRequested;

    private async Task RequestClose()
    {
        if (CloseRequested is not null)
            await CloseRequested.Invoke();
    }
}