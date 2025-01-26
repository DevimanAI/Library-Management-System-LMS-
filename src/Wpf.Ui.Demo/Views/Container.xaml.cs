// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using DocumentFormat.OpenXml.Drawing.Charts;
using LMS.CRM.Core;
using LMS.CRM.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using LMS.CRM;
using LMS.CRM.Core;
using LMS.CRM.Views.Windows;
using Windows.Gaming.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Demo.Services.Contracts;
using Wpf.Ui.Demo.ViewModels;
using Wpf.Ui.Demo.Views.Pages;
using Wpf.Ui.Demo.Views.Windows;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.TaskBar;

namespace Wpf.Ui.Demo.Views;

/// <summary>
/// Interaction logic for Container.xaml
/// </summary>
public partial class Container : INavigationWindow
{
    private bool _initialized = false;

    private readonly IThemeService _themeService;

    private readonly ITaskBarService _taskBarService;

    public DataViewModel ViewModel { get; }

    // NOTICE: In the case of this window, we navigate to the Dashboard after loading with Container.InitializeUi()

    LMS.CRM.Properties.Settings settings = new LMS.CRM.Properties.Settings();
    public static Users _activeUser = new Users();

    public Container(
        DataViewModel viewModel,
        INavigationService navigationService,
        IPageService pageService,
        IThemeService themeService,
        ITaskBarService taskBarService,
        ISnackbarService snackbarService,
        IDialogService dialogService
    )
    {
        // Assign the view model
        ViewModel = viewModel;
        DataContext = this;

        // Attach the theme service
        _themeService = themeService;

        // Attach the taskbar service
        _taskBarService = taskBarService;

        //// Context provided by the service provider.
        //DataContext = viewModel;

        // Initial preparation of the window.
        InitializeComponent();

        // We define a page provider for navigation
        SetPageService(pageService);

        // If you want to use INavigationService instead of INavigationWindow you can define its navigation here.
        navigationService.SetNavigationControl(RootNavigation);

        // Allows you to use the Snackbar control defined in this window in other pages or windows
        snackbarService.SetSnackbarControl(RootSnackbar);

        // Allows you to use the Dialog control defined in this window in other pages or windows
        dialogService.SetDialogControl(RootDialog);

        // !! Experimental option
        //RemoveTitlebar();

        // !! Experimental option
        //ApplyBackdrop(Wpf.Ui.Appearance.BackgroundType.Mica);

        KeyDown += HandleKeyPress;

        // We initialize a cute and pointless loading splash that prepares the view and navigate at the end.
        Loaded += (_, _) =>
        {
            RootMainGrid.Visibility = Visibility.Collapsed;

            InvokeLogin();
        };

        // We register a window in the Watcher class, which changes the application's theme if the system theme changes.
        // Wpf.Ui.Appearance.Watcher.Watch(this, Appearance.BackgroundType.Mica, true, false);
    }

