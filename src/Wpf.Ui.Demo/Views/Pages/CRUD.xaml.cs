// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Newtonsoft.Json;
using RtspClientSharp;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing.Imaging;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using Wpf.Ui.Common;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using LMS.CRM;
using Clipboard = Wpf.Ui.Common.Clipboard;
using Mohsen;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.UI.Notifications;
using LMS.CRM.Views.Windows;
using Wpf.Ui.Demo.Services.Contracts;
using Wpf.Ui.Demo.Services;
using Windows.ApplicationModel.Activation;
using Wpf.Ui.Demo.ViewModels;
using DocumentFormat.OpenXml.Vml.Office;
using System.Data;
using Windows.Security.Cryptography.Certificates;
using DocumentFormat.OpenXml.Math;
using Windows.Services.Maps;
using Wpf.Ui.TaskBar;
using ClosedXML;
using LMS.CRM.Core.Data;
using LMS.CRM.Core;
using DevExpress.XtraReports.Wizards;
using System.Collections.ObjectModel;

namespace Wpf.Ui.Demo.Views.Pages;

/// <summary>
/// Interaction logic for Controls.xaml
/// </summary> 

public partial class CRUD
{
    private readonly ITestWindowService _testWindowService;

    private readonly ISnackbarService _snackbarService;

    private readonly IDialogControl _dialogControl;

    private readonly ITaskBarService _taskBarService;

    public CRUDViewModel ViewModel { get; }

    public static DataType _currentDataType;
    public enum DataType
    {
        Members,
        Resources,
        Reservations,
    }

    public CRUD(CRUDViewModel viewModel, ISnackbarService snackbarService, IDialogService dialogService, ITestWindowService itestWindowService, ITaskBarService taskBarService)
    {
        ViewModel = viewModel;

        InitializeComponent();

        _testWindowService = itestWindowService;
        _snackbarService = snackbarService;
        _dialogControl = dialogService.GetDialogControl();
        _taskBarService = taskBarService;

        InvokeLoadingAsync();
    }

    private bool _initialized = false;

