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
                CRUD._currentDataType = CRUD.DataType.Members;
                _navigationService.Navigate(typeof(CRUD));
                return;

            case "navigate_to_Resources":
                CRUD._currentDataType = CRUD.DataType.Resources;
                _navigationService.Navigate(typeof(CRUD));
                return;

            case "navigate_to_Reservations":
                CRUD._currentDataType = CRUD.DataType.Reservations;
                _navigationService.Navigate(typeof(CRUD));
                return;
        }
    }

    private void OnOpenWindow(string parameter)
    {
        switch (parameter)
        {
            //case "open_window_store":
            //    _testWindowService.Show<Views.Windows.StoreWindow>();
            //    return;

            //case "open_window_manager":
            //    _testWindowService.Show<Views.Windows.TaskManagerWindow>();
            //    return;

            //case "open_window_editor":
            //    _testWindowService.Show<Views.Windows.EditorWindow>();
            //    return;

            //case "open_window_settings":
            //    _testWindowService.Show<Views.Windows.SettingsWindow>();
            //    return;

            //case "open_window_experimental":
            //    _testWindowService.Show<Views.Windows.ExperimentalWindow>();
            //    return;
        }
    }

    private bool _dataInitialized = false;

    private IEnumerable<string> _listBoxItemCollection = new string[] { };


    private IEnumerable<Brush> _brushCollection = new Brush[] { };

    public IEnumerable<string> ListBoxItemCollection
    {
        get => _listBoxItemCollection;
        set => SetProperty(ref _listBoxItemCollection, value);
    }

    private IEnumerable<int> _pageSizeCollection = new int[] { };
    public IEnumerable<int> PageSizeCollection
    {
        get => _pageSizeCollection;
        set => SetProperty(ref _pageSizeCollection, value);
    }
    private IEnumerable<int> _fetchSizeCollection = new int[] { };
    public IEnumerable<int> FetchSizeCollection
    {
        get => _fetchSizeCollection;
        set => SetProperty(ref _fetchSizeCollection, value);
    }


    private IEnumerable<Users> _usersInfoItemCollection = new Users[] { };
    public IEnumerable<Users> UsersInfoItemCollection
    {
        get => _usersInfoItemCollection;
        set => SetProperty(ref _usersInfoItemCollection, value);
    }

    private bool _areAllSelected_DataSet;
    public bool AreAllSelected_DataSet
    {
        get => _areAllSelected_DataSet;
        set
        {
            _areAllSelected_DataSet = value;
            OnPropertyChanged(nameof(AreAllSelected_DataSet));
        }
    }
    private bool _areAllSelected_VehicleDataSet;
    public bool AreAllSelected_VehicleDataSet
    {
        get => _areAllSelected_VehicleDataSet;
        set
        {
            if (_areAllSelected_VehicleDataSet != value)
            {
                _areAllSelected_VehicleDataSet = value;
                OnPropertyChanged(nameof(AreAllSelected_VehicleDataSet));
            }
        }
    }

    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }

    public ObservableCollection<ChartDataModel> DailyChartData { get; set; }
    public ObservableCollection<ChartDataModel> WeeklyChartData { get; set; }
    public ObservableCollection<ChartDataModel> MonthlyChartData { get; set; }

    public class ChartDataModel
    {
        public string ShiftStats { get; set; }
        public int EnteredCount { get; set; }
        public int ExitedCount { get; set; }
        public int TotalCount { get; set; }
    }

    private IEnumerable<string> _operationCollection = new string[] { };
    public IEnumerable<string> OperationCollection
    {
        get => _operationCollection;
        set => SetProperty(ref _operationCollection, value);
    }

    private IEnumerable<string> _dataCollection = new string[] { };
    public IEnumerable<string> DateCollection
    {
        get => _dataCollection;
        set => SetProperty(ref _dataCollection, value);
    }
    private IEnumerable<string> _vehicleDataCollection = new string[] { };
    public IEnumerable<string> VehicleDateCollection
    {
        get => _vehicleDataCollection;
        set => SetProperty(ref _vehicleDataCollection, value);
    }

    private IEnumerable<string> _passageInfoCollection = new string[] { };
    public IEnumerable<string> PassageInfoCollection
    {
        get => _passageInfoCollection;
        set => SetProperty(ref _passageInfoCollection, value);
    }

    private IEnumerable<string> _vehicleCollection = new string[] { };
    public IEnumerable<string> VehicleCollection
    {
        get => _vehicleCollection;
        set => SetProperty(ref _vehicleCollection, value);
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
        OperationCollection = new[]
        {
            "شامل",
            "=",
            ">",
            "<",
            ">=",
            "=<",
        };


        PageSizeCollection = new[]
        {
            10,
            20,
            30,
            50,
            100
        };

        FetchSizeCollection = new[]
        {
            1000,
            2500,
            5000,
            10000,
            25000,
            50000,
            100000,
        };

        _dataInitialized = true;
    }
}