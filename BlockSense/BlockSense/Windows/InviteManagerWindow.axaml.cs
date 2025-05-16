using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BlockSense.Api;
using BlockSense.Models;
using BlockSense.Models.User;
using BlockSense.Utilities;
using BlockSense.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Management;
using System.Threading.Tasks;

namespace BlockSense;

public partial class InviteManagerWindow : Window
{
    private readonly ApiClient _apiClient;
    private readonly MainWindow _mainWindow;
    private List<InviteInfoModel> _invites;
    private Border? _currentExpandedBorder = null;

    public InviteManagerWindow(ApiClient apiclient, MainWindow mainWindow, List<InviteInfoModel> inviteInfoModel)
    {
        _apiClient = apiclient;
        _mainWindow = mainWindow;
        _invites = inviteInfoModel;
        
        InitializeInviteCodes();
        InitializeComponent();

        InviteManagerContainer.PointerPressed += (sender, e) =>
        {
            if (e.Source != SearchBox && SearchBox.IsFocused)
            {
                var focused = TopLevel.GetTopLevel(this)?.FocusManager;
                if (focused is not null)
                {
                    focused.ClearFocus();
                }
            }
        };

        _mainWindow.Closing += (s, e) => this.Close();
    }

    private async void InitializeInviteCodes()
    {
        _invites.Clear();
        _invites = await _apiClient.FetchInviteInfo();

        DisplayInvites(_invites);
    }

    private void DisplayInvites(List<InviteInfoModel> invitesToDisplay)
    {
        // Clear existing Grid content
        InviteCodesGrid.Children.Clear();
        InviteCodesGrid.RowDefinitions.Clear();
        _currentExpandedBorder = null;

        for (int iRow = 0; iRow < invitesToDisplay.Count; iRow++)
        {
            InviteInfoModel invite = invitesToDisplay[iRow];

            if (invite is null || invite.InvitationCode is null || invite.InvitedUser is null)
                return;

            // Create row border
            Border rowBorder = new Border
            {
                Classes = { "RowBorder" }
            };
            Grid.SetRow(rowBorder, iRow);
            Grid.SetColumn(rowBorder, 0);
            InviteCodesGrid.Children.Add(rowBorder);

            // Create creation date text block
            TextBlock creationTextBlock = new TextBlock
            {
                Text = invite.CreationDate,
                Classes = { "CreationTextBlock" }
            };
            Grid.SetRow(creationTextBlock, iRow);
            Grid.SetColumn(creationTextBlock, 0);
            InviteCodesGrid.Children.Add(creationTextBlock);

            // Create expiration date text block
            TextBlock expirationTextBlock = new TextBlock
            {
                Text = invite.ExpirationDate,
                Classes = { "ExpirationTextBlock" }
            };
            Grid.SetRow(expirationTextBlock, iRow);
            Grid.SetColumn(expirationTextBlock, 1);
            InviteCodesGrid.Children.Add(expirationTextBlock);

            // Create invite code border and text block
            Border inviteCodeBorder = new Border
            {
                Classes = { "InviteCodeBorder" },
            };
            inviteCodeBorder.PointerPressed += ExpandInviteCode;
            Grid.SetRow(inviteCodeBorder, iRow);
            Grid.SetColumn(inviteCodeBorder, 2);

            TextBlock inviteCodeTextBlock = new TextBlock
            {
                Text = "Click here to Expand",
                Classes = { "InviteCodeTextBlock" },
            };
            inviteCodeBorder.Child = inviteCodeTextBlock;
            InviteCodesGrid.Children.Add(inviteCodeBorder);

            // Create invited user text block with appropriate styling
            TextBlock userTextBlock = new TextBlock
            {
                Text = InputHelper.Check(invite.InvitedUser) ? invite.InvitedUser : "(not used)",
                Classes = { InputHelper.Check(invite.InvitedUser) ? "InvitedUser" : "NotInvitedUser" }
            };
            Grid.SetRow(userTextBlock, iRow);
            Grid.SetColumn(userTextBlock, 3);
            InviteCodesGrid.Children.Add(userTextBlock);

            // Create status border and text block
            Border statusBorder = new Border();
            switch (invite.Status)
            {
                case "active":
                    statusBorder.Classes.Add("ActiveStatusBorder");
                    break;
                case "expired":
                    statusBorder.Classes.Add("ExpiredStatusBorder");
                    break;
                case "revoked":
                    statusBorder.Classes.Add("RevokedStatusBorder");
                    break;
                default:
                    statusBorder.Classes.Add("ActiveStatusBorder");
                    break;
            }
            Grid.SetRow(statusBorder, iRow);
            Grid.SetColumn(statusBorder, 4);

            TextBlock statusTextBlock = new TextBlock
            {
                Text = invite.Status,
                Classes = { "StatusTextBlock" }
            };
            statusBorder.Child = statusTextBlock;
            InviteCodesGrid.Children.Add(statusBorder);
        }

        // Add row definitions
        for (int i = 0; i < Math.Max(invitesToDisplay.Count, 1); i++)
        {
            InviteCodesGrid.RowDefinitions.Add(new RowDefinition(GridLength.Parse("40")));
        }
    }

