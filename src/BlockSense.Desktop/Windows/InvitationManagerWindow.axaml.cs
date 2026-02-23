using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using BlockSense.Contracts.DTOs.Invitation;
using BlockSense.Contracts.Enums;
using BlockSense.Desktop.Providers.Interfaces;
using BlockSense.Desktop.Utilities.Formatting;
using BlockSense.Desktop.Utilities.UIComponents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockSense.Desktop;

public partial class InvitationManagerWindow : Window
{
    private readonly ICurrentUserProvider _currentUserProvider;

    private Border? _currentExpandedBorder;

    public InvitationManagerWindow()
    {
        _currentUserProvider = App.ServiceProvider.GetRequiredService<ICurrentUserProvider>()
            ?? throw new ArgumentNullException(nameof(ICurrentUserProvider));

        InitializeComponent();

        DisplayInvites(_currentUserProvider.Invitations);

        DraggableArea.PointerPressed += DraggableAreaPointerPressed;
        CloseWindowButton.Click += CloseWindowClick;

        InvitationSearchTextBox.TextChanged += InvitationSearchTextBoxTextChanged;

        MainWindow.Instance.Closing += (s, e) => this.Close();
    }

    private void DisplayInvites(IReadOnlyList<InvitationDto> invites)
    {
        InviteCodesGrid.Children.Clear();
        InviteCodesGrid.RowDefinitions.Clear();
        _currentExpandedBorder = null;

        if (invites == null || invites.Count == 0)
            return;

        for (int row = 0; row < invites.Count; row++)
        {
            var invite = invites[row];

            InviteCodesGrid.RowDefinitions.Add(new RowDefinition(new GridLength(40)));

            AddRowBackground(row);
            AddCreationDate(invite, row);
            AddExpirationDate(invite, row);
            AddInviteCode(invite, row);
            AddInvitedUser(invite, row);
            AddStatus(invite, row);
        }
    }

    private async void CloseWindowClick(object? sender, RoutedEventArgs e)
    {
        // Fade out animation on Window close
        await Animations.FadeOutAnimation.RunAsync(this);

        // Close the window
        this.Hide();
    }

