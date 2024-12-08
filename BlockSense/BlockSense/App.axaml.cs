using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using BlockSense.ViewModels;
using BlockSense.Views;
using System.Runtime.InteropServices;
using System;

namespace BlockSense;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        // Open the console at application startup
        ConsoleHelper.OpenConsole();

        // Create the main window (or perform any other initialization)
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
    }
}