    private void InviteFinder(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchBox.Text?.Trim().ToLower() ?? string.Empty;
        List<InviteInfoModel> filteredInviteCodes = new();

        if (!InputHelper.Check(searchText))
        {
            // Show all invites if search is empty
            DisplayInvites(_invites);
            return;
        }

        foreach (InviteInfoModel invite in _invites)
        {
            if (invite.InvitationCode is null || invite.InvitedUser is null || invite.CreationDate is null || invite.ExpirationDate is null || invite.Status is null)
                return;

            if (invite.InvitationCode.ToLower().Contains(searchText) || invite.CreationDate.ToLower().Contains(searchText) ||
                invite.ExpirationDate.ToLower().Contains(searchText) || invite.InvitedUser.ToLower().Contains(searchText) || invite.Status.ToLower().Contains(searchText))
                filteredInviteCodes.Add(invite);
        }

        DisplayInvites(filteredInviteCodes);
    }

    private void DragWindow(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && VisualRoot is Window window)
            window.BeginMoveDrag(e);
    }

    private async void CloseWindowClick(object sender, RoutedEventArgs e)
    {
        // Fade out animation on Window close
        await AnimationManager.FadeOutAnimation.RunAsync(this);
        // Close the window
        this.Close();
    }

    public void ExpandInviteCode(object? sender, PointerPressedEventArgs e)
    {
        // Get the TextBlock inside the border
        if (sender is not Border clickedBorder) return;
        if (clickedBorder.Child is not TextBlock textBlock) return;

        // If clicking the already expanded border, collapse it
        if (_currentExpandedBorder == clickedBorder)
        {
            CollapseCurrentExpandedBorder();
            return;
        }

        // First collapse any previously expanded border
        CollapseCurrentExpandedBorder();

        textBlock.Text = _invites[Grid.GetRow(clickedBorder)].InvitationCode;

        // Set the border to span all columns
        Grid.SetColumn(clickedBorder, 0);
        Grid.SetColumnSpan(clickedBorder, 5);

        // Add the expanded styles
        clickedBorder.Classes.Add("InviteCodeExpandedBorder");
        textBlock.Classes.Add("InviteCodeExpandedTextBlock");
        _currentExpandedBorder = clickedBorder;

        // Set higher Z-index to ensure it appears on top
        clickedBorder.ZIndex = 100;
    }

    private void CollapseCurrentExpandedBorder()
    {
        if (_currentExpandedBorder != null)
        {
            // Get the TextBlock inside the border
            var textBlock = _currentExpandedBorder.Child as TextBlock;
            if (textBlock != null)
            {
                // Reset text to the default value
                textBlock.Text = "Click here to Expand";

                // Remove expanded styles
                textBlock.Classes.Remove("InviteCodeExpandedTextBlock");
            }

            Grid.SetColumn(_currentExpandedBorder, 2);
            Grid.SetColumnSpan(_currentExpandedBorder, 1);

            // Remove expanded styles
            _currentExpandedBorder.Classes.Remove("InviteCodeExpandedBorder");
            if (textBlock != null)
            {
                textBlock.Classes.Remove("InviteCodeExpandedTextBlock");
            }

            // Reset Z-index
            _currentExpandedBorder.ZIndex = 0;
            _currentExpandedBorder = null;
        }
    }
}