    private void DraggableAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
        {
            window.BeginMoveDrag(e);
        }
    }

    private void InvitationSearchTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = InvitationSearchTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(searchText))
        {
            DisplayInvites(_currentUserProvider.Invitations);
            return;
        }

        searchText = searchText.ToLowerInvariant();

        var filteredInvites = _currentUserProvider.Invitations
            .Where(invite => InviteMatchesSearch(invite, searchText))
            .ToList();

        DisplayInvites(filteredInvites);
    }

    private void ExpandInviteCode(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border ||
            border.Child is not TextBlock text ||
            border.Tag is not InvitationDto invite)
        {
            return;
        }

        if (_currentExpandedBorder == border)
        {
            CollapseCurrentExpandedBorder();
            return;
        }

        CollapseCurrentExpandedBorder();

        border.Opacity = 0;

        Grid.SetColumn(border, 0);
        Grid.SetColumnSpan(border, 5);
        border.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        border.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        border.ZIndex = 10;

        text.Text = invite.InvitationCode;
        text.Classes.Add("InviteCodeExpandedTextBlock");

        border.Classes.Add("InviteCodeExpandedBorder");

        border.Opacity = 1;

        _currentExpandedBorder = border;
    }

    private void CollapseCurrentExpandedBorder()
    {
        if (_currentExpandedBorder?.Child is not TextBlock text)
        {
            return;
        }

        var border = _currentExpandedBorder;

        border.Opacity = 0;

        border.Classes.Remove("InviteCodeExpandedBorder");

        Grid.SetColumn(_currentExpandedBorder, 2);
        Grid.SetColumnSpan(_currentExpandedBorder, 1);
        _currentExpandedBorder.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        _currentExpandedBorder.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        _currentExpandedBorder.ZIndex = 0;

        text.Text = "Click here to Expand";
        text.Classes.Remove("InviteCodeExpandedTextBlock");

        border.Opacity = 1;

        _currentExpandedBorder = null;
    }

    private void AddRowBackground(int row)
    {
        var border = new Border { Classes = { "InvitationBorder" } };
        Grid.SetRow(border, row);
        Grid.SetColumnSpan(border, 5);
        InviteCodesGrid.Children.Add(border);
    }

    private void AddCreationDate(InvitationDto invitation, int row)
    {
        var text = new TextBlock
        {
            Text = DateTimeFormatter.ToOrdinalDate(invitation.CreatedAt),
            Classes = { "CreationTextBlock" }
        };

        Grid.SetRow(text, row);
        Grid.SetColumn(text, 0);
        InviteCodesGrid.Children.Add(text);
    }

    private void AddExpirationDate(InvitationDto invitation, int row)
    {
        var text = new TextBlock
        {
            Text = DateTimeFormatter.ToOrdinalDate(invitation.ExpiresAt),
            Classes = { "ExpirationTextBlock" }
        };
        Grid.SetRow(text, row);
        Grid.SetColumn(text, 1);
        InviteCodesGrid.Children.Add(text);
    }

    private void AddInviteCode(InvitationDto invitation, int row)
    {
        var border = new Border
        {
            Classes = { "ExpandInvitationCodeBorder" },
            Tag = invitation,
        };

        var text = new TextBlock
        {
            Text = "Click here to Expand",
            Classes = { "InvitationCodeTextBlock" }
        };

        border.Child = text;
        border.PointerPressed += ExpandInviteCode;

        Grid.SetRow(border, row);
        Grid.SetColumn(border, 2);

        InviteCodesGrid.Children.Add(border);
    }

    private void AddInvitedUser(InvitationDto invitation, int row)
    {
        bool hasUser = !string.IsNullOrWhiteSpace(invitation.UsedBy);

        var text = new TextBlock
        {
            Text = hasUser ? invitation.UsedBy : "( not used )",
            Classes = { "InvitedUser" }
        };

        Grid.SetRow(text, row);
        Grid.SetColumn(text, 3);
        InviteCodesGrid.Children.Add(text);
    }

    private void AddStatus(InvitationDto invitation, int row)
    {
        var border = new Border
        {
            Classes = { GetStatusClass(invitation.Status) }
        };

        var text = new TextBlock
        {
            Text = invitation.Status.ToString(),
            Classes = { "StatusTextBlock" }
        };

        border.Child = text;

        Grid.SetRow(border, row);
        Grid.SetColumn(border, 4);

        InviteCodesGrid.Children.Add(border);
    }

    private static bool InviteMatchesSearch(InvitationDto invite, string searchText)
    {
        if (invite is null || string.IsNullOrWhiteSpace(searchText))
            return false;

        searchText = searchText.Trim();

        return
            Contains(invite.InvitationCode, searchText) ||
            Contains(invite.UsedBy, searchText) ||
            Contains(invite.Status.ToString(), searchText) ||
            DateMatches(invite.CreatedAt, searchText) ||
            DateMatches(invite.ExpiresAt, searchText);
    }

    private static bool Contains(string? source, string searchText)
    {
        return !string.IsNullOrWhiteSpace(source) &&
            source.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private static bool DateMatches(DateTime date, string searchText)
    {
        return $"Q{((date.Month - 1) / 3) + 1}".Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            date.ToString("yyyy-MM-dd").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            date.ToString("dd.MM.yyyy").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            date.ToString("MM/dd/yyyy").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            date.ToString("MMMM").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            date.ToString("MMM").Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            date.DayOfYear.ToString().Contains(searchText) ||
            date.DayOfWeek.ToString().Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            date.Year.ToString().Contains(searchText) ||
            date.Month.ToString().Contains(searchText) ||
            date.Day.ToString().Contains(searchText);
    }

    private static string GetStatusClass(InvitationStatus invitationStatus) =>
        invitationStatus switch
        {
            InvitationStatus.Active => "ActiveStatusBorder",
            InvitationStatus.Used => "UsedStatusBorder",
            InvitationStatus.Expired => "ExpiredStatusBorder",
            InvitationStatus.Revoked => "RevokedStatusBorder",
            _ => "ActiveStatusBorder"
        };

}