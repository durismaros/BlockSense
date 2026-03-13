using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.Formatting;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockSense.Desktop;

/// <summary>
/// A floating window that lists all invitations belonging to the current user
/// and allows the user to search and inspect individual invitation codes.
/// </summary>
public partial class InvitationManagerWindow : Window
{
    private const string CollapsedCodeLabel = "Click here to Expand";

    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<InvitationManagerWindow> _logger;

    /// <summary>The code cell border that is currently in its expanded state, or <see langword="null"/> when none is expanded.</summary>
    private Border? _expandedCodeBorder;

    /// <summary>
    /// Initialises a new instance of <see cref="InvitationManagerWindow"/>,
    /// resolves dependencies, wires up event handlers, and performs the initial
    /// invitation display.
    /// </summary>
    public InvitationManagerWindow()
    {
        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>()
            ?? throw new ArgumentNullException(nameof(ICurrentUserProvider));

        _logger = App.ServiceProvider.GetRequiredService<ILogger<InvitationManagerWindow>>()
            ?? throw new ArgumentNullException(nameof(ILogger<InvitationManagerWindow>));

        InitializeComponent();

        DisplayInvitations(_currentUserProvider.Invitations);

        DraggableArea.PointerPressed += OnDraggableAreaPointerPressed;
        CloseWindowButton.Click += OnCloseWindowButtonClick;
        InvitationSearchTextBox.TextChanged += OnInvitationSearchTextChanged;

        MainWindow.Instance.Closing += (_, _) => Close();
    }

    /// <summary>
    /// Fades the window out and hides it when the close button is clicked.
    /// </summary>
    private async void OnCloseWindowButtonClick(object? sender, RoutedEventArgs e)
    {
        await Animations.FadeOutAnimation.RunAsync(this);
        Hide();
    }

    /// <summary>
    /// Begins a native window drag operation when the user presses the left
    /// mouse button on the drag area.
    /// </summary>
    private void OnDraggableAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        bool isLeftButton = e.GetCurrentPoint(this).Properties.IsLeftButtonPressed;

