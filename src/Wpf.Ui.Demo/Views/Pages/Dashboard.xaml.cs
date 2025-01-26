// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using DocumentFormat.OpenXml.CustomProperties;
using LMS.CRM.Core;
using LMS.CRM.Core.Data;
using System;
using System.Collections.Generic;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Demo.ViewModels;

namespace Wpf.Ui.Demo.Views.Pages;

/// <summary>
/// Interaction logic for Dashboard.xaml
/// </summary>
public partial class Dashboard : INavigableView<DashboardViewModel>
{
    public DashboardViewModel ViewModel { get; }

    public Dashboard(DashboardViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        GetDataAsync();
    }

    private void GetDataAsync()
    {
        try
        {
            var libraryBranches = AppDbContext.FetchLibraryBranches();
            ViewModel.LibraryBranchesItemCollection = libraryBranches;
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(GetDataAsync), Logger.LogLevel.Error);
        }
    }
}
