using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using BlockSense.Contracts.DTOs.User;
using BlockSense.Desktop.Utilities.Formatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockSense.Desktop;

public partial class ActivityLogView : UserControl
{
    private List<ActivityLogDto> _allLogs = [];
    private List<ActivityLogDto> _filteredLogs = [];

    private string _activeTypeFilter = "all";
    private string _searchQuery = string.Empty;

    private int _currentPage = 1;
    private const int PageSize = 20;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(_filteredLogs.Count / (double)PageSize));
    public ActivityLogView()
    {
        InitializeComponent();

        WireEvents();

        // Seed sample data so the UI is immediately visible during development.
        LoadSampleData();
    }

    public async Task LoadAsync()
    {
        LogCountTextBlock.Text = "Loading…";
        LogRowsPanel.Children.Clear();

        ApplyFilters();
    }

    private void LoadSampleData()
    {
        var now = DateTime.UtcNow;

        _allLogs =
        [
            // ── User events ──────────────────────────────────────────
Make(1,  "user",   "device.auth.succeeded",              "Device authenticated",                     now.AddMinutes(-8)),
Make(2,  "user",   "profile.password.changed",           "Password changed",                         now.AddHours(-2)),
Make(3,  "user",   "auth.2fa.enabled",                   "Two-factor authentication enabled",        now.AddHours(-5)),
Make(4,  "user",   "profile.picture.changed",            "Profile picture changed",                  now.AddHours(-14)),
Make(5,  "user",   "device.auth.succeeded",              "Device authenticated",                     now.AddDays(-1)),
Make(6,  "user",   "device.revoked",                     "Device revoked",                           now.AddDays(-1).AddHours(-3)),
Make(7,  "user",   "invitation.code.redeemed",           "Invitation code redeemed",                 now.AddDays(-2)),
Make(8,  "user",   "device.auth.succeeded",              "Device authenticated",                     now.AddDays(-2).AddMinutes(-6)),
Make(9,  "user",   "profile.email.changed",              "Email address changed",                    now.AddDays(-4)),
Make(10, "user",   "invitation.code.generated",          "Invitation code generated",                now.AddDays(-6)),
Make(11, "user",   "auth.2fa.disabled",                  "Two-factor authentication disabled",       now.AddDays(-9)),
Make(12, "user",   "invitation.code.generated",          "Invitation code generated",                now.AddDays(-10)),
Make(13, "user",   "profile.username.changed",           "Username changed",                         now.AddDays(-12)),
Make(14, "user",   "device.auth.succeeded",              "Device authenticated",                     now.AddDays(-14)),
Make(15, "user",   "device.revoked",                     "Device revoked",                           now.AddDays(-15)),
Make(16, "user",   "device.auth.succeeded",              "Device authenticated",                     now.AddDays(-21)),

// ── System events ────────────────────────────────────────
Make(17, "system", "user.role.updated",                  "User role updated to Administrator",       now.AddDays(-11)),
Make(18, "system", "auth.2fa.backup.generated",          "Two-factor backup codes generated",        now.AddDays(-5)),
Make(19, "system", "user.registered",                    "New user registered",                      now.AddDays(-3)),
Make(20, "system", "user.restored",                      "User account restored",                    now.AddDays(-13)),

// ── Cron events ──────────────────────────────────────────
Make(21, "cron",   "device.revoked",                     "Expired device credentials revoked",       now.AddHours(-3)),
Make(22, "cron",   "invitation.code.generated",          "Scheduled invitation codes generated",     now.AddDays(-1).AddHours(-3)),
Make(23, "cron",   "device.revoked",                     "Expired device credentials revoked",       now.AddDays(-2).AddHours(-3)),
Make(24, "cron",   "auth.2fa.backup.generated",          "Scheduled 2FA backup codes generated",     now.AddDays(-3).AddHours(-3)),
Make(25, "cron",   "device.revoked",                     "Expired device credentials revoked",       now.AddDays(-4).AddHours(-3))
        ];

        ApplyFilters();

        // ── Local factory ─────────────────────────────────────────────
        static ActivityLogDto Make(
            ulong id, string type, string action, string message, DateTime occurredAt) =>
            new()
            {
                Id = id,
                Type = type,
                UserId = 1,
                Action = action,
                ActivityMessage = message,
                OccurredAt = occurredAt
            };
    }

    // ── Wiring ────────────────────────────────────────────────────────
    private void WireEvents()
    {
        // Close button
        CloseActivityLogButton.Click += async (_, _) => await RequestClose();

        // Search
        SearchBox.TextChanged += (_, _) =>
        {
            _searchQuery = SearchBox.Text ?? string.Empty;
            _currentPage = 1;
            ApplyFilters();
        };

        // Chip clicks
        ChipAll.PointerPressed += (_, _) => SetTypeFilter("all");
        ChipUser.PointerPressed += (_, _) => SetTypeFilter("user");
        ChipSystem.PointerPressed += (_, _) => SetTypeFilter("system");
        ChipCron.PointerPressed += (_, _) => SetTypeFilter("cron");

        // Pager
        PrevPageButton.Click += (_, _) => { if (_currentPage > 1) { _currentPage--; RenderPage(); } };
        NextPageButton.Click += (_, _) => { if (_currentPage < TotalPages) { _currentPage++; RenderPage(); } };
    }

    // ── Filter / paging ───────────────────────────────────────────────
    private void SetTypeFilter(string type)
    {
        _activeTypeFilter = type;
        _currentPage = 1;

        // Swap chip styles
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

        // Type filter
        if (_activeTypeFilter != "all")
            query = query.Where(l => l.Type == _activeTypeFilter);

        // Search filter (action code or human message)
        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            var q = _searchQuery.ToLowerInvariant();
            query = query.Where(l =>
                l.Action.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                l.ActivityMessage.Contains(q, StringComparison.OrdinalIgnoreCase));
        }

        _filteredLogs = [.. query.OrderByDescending(l => l.OccurredAt)];

        LogCountTextBlock.Text = $"{_filteredLogs.Count:N0} {(_filteredLogs.Count == 1 ? "entry" : "entries")}";

        RenderPage();
    }

    private void RenderPage()
    {
        LogRowsPanel.Children.Clear();

        var page = _filteredLogs
            .Skip((_currentPage - 1) * PageSize)
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

    // ── Row builder ───────────────────────────────────────────────────
    private static Control CreateRow(ActivityLogDto log)
    {
        var wrapper = new StackPanel { Spacing = 0 };

        // Divider
        wrapper.Children.Add(new Border { Classes = { "RowDivider" } });

        // Content grid: Date | Activity | Type
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("160 * 90"),
            Margin = new Avalonia.Thickness(0, 10)
        };

        // Date
        var dateBlock = new TextBlock
        {
            Classes = { "RowDate" },
            Text = DateTimeFormatter.ToOrdinalDate(log.OccurredAt)
        };
        Grid.SetColumn(dateBlock, 0);

        // Activity message
        var msgBlock = new TextBlock
        {
            Classes = { "RowAction" },
            Text = string.IsNullOrWhiteSpace(log.ActivityMessage)
                ? HumanizeAction(log.Action)
                : log.ActivityMessage,
            Margin = new Avalonia.Thickness(12, 0)
        };
        Grid.SetColumn(msgBlock, 1);

        // Type pill
        var (bgHex, fgHex) = TypeColors(log.Type);
        var pill = new Border
        {
            Classes = { "TypePill" },
            Background = new SolidColorBrush(Color.Parse(bgHex)),
            Child = new TextBlock
            {
                Classes = { "TypePillText" },
                Text = log.Type.ToUpperInvariant(),
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

    // ── Pager UI ──────────────────────────────────────────────────────
    private void UpdatePager()
    {
        PrevPageButton.IsEnabled = _currentPage > 1;
        NextPageButton.IsEnabled = _currentPage < TotalPages;

        PagerInfoTextBlock.Text = $"Page {_currentPage} of {TotalPages}";

        // Page number buttons (show up to 5 around current)
        PageNumbersPanel.Children.Clear();

        int start = Math.Max(1, _currentPage - 2);
        int end = Math.Min(TotalPages, start + 4);
        start = Math.Max(1, end - 4);

        for (int p = start; p <= end; p++)
        {
            int page = p; // capture
            bool isCurrent = page == _currentPage;

            var btn = new Button
            {
                Classes = { isCurrent ? "PagerBtnActive" : "PagerBtn" },
                Content = new TextBlock
                {
                    Text = page.ToString(),
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(
                        Color.Parse(isCurrent ? "#F5E1C5" : "#6D4C41"))
                }
            };

            btn.Click += (_, _) => { _currentPage = page; RenderPage(); };
            PageNumbersPanel.Children.Add(btn);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Converts a dot-namespaced action code like <c>profile.picture.changed</c>
    /// into a readable string like <c>Profile Picture Changed</c>.
    /// </summary>
    private static string HumanizeAction(string action)
        => System.Globalization.CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(action.Replace('.', ' ').Replace('_', ' '));

    /// <summary>Returns (background hex, foreground hex) for each actor type.</summary>
    private static (string bg, string fg) TypeColors(string type) => type switch
    {
        "user" => ("#E8F5E9", "#2E7D32"),
        "system" => ("#E3F2FD", "#1565C0"),
        "cron" => ("#FFF8E1", "#F57F17"),
        _ => ("#EDE7DE", "#6D4C41")
    };

    /// <summary>Raised when the overlay wants to be dismissed.</summary>
    public event Func<Task>? CloseRequested;

    private async Task RequestClose()
    {
        if (CloseRequested is not null)
            await CloseRequested.Invoke();
    }
}