// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Demo.Models.Data;
using Wpf.Ui.Demo.Views.Pages;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Demo.Services.Contracts;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Input;
using System.Collections.ObjectModel;
using LMS.CRM.Views.Windows;
using System.ComponentModel;
using LMS.CRM.Core.Data;

namespace Wpf.Ui.Demo.ViewModels;

public class DataViewModel : ObservableObject, INavigationAware, INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;

    private readonly ITestWindowService _testWindowService;

    private ICommand _navigateCommand;

    private ICommand _openWindowCommand;

    public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<string>(OnNavigate);

    public ICommand OpenWindowCommand =>
        _openWindowCommand ??= new RelayCommand<string>(OnOpenWindow);

    public Users ActiveUser;

    public DataViewModel(
        INavigationService navigationService,
        ITestWindowService testWindowService
    )
    {
        _navigationService = navigationService;
        _testWindowService = testWindowService;

        if (!_dataInitialized)
            InitializeData();
    }

    private void OnNavigate(string parameter)
    {
        switch (parameter)
        {
            case "navigate_to_Members":
                CRUD.CurrentDataType = CRUD.DataType.Members;
                _navigationService.Navigate(1);
                return;

            case "navigate_to_Resources":
                CRUD.CurrentDataType = CRUD.DataType.Resources;
                _navigationService.Navigate(2);
                return;

            case "navigate_to_Reservations":
                CRUD.CurrentDataType = CRUD.DataType.Reservations;
                _navigationService.Navigate(3);
                return;
            
            case "navigate_to_LibraryBranches":
                CRUD.CurrentDataType = CRUD.DataType.LibraryBranches;
                _navigationService.Navigate(4);
                return;
        }
    }


    private void OnOpenWindow(string parameter)
    {
        switch (parameter)
        {
        }
    }

    private bool _dataInitialized = false;

    private IEnumerable<Users> _usersInfoItemCollection = new Users[] { };
    public IEnumerable<Users> UsersInfoItemCollection
    {
        get => _usersInfoItemCollection;
        set => SetProperty(ref _usersInfoItemCollection, value);
    }

    public void OnNavigatedTo()
    {
        if (!_dataInitialized)
            InitializeData();
    }

    public void OnNavigatedFrom() 
    {
    }

    private void InitializeData()
    {
        _dataInitialized = true;
    }
}