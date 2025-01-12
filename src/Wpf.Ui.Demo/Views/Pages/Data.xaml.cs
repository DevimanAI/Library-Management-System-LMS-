// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Data.SQLite;
using System;
using System.Windows;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Demo.ViewModels;
using Wpf.Ui.Mvvm.Contracts;
using System.Linq;
using System.Globalization;
using Mohsen;
using System.Collections.Generic;
using Wpf.Ui.Demo.Models.Data;
using System.Drawing;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Common;
using System.ComponentModel;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Windows.Media.Media3D;
using System.Data;
using Windows.Devices.Geolocation;
using ExcelDataReader;
using System.Text;
using System.Windows.Media.Imaging;
using LMS.CRM.Core;
using LMS.CRM.Core.Data;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Navigation;
using LMS.CRM.Views.Windows;

namespace Wpf.Ui.Demo.Views.Pages;

/// <summary>
/// Interaction logic for Data.xaml
/// </summary>
public partial class Data : INavigableView<DataViewModel>
{
    public DataViewModel ViewModel { get; }
    private readonly ISnackbarService _snackbarService;

    public Data(DataViewModel viewModel, ISnackbarService snackbarService, IPageService pageService)
    {
        ViewModel = viewModel;
        ViewModel.ActiveUser = Container._activeUser;

        InitializeComponent();
        _snackbarService = snackbarService;
    }
}