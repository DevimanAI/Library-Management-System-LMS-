// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LMS.CRM.Core.Data;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Demo.Services.Contracts;
using Wpf.Ui.Mvvm.Contracts;

namespace Wpf.Ui.Demo.ViewModels;

public class DashboardViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    private readonly ITestWindowService _testWindowService;

    private ICommand _navigateCommand;

    private ICommand _openWindowCommand;

    public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<string>(OnNavigate);

    public ICommand OpenWindowCommand =>
        _openWindowCommand ??= new RelayCommand<string>(OnOpenWindow);

    public DashboardViewModel(
        INavigationService navigationService,
        ITestWindowService testWindowService
    )
    {
        _navigationService = navigationService;
        _testWindowService = testWindowService;
    }

    public void OnNavigatedTo()
    {
        System.Diagnostics.Debug.WriteLine(
            $"INFO | {typeof(DashboardViewModel)} navigated",
            "Tiva.Gate.App"
        );
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine(
            $"INFO | {typeof(DashboardViewModel)} navigated",
            "Tiva.Gate.App"
        );
    }

    private void OnNavigate(string parameter)
    {
        switch (parameter)
        {
            case "navigate_to_licenseControl":
                return;

            case "navigate_to_usersInfo":
                return;

            case "navigate_to_CameraStreamViewer":
                return;

            case "navigate_to_Data":
                _navigationService.Navigate(typeof(Views.Pages.Data));
                return;

            case "navigate_to_AccessCheck":
                return;

            case "navigate_to_BasicData":
                return;
        }
    }

    private void OnOpenWindow(string parameter)
    {
        switch (parameter)
        {
            case "open_window_store":
                _testWindowService.Show<Views.Windows.StoreWindow>();
                return;

            case "open_window_manager":
                _testWindowService.Show<Views.Windows.TaskManagerWindow>();
                return;

            case "open_window_editor":
                _testWindowService.Show<Views.Windows.EditorWindow>();
                return;

            case "open_window_experimental":
                _testWindowService.Show<Views.Windows.ExperimentalWindow>();
                return;
        }
    }
}