    private async Task InvokeLoadingAsync()
    {
        if (_initialized)
            return;

        _initialized = true;

        try
        {
            LoadingRing.Visibility = Visibility.Visible;
            gridControl.Opacity = 0.5;
            await Task.Delay(1000);
            await GetDataAsync(0, 1000);
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(InvokeLoadingAsync), Logger.LogLevel.Error);
            OpenSnackbar("خطا", "مشکلی در بارگذاری اولیه رخ داد", SymbolRegular.ArrowRepeatAll24, ControlAppearance.Danger);
        }
        finally
        {
            gridControl.Opacity = 1;
            LoadingRing.Visibility = Visibility.Collapsed;
        }
    }

    private async Task RefreshDataAsync()
    {
        MainPanelGrid.IsEnabled = false;
        LoadingRing.Visibility = Visibility.Visible;

        try
        {
            await GetDataAsync(0, 1000);
        }
        finally
        {
            MainPanelGrid.IsEnabled = true;
            LoadingRing.Visibility = Visibility.Collapsed;
        }
    }

    private async Task<List<T>> FetchDataAsync<T>(Func<AppDbContext, IQueryable<T>> queryBuilder, int startIndex, int fetchSize)
        where T : class
    {
        try
        {
            return await Task.Run(() =>
            {
                using (var context = new AppDbContext())
                {
                    return queryBuilder(context)
                        .Skip(startIndex)
                        .Take(fetchSize)
                        .ToList();
                }
            });
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(FetchDataAsync), Logger.LogLevel.Error);
            OpenSnackbar("Error Fetching Data", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
            return new List<T>();
        }
    }

    private Func<AppDbContext, IQueryable<Members>> MemberQueryBuilder = context =>
    context.Members.OrderBy(m => m.MemberID);

    private Func<AppDbContext, IQueryable<Resources>> ResourceQueryBuilder = context =>
        context.Resources.OrderBy(r => r.ResourceID);

    private Func<AppDbContext, IQueryable<Reservations>> ReservationQueryBuilder = context =>
        context.Reservations.OrderBy(res => res.ReservationID);


    private async Task GetDataAsync(int startIndex, int fetchSize)
    {
        try
        {
            // Validate fetch size
            fetchSize = fetchSize > 0 ? fetchSize : 20;

            switch (_currentDataType)
            {
                case DataType.Members:
                    var members = await FetchDataAsync(MemberQueryBuilder, startIndex, fetchSize);
                    Dispatcher.Invoke(() => gridControl.ItemsSource = members);
                    break;

                case DataType.Resources:
                    var resources = await FetchDataAsync(ResourceQueryBuilder, startIndex, fetchSize);
                    Dispatcher.Invoke(() => gridControl.ItemsSource = resources);
                    break;

                case DataType.Reservations:
                    var reservations = await FetchDataAsync(ReservationQueryBuilder, startIndex, fetchSize);
                    Dispatcher.Invoke(() => gridControl.ItemsSource = reservations);
                    break;

                default:
                    Dispatcher.Invoke(() => gridControl.ItemsSource = new List<object>()); // Fallback empty list
                    break;
            }

            // Display success message
            Dispatcher.Invoke(() =>
            {
                OpenSnackbar("Data Loaded", "Additional data fetched successfully.", SymbolRegular.CheckmarkCircle24, ControlAppearance.Success);
            });
        }
        catch (Exception ex)
        {
            // Log and show error
            Dispatcher.Invoke(() =>
            {
                Logger.SaveLog(ex, nameof(GetDataAsync), Logger.LogLevel.Error);
                OpenSnackbar("Error Fetching Data", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
            });
        }
    }

    private async void TableView_RowUpdated(object sender, DevExpress.Xpf.Grid.RowEventArgs e)
    {
        var member = e.Row as Members;
        if (member == null) return;

        try
        {
            AppDbContext.UpdateMember(member);
            OpenSnackbar("Success", "Changes saved successfully.", SymbolRegular.Checkmark24, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(TableView_RowUpdated), Logger.LogLevel.Error);
            OpenSnackbar("Error Saving Data", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    private void TableView_ValidateRow(object sender, DevExpress.Xpf.Grid.GridRowValidationEventArgs e)
    {

    }

    private void TableView_ValidateRowDeletion(object sender, DevExpress.Xpf.Grid.GridValidateRowDeletionEventArgs e)
    {
        var member = e.Rows.FirstOrDefault() as Members;
        if (member == null) return;

        try
        {
            AppDbContext.DeleteMember(member);
            OpenSnackbar("Success", "Record deleted successfully.", SymbolRegular.Checkmark24, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(TableView_ValidateRowDeletion), Logger.LogLevel.Error);
            OpenSnackbar("Error Deleting Data", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }


    private void OnAddMember(object sender, RoutedEventArgs e)
    {

    }

    private void OnEditMember(object sender, RoutedEventArgs e)
    {

    }

    private void OnDeleteMember(object sender, RoutedEventArgs e)
    {

    }

    private void ShowAddPanel_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Visible;
        FilterPanel.Visibility = Visibility.Collapsed;
    }

    private async Task ManagePanelVisibilityAsync(bool isPanelVisible, Grid overlayGrid)
    {
        overlayGrid.IsEnabled = !isPanelVisible;
        overlayGrid.Opacity = isPanelVisible ? 0.5 : 1;

        if (isPanelVisible) await Task.Delay(500);
    }

    private void ShowEditPanel_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Visible;
        _ = ManagePanelVisibilityAsync(true, MainPanelGrid);
    }

    private void CloseEditPanel_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Collapsed;
        _ = ManagePanelVisibilityAsync(false, MainPanelGrid);
    }


    private void ShowFilterPanel_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Visible;
        _ = ManagePanelVisibilityAsync(true, MainPanelGrid);
    }

    private void CloseFilterPanel_Click(object sender, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Collapsed;
        _ = ManagePanelVisibilityAsync(false, MainPanelGrid);
    }

    private void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {

    }

    private void CancelFilter_Click(object sender, RoutedEventArgs e)
    {

    }

    private void CancelEdit_Click(object sender, RoutedEventArgs e)
    {

    }

    private void SaveEdit_Click(object sender, RoutedEventArgs e)
    {

    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        //RootPanel.ScrollOwner = ScrollHost;

        _dialogControl.ButtonRightClick += DialogControlOnButtonRightClick;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _dialogControl.ButtonRightClick -= DialogControlOnButtonRightClick;
    }

    private async void OpenDialog(string title, string message)
    {
        var result = await _dialogControl.ShowAndWaitAsync(title, message);
    }

    private static void DialogControlOnButtonRightClick(object sender, RoutedEventArgs e)
    {
        var dialogControl = (IDialogControl)sender;
        dialogControl.Hide();
    }

    private void OpenSnackbar(string title, string message, SymbolRegular symbol, ControlAppearance controlAppearance)
    {
        _snackbarService.Show(title, message, symbol);
    }

    private void OpenMessageBox(string message, string caption, string buttonLeftName, string buttonRightName)
    {
        var messageBox = new Wpf.Ui.Controls.MessageBox();

        messageBox.ButtonLeftName = buttonLeftName;
        messageBox.ButtonRightName = buttonRightName;

        messageBox.ButtonLeftClick += MessageBox_LeftButtonClick;
        messageBox.ButtonRightClick += MessageBox_RightButtonClick;

        messageBox.Show(caption, message);
    }

    private void MessageBox_LeftButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        (sender as Wpf.Ui.Controls.MessageBox)?.Close();
    }

    private void MessageBox_RightButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        (sender as Wpf.Ui.Controls.MessageBox)?.Close();
    }
}