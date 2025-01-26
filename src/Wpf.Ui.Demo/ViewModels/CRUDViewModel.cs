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
using LMS.CRM.Core.Data;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Demo.Services.Contracts;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Input;
using System.Collections.ObjectModel;

namespace Wpf.Ui.Demo.ViewModels;

public class CRUDViewModel : ObservableObject
{
    public CRUDViewModel() { }

    
    private bool _dataInitialized = false;

    private ObservableCollection<Members> _entities = new ObservableCollection<Members>();
    public ObservableCollection<Members> Entities
    {
        get => _entities;
        set
        {
            _entities = value;
            OnPropertyChanged(nameof(Entities));
        }
    }

    public void OnNavigatedTo()
    {
        if (!_dataInitialized)
            InitializeData();

        System.Diagnostics.Debug.WriteLine(
            $"INFO | {typeof(DashboardViewModel)} navigated",
            "LMS.CRM.App"
        );
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine(
            $"INFO | {typeof(DashboardViewModel)} navigated",
            "LMS.CRM.App"
        );
    }

    private void InitializeData()
    {
        _dataInitialized = true;
    }
}