    private void HandleKeyPress(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Login_Click(sender, e);
        }
    }

    /// <summary>
    /// Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    #region INavigationWindow methods

    public Frame GetFrame() => RootFrame;

    public INavigation GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) =>
        RootNavigation.PageService = pageService;

    public void ShowWindow() => Show();

    public void CloseWindow() => Close();

    #endregion INavigationWindow methods

    private void OpenSnackbar(string title, string message, SymbolRegular symbol, ControlAppearance controlAppearance)
    {
        RootSnackbar.Show(title, message, symbol, controlAppearance);
    }

    public void InvokeLogin()
    {
        _initialized = false;

        RootLoginGrid.Visibility = Visibility.Visible;

        Username.Text = settings.SavedUser;

        var pass = PasswordEncryptor.DecryptPassword(settings.SavedPass);
        if (!string.IsNullOrEmpty(pass)) Password.Password = pass;

        if (AppDbContext.IsDatabaseConnected())
        {
            AppDbContext.ReadAllUsers(out var users);
            ViewModel.UsersInfoItemCollection = users;

            NotificationBar.IsOpen = false;
            DatabaseReconnect.Visibility = Visibility.Collapsed;
        }
        else
        {
            NotificationBar.Title = "پایگاه داده";
            NotificationBar.IsOpen = true;
            NotificationBar.Message = "خطا در اتصال به پایگاه داده";
            NotificationBar.Severity = Wpf.Ui.Controls.InfoBarSeverity.Warning;

            DatabaseReconnect.Visibility = Visibility.Visible;
        }
    }
    private void DatabaseReconnect_Click(object sender, RoutedEventArgs e)
    {
        InvokeLogin();
    }
    private void ForgetPass_Click(object sender, RoutedEventArgs e)
    {
        settings.SavedUser = "";
        settings.SavedPass = "";

        settings.Save();
    }
    private void Login_Click(object sender, RoutedEventArgs e)
    {
        login();

        Login.Focus();
    }
    private void RegisterAndLogin_Click(object sender, RoutedEventArgs e)
    {
        AppDbContext.UpdateUser(new Users()
        {
            Username = Username.Text,
            PasswordHash = PasswordEncryptor.EncryptPassword(Password.Password),
        });

        AppDbContext.ReadAllUsers(out var users);
        ViewModel.UsersInfoItemCollection = users;

        login();

        RegisterAndLogin.Focus();
    }

    private void Login_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Login_Click(sender, e);
        }
    }
    private void login()
    {
        // Encrypt the password
        string encryptedPassword = PasswordEncryptor.EncryptPassword(Password.Password);

        // Find the user based on username and encrypted password
        _activeUser = ViewModel.UsersInfoItemCollection
            .FirstOrDefault(u =>
                u.Username == Username.Text &&
                encryptedPassword == u.PasswordHash);

        if (_activeUser != null)
        {
            // Set active user details
            ActiveUser.Content = _activeUser.Username;
            ActiveUser.Tag = _activeUser;

            // Save settings
            LMS.CRM.Properties.Settings settings = new LMS.CRM.Properties.Settings
            {
                ActiveUser = Username.Text
            };

            if ((bool)SaveUser.IsChecked) settings.SavedUser = Username.Text;
            if ((bool)SavePass.IsChecked) settings.SavedPass = encryptedPassword;

            settings.Save();

            // Log the user in and navigate to the dashboard
            Logger log = new Logger(_activeUser);
            NavigateLoginToDashboard();
        }
        else
        {
            // Handle login failure
            Username.Focus();
            NotificationBar.Title = "ورود ناموفق";
            NotificationBar.IsOpen = true;
            NotificationBar.Message = "مشخصات کاربر اشتباه است.";
            NotificationBar.Severity = Wpf.Ui.Controls.InfoBarSeverity.Warning;
        }
    }

    private void NavigateLoginToDashboard()
    {
        InvokeSplashScreen();
    }
    private void InvokeSplashScreen()
    {
        if (_initialized)
            return;

        _initialized = true;

        WelcomeMessage.Text = $"{_activeUser.Username} به پنل کنترلی خوش آمدید!\nدر حال آماده سازی.";
        RootLoginGrid.Visibility = Visibility.Collapsed;
        RootWelcomeGrid.Visibility = Visibility.Visible;

        _taskBarService.SetState(this, TaskBarProgressState.Indeterminate);

        Task.Run(async () =>
        {
            Logger.SaveLog($"ورود کاربر", System.Reflection.MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Events);

            List<Users> users = ViewModel.UsersInfoItemCollection.ToList();
            ViewModel.UsersInfoItemCollection = users;

            await Task.Delay(1000);

            await Dispatcher.InvokeAsync(() =>
            {
                RootWelcomeGrid.Visibility = Visibility.Hidden;
                RootMainGrid.Visibility = Visibility.Visible;

                LMS.CRM.Properties.Settings settings = new LMS.CRM.Properties.Settings();

                Navigate(typeof(Pages.Dashboard));

                _taskBarService.SetState(this, TaskBarProgressState.None);
            });

            return true;
        });
    }

    private void NavigationButtonTheme_OnClick(object sender, RoutedEventArgs e)
    {
        _themeService.SetTheme(
            _themeService.GetTheme() == ThemeType.Dark ? ThemeType.Light : ThemeType.Dark
        );
    }

    private void TrayMenuItemReload_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;

        System.Diagnostics.Debug.WriteLine(
            $"DEBUG | LMS.CRM Tray clicked: {menuItem.Tag}",
            "LMS.CRM"
        );

        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
        Application.Current.Shutdown();
    }
    private void TrayMenuItemExit_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem)
            return;

        System.Diagnostics.Debug.WriteLine(
            $"DEBUG | LMS.CRM Tray clicked: {menuItem.Tag}",
            "LMS.CRM"
        );

        Application.Current.Shutdown();
    }

    private void RootNavigation_OnNavigated(INavigation sender, RoutedNavigationEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(
            $"DEBUG | LMS.CRM Navigated to: {sender?.Current ?? null}",
            "LMS.CRM"
        );

        // This funky solution allows us to impose a negative
        // margin for Frame only for the Dashboard page, thanks
        // to which the banner will cover the entire page nicely.
        RootFrame.Margin = new System.Windows.Thickness(
            left: 0,
            top: sender?.Current?.PageTag == "dashboard" ? -69 : 0,
            right: 0,
            bottom: 0
        );
    }

    private void UiWindow_Closed(object sender, EventArgs e)
    {
        LMS.CRM.Properties.Settings settings = new LMS.CRM.Properties.Settings();
        settings.ActiveUser = "";
        settings.Save();
    }

    private void LogOut_Click(object sender, RoutedEventArgs e)
    {
        Logger.SaveLog($"خروج کاربر", System.Reflection.MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Events);

        RootMainGrid.Visibility = Visibility.Collapsed;
        InvokeLogin();
    }

    //private void LogOut_OnClick(object sender, RoutedEventArgs e)
    //{
    //    System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
    //    Application.Current.Shutdown();
    //}
}