        if (isLeftButton && VisualRoot is Window window)
        {
            window.BeginMoveDrag(e);
        }
    }

    /// <summary>
    /// Filters the displayed invitations whenever the search query changes.
    /// Resets to the full list when the query is empty.
    /// </summary>
    private void OnInvitationSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var query = InvitationSearchTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            DisplayInvitations(_currentUserProvider.Invitations);
            return;
        }

        var normalizedQuery = query.ToLowerInvariant();

        var filtered = _currentUserProvider.Invitations
            .Where(invitation => InvitationMatchesSearch(invitation, normalizedQuery))
            .ToList();

        DisplayInvitations(filtered);
    }

    /// <summary>
    /// Toggles the expansion state of an invitation-code cell when it is clicked.
    /// Collapses any previously expanded cell before expanding the new one.
    /// </summary>
    private void OnCodeCellPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border ||
            border.Child is not TextBlock label ||
            border.Tag is not InvitationDto invitation)
        {
            return;
        }

        bool isSameBorder = _expandedCodeBorder == border;

        CollapseExpandedCodeCell();

        if (!isSameBorder)
        {
            ExpandCodeCell(border, label, invitation);
        }
    }

    /// <summary>
    /// Clears the invitation grid and rebuilds it from the supplied
    /// <paramref name="invitations"/> collection.
    /// </summary>
    /// <param name="invitations">The invitations to render.</param>
    private void DisplayInvitations(IEnumerable<InvitationDto> invitations)
    {
        InviteCodesGrid.Children.Clear();
        InviteCodesGrid.RowDefinitions.Clear();
        _expandedCodeBorder = null;

        var invitationList = invitations?.ToList();

        if (invitationList is null || invitationList.Count == 0)
        {
            _logger.LogDebug("No invitations to display.");
            return;
        }

        for (int rowIndex = 0; rowIndex < invitationList.Count; rowIndex++)
        {
            var invitation = invitationList[rowIndex];

            InviteCodesGrid.RowDefinitions.Add(new RowDefinition(new GridLength(40)));

            AddRowBackground(rowIndex);
            AddCreatedDateCell(invitation, rowIndex);
            AddExpiredDateCell(invitation, rowIndex);
            AddCodeCell(invitation, rowIndex);
            AddRedeemedByCell(invitation, rowIndex);
            AddStatusCell(invitation, rowIndex);
        }

        _logger.LogDebug("Displayed {Count} invitation(s).", invitationList.Count);
    }

    /// <summary>
    /// Adds the full-width dark pill that serves as the row background for the
    /// given <paramref name="rowIndex"/>.
    /// </summary>
    /// <param name="rowIndex">Zero-based row index in the invitation grid.</param>
    private void AddRowBackground(int rowIndex)
    {
        var background = new Border { Classes = { "InvitationRow" } };

        Grid.SetRow(background, rowIndex);
        Grid.SetColumnSpan(background, 5);

        InviteCodesGrid.Children.Add(background);
    }

    /// <summary>
    /// Adds the formatted creation-date label to column 0 of the given row.
    /// </summary>
    /// <param name="invitation">The invitation whose creation date is displayed.</param>
    /// <param name="rowIndex">Zero-based row index in the invitation grid.</param>
    private void AddCreatedDateCell(InvitationDto invitation, int rowIndex)
    {
        var label = new TextBlock
        {
            Text = DateTimeFormatter.ToOrdinalDate(invitation.CreatedAt),
            Classes = { "InvitationCreatedDate" }
        };

        Grid.SetRow(label, rowIndex);
        Grid.SetColumn(label, 0);

        InviteCodesGrid.Children.Add(label);
    }

    /// <summary>
    /// Adds the formatted expiry-date label to column 1 of the given row.
    /// </summary>
    /// <param name="invitation">The invitation whose expiry date is displayed.</param>
    /// <param name="rowIndex">Zero-based row index in the invitation grid.</param>
    private void AddExpiredDateCell(InvitationDto invitation, int rowIndex)
    {
        var label = new TextBlock
        {
            Text = DateTimeFormatter.ToOrdinalDate(invitation.ExpiresAt),
            Classes = { "InvitationExpiresDate" }
        };

        Grid.SetRow(label, rowIndex);
        Grid.SetColumn(label, 1);

        InviteCodesGrid.Children.Add(label);
    }

    /// <summary>
    /// Adds a collapsible code pill to column 2 of the given row.
    /// The pill expands to reveal the full invitation code on click.
    /// </summary>
    /// <param name="invitation">The invitation whose code is displayed.</param>
    /// <param name="rowIndex">Zero-based row index in the invitation grid.</param>
    private void AddCodeCell(InvitationDto invitation, int rowIndex)
    {
        var label = new TextBlock
        {
            Text = CollapsedCodeLabel,
            Classes = { "InvitationCodeCollapsedLabel" }
        };

        var pill = new Border
        {
            Classes = { "InvitationCodeCollapsed" },
            Tag = invitation,
            Child = label
        };

        pill.PointerPressed += OnCodeCellPointerPressed;

        Grid.SetRow(pill, rowIndex);
        Grid.SetColumn(pill, 2);

        InviteCodesGrid.Children.Add(pill);
    }

    /// <summary>
    /// Adds the "redeemed by" label to column 3 of the given row.
    /// Falls back to "(not used)" when the invitation has not been redeemed.
    /// </summary>
    /// <param name="invitation">The invitation whose redeemer is displayed.</param>
    /// <param name="rowIndex">Zero-based row index in the invitation grid.</param>
    private void AddRedeemedByCell(InvitationDto invitation, int rowIndex)
    {
        bool isRedeemed = !string.IsNullOrWhiteSpace(invitation.RedeemedBy);

        var label = new TextBlock
        {
            Text = isRedeemed ? invitation.RedeemedBy : "( not used )",
            Classes = { "InvitationRedeemedBy" }
        };

        Grid.SetRow(label, rowIndex);
        Grid.SetColumn(label, 3);

        InviteCodesGrid.Children.Add(label);
    }

    /// <summary>
    /// Adds a colour-coded status badge to column 4 of the given row.
    /// </summary>
    /// <param name="invitation">The invitation whose status is displayed.</param>
    /// <param name="rowIndex">Zero-based row index in the invitation grid.</param>
    private void AddStatusCell(InvitationDto invitation, int rowIndex)
    {
        var label = new TextBlock
        {
            Text = invitation.Status.ToString(),
            Classes = { "InvitationStatusLabel" }
        };

        var badge = new Border
        {
            Classes = { ResolveStatusBadgeClass(invitation.Status) },
            Child = label
        };

        Grid.SetRow(badge, rowIndex);
        Grid.SetColumn(badge, 4);

        InviteCodesGrid.Children.Add(badge);
    }

    /// <summary>
    /// Expands the given code cell to span all columns and reveals the raw
    /// invitation code.
    /// </summary>
    /// <param name="pill">The border element acting as the code pill.</param>
    /// <param name="label">The text block inside the pill.</param>
    /// <param name="invitation">The invitation whose code is revealed.</param>
    private void ExpandCodeCell(Border pill, TextBlock label, InvitationDto invitation)
    {
        pill.Opacity = 0;

        Grid.SetColumn(pill, 0);
        Grid.SetColumnSpan(pill, 5);
        pill.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        pill.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        pill.ZIndex = 10;

        label.Text = invitation.Code;
        label.Classes.Remove("InvitationCodeCollapsedLabel");
        label.Classes.Add("InvitationCodeExpandedLabel");

        pill.Classes.Remove("InvitationCodeCollapsed");
        pill.Classes.Add("InvitationCodeExpanded");

        pill.Opacity = 1;

        _expandedCodeBorder = pill;
    }

    /// <summary>
    /// Collapses the currently expanded code cell back to its default state.
    /// Does nothing when no cell is expanded.
    /// </summary>
    private void CollapseExpandedCodeCell()
    {
        if (_expandedCodeBorder is null || _expandedCodeBorder.Child is not TextBlock label)
        {
            return;
        }

        var pill = _expandedCodeBorder;
        pill.Opacity = 0;

        pill.Classes.Remove("InvitationCodeExpanded");
        pill.Classes.Add("InvitationCodeCollapsed");

        Grid.SetColumn(pill, 2);
        Grid.SetColumnSpan(pill, 1);
        pill.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        pill.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        pill.ZIndex = 0;

        label.Text = CollapsedCodeLabel;
        label.Classes.Remove("InvitationCodeExpandedLabel");
        label.Classes.Add("InvitationCodeCollapsedLabel");

        pill.Opacity = 1;

        _expandedCodeBorder = null;
    }

    /// <summary>
    /// Returns <see langword="true"/> when any searchable field of
    /// <paramref name="invitation"/> contains <paramref name="normalizedQuery"/>.
    /// </summary>
    /// <param name="invitation">The invitation to test.</param>
    /// <param name="normalizedQuery">Lower-cased search query.</param>
    /// <returns><see langword="true"/> if the invitation matches the query.</returns>
    private static bool InvitationMatchesSearch(InvitationDto invitation, string normalizedQuery)
    {
        return
            ContainsQuery(invitation.Code, normalizedQuery) ||
            ContainsQuery(invitation.RedeemedBy, normalizedQuery) ||
            ContainsQuery(invitation.Status.ToString(), normalizedQuery) ||
            DateContainsQuery(invitation.CreatedAt, normalizedQuery) ||
            DateContainsQuery(invitation.ExpiresAt, normalizedQuery);
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="source"/> contains
    /// <paramref name="query"/> using a case-insensitive ordinal comparison.
    /// </summary>
    /// <param name="source">The string to search within. May be <see langword="null"/>.</param>
    /// <param name="query">The search term.</param>
    /// <returns><see langword="true"/> if <paramref name="source"/> contains <paramref name="query"/>.</returns>
    private static bool ContainsQuery(string? source, string query)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns <see langword="true"/> when any of the common date representations
    /// of <paramref name="date"/> contain <paramref name="query"/>.
    /// </summary>
    /// <param name="date">The date to test against.</param>
    /// <param name="query">The search term.</param>
    /// <returns><see langword="true"/> if the date matches the query in any supported format.</returns>
    private static bool DateContainsQuery(DateTime date, string query)
    {
        string quarter = $"Q{((date.Month - 1) / 3) + 1}";

        return quarter.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               date.ToString("yyyy-MM-dd").Contains(query, StringComparison.OrdinalIgnoreCase) ||
               date.ToString("dd.MM.yyyy").Contains(query, StringComparison.OrdinalIgnoreCase) ||
               date.ToString("MM/dd/yyyy").Contains(query, StringComparison.OrdinalIgnoreCase) ||
               date.ToString("MMMM").Contains(query, StringComparison.OrdinalIgnoreCase) ||
               date.ToString("MMM").Contains(query, StringComparison.OrdinalIgnoreCase) ||
               date.DayOfWeek.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
               date.DayOfYear.ToString().Contains(query) ||
               date.Year.ToString().Contains(query) ||
               date.Month.ToString().Contains(query) ||
               date.Day.ToString().Contains(query);
    }

    /// <summary>
    /// Maps an <see cref="InvitationStatus"/> value to the corresponding
    /// AXAML style class name for the status badge border.
    /// </summary>
    /// <param name="status">The invitation status to resolve.</param>
    /// <returns>The AXAML class name for the matching status badge style.</returns>
    private static string ResolveStatusBadgeClass(InvitationStatus status) =>
        status switch
        {
            InvitationStatus.Active => "InvitationStatusActive",
            InvitationStatus.Used => "InvitationStatusUsed",
            InvitationStatus.Expired => "InvitationStatusExpired",
            InvitationStatus.Revoked => "InvitationStatusRevoked",
            _ => "InvitationStatusActive"
        };
}
