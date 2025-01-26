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
using DevExpress.XtraScheduler.Native;
using static LMS.CRM.Core.Data.AppDbContext;
using DocumentFormat.OpenXml.Bibliography;

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

    private static DataType _currentDataType;
    public static DataType CurrentDataType
    {
        get => _currentDataType;
        set
        {
            if (_currentDataType != value)
            {
                _currentDataType = value;
                OnDataTypeChanged();
            }
        }
    }

    // Define a static event
    public static event Action DataTypeChanged;

    private static void OnDataTypeChanged()
    {
        DataTypeChanged?.Invoke();
    }
    public enum DataType
    {
        Members,
        Resources,
        Reservations,
        LibraryBranches,
    }

    public CRUD(CRUDViewModel viewModel, ISnackbarService snackbarService, IDialogService dialogService, ITestWindowService itestWindowService, ITaskBarService taskBarService)
    {
        ViewModel = viewModel;

        InitializeComponent();

        DataTypeChanged += UpdatePanelDataType;
        //DataTypeChanged += async () => await InvokeLoadingAsync();

        _testWindowService = itestWindowService;
        _snackbarService = snackbarService;
        _dialogControl = dialogService.GetDialogControl();
        _taskBarService = taskBarService;

        InvokeLoadingAsync();
    }
    private void UpdatePanelDataType()
    {
        _initialized = false;

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
    private Func<AppDbContext, IQueryable<LibraryBranches>> LibraryBranchesQueryBuilder = context =>
        context.LibraryBranches.OrderBy(lib => lib.BranchID);

    private async Task GetDataAsync(int startIndex, int fetchSize)
    {
        try
        {
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

                case DataType.LibraryBranches:
                    var libBranches = await FetchDataAsync(LibraryBranchesQueryBuilder, startIndex, fetchSize);
                    Dispatcher.Invoke(() => gridControl.ItemsSource = libBranches);
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
        if (e.Row == null) return;

        try
        {
            switch (_currentDataType)
            {
                case DataType.Members:
                    AppDbContext.UpdateMember(e.Row as Members);
                    break;
                case DataType.Resources:
                    AppDbContext.UpdateResource(e.Row as Resources);
                    break;
                case DataType.Reservations:
                    AppDbContext.UpdateReservation(e.Row as Reservations);
                    break;

                case DataType.LibraryBranches:
                    AppDbContext.UpdateLibraryBranches(e.Row as LibraryBranches);
                    break;
            }
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
        if (e.Row is Members member && string.IsNullOrEmpty(member.FirstName))
        {
            e.IsValid = false;
            e.ErrorContent = "First Name is required.";
        }
    }

    private void TableView_ValidateRowDeletion(object sender, DevExpress.Xpf.Grid.GridValidateRowDeletionEventArgs e)
    {
        var item = e.Rows.FirstOrDefault();
        if (item == null) return;

        try
        {
            switch (_currentDataType)
            {
                case DataType.Members:
                    AppDbContext.DeleteMember(item as Members);
                    break;
                case DataType.Resources:
                    AppDbContext.DeleteResource(item as Resources);
                    break;
                case DataType.Reservations:
                    AppDbContext.DeleteReservation(item as Reservations);
                    break;
                case DataType.LibraryBranches:
                    AppDbContext.DeleteLibraryBranches(item as LibraryBranches);
                    break;
            }
            OpenSnackbar("Success", "Record deleted successfully.", SymbolRegular.Checkmark24, ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(TableView_ValidateRowDeletion), Logger.LogLevel.Error);
            OpenSnackbar("Error Deleting Data", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    private async Task ManagePanelVisibilityAsync(bool isPanelVisible, FrameworkElement panel)
    {
        panel.Visibility = isPanelVisible ? Visibility.Visible : Visibility.Collapsed;
        MainPanelGrid.IsEnabled = !isPanelVisible;
        MainPanelGrid.Opacity = isPanelVisible ? 0.5 : 1;

        if (isPanelVisible) await Task.Delay(500);
    }

    private async void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            switch (_currentDataType)
            {
                case DataType.Members:
                    var memberFilter = new MemberFilter
                    {
                        FirstName = FilterFirstName.Text,
                        LastName = FilterLastName.Text,
                        NationalCode = FilterNationalCode.Text
                    };
                    gridControl.ItemsSource = await FetchDataAsync(GetFilteredMembers(memberFilter), 0, 20);
                    break;

                case DataType.Resources:
                    var resourceFilter = new ResourceFilter
                    {
                        Title = FilterResourceName.Text,
                        ResourceType = FilterCategory.SelectedItemValue?.ToString(),
                        Author = FilterAuthor.Text,
                        Publisher = FilterPublisher.Text,
                        PublishYear = FilterPublishYear.Text,
                        ISBN = FilterISBN.Text,
                        IsAvailable = FilterAvailable.IsChecked
                    };
                    gridControl.ItemsSource = await FetchDataAsync(GetFilteredResources(resourceFilter), 0, 20);
                    break;

                case DataType.Reservations:
                    var reservationFilter = new ReservationFilter
                    {
                        StartDate = FilterStartDate.DateTime,
                        EndDate = FilterExpiryDate.DateTime,
                        MemberID = int.TryParse(FilterMemberID.Text, out var memberId) ? memberId : (int?)null,
                        ResourceID = int.TryParse(FilterResourceID.Text, out var resourceId) ? resourceId : (int?)null,
                        Status = FilterStatus.Text
                    };
                    gridControl.ItemsSource = await FetchDataAsync(GetFilteredReservations(reservationFilter), 0, 20);
                    break;

                case DataType.LibraryBranches:
                    var libraryBranchesFilter = new LibraryBranchesFilter
                    {
                        BranchName = FilterBranchName.Text,
                        BranchAddress = FilterBranchAddress.Text,
                        BranchPhoneNum = FilterBranchPhoneNum.Text
                    };
                    gridControl.ItemsSource = await FetchDataAsync(GetFilteredLibraryBranches(libraryBranchesFilter), 0, 20);
                    break;
            }

            OpenSnackbar("Success", "Filter applied successfully.", SymbolRegular.Checkmark24, ControlAppearance.Success);
            await ManagePanelVisibilityAsync(false, FilterPanel);
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(ApplyFilter_Click), Logger.LogLevel.Error);
            OpenSnackbar("Error Applying Filter", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }

    #region Filters
    public class MemberFilter
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalCode { get; set; }
    }
    public class ResourceFilter
    {
        public string Title { get; set; }
        public string ResourceType { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string PublishYear { get; set; }
        public string ISBN { get; set; }
        public bool? IsAvailable { get; set; }
    }

    public class ReservationFilter
    {
        public int? MemberID { get; set; }
        public int? ResourceID { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class LibraryBranchesFilter
    {
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public string BranchPhoneNum { get; set; }
    }

    private Func<AppDbContext, IQueryable<Members>> GetFilteredMembers(MemberFilter filter)
    {
        return context =>
        {
            var query = context.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.FirstName))
                query = query.Where(m => m.FirstName.Contains(filter.FirstName));

            if (!string.IsNullOrWhiteSpace(filter.LastName))
                query = query.Where(m => m.LastName.Contains(filter.LastName));

            if (!string.IsNullOrWhiteSpace(filter.NationalCode))
                query = query.Where(m => m.NationalCode.Contains(filter.NationalCode));

            return query.OrderBy(m => m.MemberID);
        };
    }

    private Func<AppDbContext, IQueryable<Resources>> GetFilteredResources(ResourceFilter filter)
    {
        return context =>
        {
            var query = context.Resources.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Title))
                query = query.Where(r => r.Title.Contains(filter.Title));

            if (!string.IsNullOrWhiteSpace(filter.ResourceType))
                query = query.Where(r => r.ResourceType == filter.ResourceType);

            if (!string.IsNullOrWhiteSpace(filter.Author))
                query = query.Where(r => r.Author.Contains(filter.Author));

            if (!string.IsNullOrWhiteSpace(filter.Publisher))
                query = query.Where(r => r.Publisher.Contains(filter.Publisher));

            if (!string.IsNullOrWhiteSpace(filter.PublishYear))
                query = query.Where(r => r.PublishYear == filter.PublishYear);

            if (!string.IsNullOrWhiteSpace(filter.ISBN))
                query = query.Where(r => r.ISBN == filter.ISBN);

            if (filter.IsAvailable.HasValue)
                query = query.Where(r => r.AvailableCopies > 0); // Availability check

            return query.OrderBy(r => r.ResourceID);
        };
    }

    private Func<AppDbContext, IQueryable<Reservations>> GetFilteredReservations(ReservationFilter filter)
    {
        return context =>
        {
            var query = context.Reservations.AsQueryable();

            if (filter.MemberID.HasValue)
                query = query.Where(r => r.FK_MemberID == filter.MemberID.Value);

            if (filter.ResourceID.HasValue)
                query = query.Where(r => r.FK_ResourceID == filter.ResourceID.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(r => r.Status == filter.Status);

            if (filter.StartDate.HasValue)
                query = query.Where(r => r.ReservationDate >= filter.StartDate.Value);

            if (filter.EndDate.HasValue)
                query = query.Where(r => r.ReservationDate <= filter.EndDate.Value);

            return query.OrderBy(r => r.ReservationID);
        };
    }

    private Func<AppDbContext, IQueryable<LibraryBranches>> GetFilteredLibraryBranches(LibraryBranchesFilter filter)
    {
        return context =>
        {
            var query = context.LibraryBranches.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.BranchName))
                query = query.Where(lib => lib.BranchName == filter.BranchName);

            if (!string.IsNullOrWhiteSpace(filter.BranchAddress))
                query = query.Where(lib => lib.Address == filter.BranchAddress);

            if (!string.IsNullOrWhiteSpace(filter.BranchName))
                query = query.Where(lib => lib.PhoneNumber == filter.BranchPhoneNum);

            return query.OrderBy(lib => lib.BranchID);
        };
    }

    #endregion

    // DRY Panel Management Methods
    private void PopulateEditPanel(object selectedItem)
    {
        MemberFields.Visibility = Visibility.Collapsed;
        ResourceFields.Visibility = Visibility.Collapsed;
        ReservationFields.Visibility = Visibility.Collapsed;
        LibraryBranchesFields.Visibility = Visibility.Collapsed;

        switch (CRUD.CurrentDataType)
        {
            case CRUD.DataType.Members:
                MemberFields.Visibility = Visibility.Visible;
                var member = selectedItem as Members;
                if (member != null)
                {
                    EditNationalCode.Text = member.NationalCode;
                    EditFirstName.Text = member.FirstName;
                    EditLastName.Text = member.LastName;
                    EditBirthDate.DateTime = member.BirthDate ?? DateTime.Now;
                    EditAddress.Text = member.Address;
                    EditPhoneNumber.Text = member.PhoneNumber;
                    EditPostCode.Text = member.PostCode;
                    EditFatherName.Text = member.FatherName;
                    EditDebt.Value = member.Debt ?? 0;
                    EditStatus.SelectedItem = member.Status;
                    EditMembershipStart.DateTime = member.StartDate;
                    EditMembershipEnd.DateTime = member.ExpiryDate;
                    EditMaxBorrowCount.Value = member.MaxBorrowCount ?? 5;
                }
                break;

            case CRUD.DataType.Resources:
                ResourceFields.Visibility = Visibility.Visible;
                var resource = selectedItem as Resources;
                if (resource != null)
                {
                    EditResourceName.Text = resource.Title;
                    EditAuthor.Text = resource.Author;
                    EditPublisher.Text = resource.Publisher;
                    EditPublishYear.Value = Convert.ToDecimal(resource.PublishYear);
                    EditISBN.Text = resource.ISBN;
                    EditPublishYear.Text = resource.PublishYear;
                    EditQuantity.Value = resource.Quantity.HasValue ? (decimal)resource.Quantity.Value : 0;
                    EditAvailableCopies.Value = resource.AvailableCopies.HasValue ? (decimal)resource.AvailableCopies.Value : 0;
                    EditCategory.SelectedItem = resource.ResourceType;
                }
                break;

            case CRUD.DataType.Reservations:
                ReservationFields.Visibility = Visibility.Visible;
                var reservation = selectedItem as Reservations;
                if (reservation != null)
                {
                    EditMemberID.Value = reservation.FK_MemberID;
                    EditResourceID.Value = reservation.FK_ResourceID;
                    EditReservationDate.DateTime = reservation.ReservationDate;
                    EditStatus.SelectedItem = reservation.Status;
                }
                break;

            case CRUD.DataType.LibraryBranches:
                LibraryBranchesFields.Visibility = Visibility.Visible;
                var libraryBranches = selectedItem as LibraryBranches;
                if (libraryBranches != null)
                {
                    LibraryBranchesFields.Tag = libraryBranches.BranchID;
                    EditBranchName.Text = libraryBranches.BranchName;
                    EditBranchAddress.Text = libraryBranches.Address;
                    EditBranchPhoneNum.Text = libraryBranches.PhoneNumber;
                    EditBranchImage.Source = libraryBranches.ImageSource;
                }
                break;
        }
    }
    private void ClearEditPanel()
    {
        MemberFields.Visibility = Visibility.Collapsed;
        ResourceFields.Visibility = Visibility.Collapsed;
        ReservationFields.Visibility = Visibility.Collapsed;
        LibraryBranchesFields.Visibility = Visibility.Collapsed;

        switch (CRUD.CurrentDataType)
        {
            case CRUD.DataType.Members:
                MemberFields.Visibility = Visibility.Visible;
                EditNationalCode.Text = "";
                EditFirstName.Text = "";
                EditLastName.Text = "";
                EditBirthDate.DateTime = DateTime.Now;
                EditAddress.Text = "";
                EditPhoneNumber.Text = "";
                EditPostCode.Text = "";
                EditFatherName.Text = "";
                EditDebt.Value = 0;
                EditStatus.SelectedIndex = -1;
                EditMembershipStart.DateTime = DateTime.Now;
                EditMembershipEnd.DateTime = DateTime.Now;
                EditMaxBorrowCount.Value = 5;
                break;

            case CRUD.DataType.Resources:
                ResourceFields.Visibility = Visibility.Visible;
                EditResourceName.Text = "";
                EditAuthor.Text = "";
                EditPublisher.Text = "";
                EditPublishYear.Value = DateTime.Now.Year;
                EditISBN.Text = "";
                EditPublishYear.Text = "";
                EditQuantity.Value = 0;
                EditAvailableCopies.Value = 0;
                EditCategory.SelectedItem = "";
                break;

            case CRUD.DataType.Reservations:
                ReservationFields.Visibility = Visibility.Visible;
                EditMemberID.Value = 0;
                EditResourceID.Value = 0;
                EditReservationDate.DateTime = DateTime.Now;
                ReserveEditStatus.SelectedItem = -1;
                break;

            case CRUD.DataType.LibraryBranches:
                LibraryBranchesFields.Visibility = Visibility.Visible;
                LibraryBranchesFields.Tag = null;
                EditBranchName.Text = "";
                EditBranchAddress.Text = "";
                EditBranchPhoneNum.Text = "";
                EditBranchImage.Source = null;
                break;
        }
    }
    private async void SaveEdit_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using (var context = new AppDbContext())
            {
                switch (CRUD.CurrentDataType)
                {
                    case CRUD.DataType.Members:
                        var existingMember = context.Members.FirstOrDefault(m => m.NationalCode == EditNationalCode.Text);
                        if (existingMember != null)
                        {
                            // Update only the properties
                            existingMember.FirstName = EditFirstName.Text;
                            existingMember.LastName = EditLastName.Text;
                            existingMember.BirthDate = EditBirthDate.DateTime;
                            existingMember.Address = EditAddress.Text;
                            existingMember.PhoneNumber = EditPhoneNumber.Text;
                            existingMember.PostCode = EditPostCode.Text;
                            existingMember.FatherName = EditFatherName.Text;
                            existingMember.Debt = (decimal?)EditDebt.Value;
                            existingMember.Status = EditStatus.SelectedItem?.ToString();
                            existingMember.StartDate = EditMembershipStart.DateTime;
                            existingMember.ExpiryDate = EditMembershipEnd.DateTime;
                            existingMember.MaxBorrowCount = (byte?)EditMaxBorrowCount.Value;
                        }
                        else
                        {
                            context.Members.Add(new Members
                            {
                                FirstName = EditFirstName.Text,
                                LastName = EditLastName.Text,
                                NationalCode = EditNationalCode.Text,
                                BirthDate = EditBirthDate.DateTime,
                                Address = EditAddress.Text,
                                PhoneNumber = EditPhoneNumber.Text,
                                PostCode = EditPostCode.Text,
                                FatherName = EditFatherName.Text,
                                Debt = (decimal?)EditDebt.Value,
                                Status = EditStatus.SelectedItem?.ToString(),
                                StartDate = EditMembershipStart.DateTime,
                                ExpiryDate = EditMembershipEnd.DateTime,
                                MaxBorrowCount = (byte?)EditMaxBorrowCount.Value
                            });
                        }
                        break;

                    case CRUD.DataType.Resources:
                        var existingResource = context.Resources.FirstOrDefault(r => r.ISBN == EditISBN.Text);
                        if (existingResource != null)
                        {
                            // Update only the properties
                            existingResource.Title = EditResourceName.Text;
                            existingResource.ResourceType = EditCategory.SelectedItem?.ToString();
                            existingResource.Author = EditAuthor.Text;
                            existingResource.Publisher = EditPublisher.Text;
                            existingResource.PublishYear = EditPublishYear.Text;
                            existingResource.Quantity = (short?)EditQuantity.Value;
                            existingResource.AvailableCopies = (short?)EditAvailableCopies.Value;
                        }
                        else
                        {
                            context.Resources.Add(new Resources
                            {
                                Title = EditResourceName.Text,
                                ResourceType = EditCategory.SelectedItem?.ToString(),
                                Author = EditAuthor.Text,
                                Publisher = EditPublisher.Text,
                                PublishYear = EditPublishYear.Text,
                                ISBN = EditISBN.Text,
                                Quantity = (short?)EditQuantity.Value,
                                AvailableCopies = (short?)EditAvailableCopies.Value
                            });
                        }
                        break;

                    case CRUD.DataType.Reservations:
                        var existingReservations = context.Reservations.FirstOrDefault(r => r.FK_MemberID == (int)EditMemberID.Value && r.FK_ResourceID == (int)EditResourceID.Value);
                        if (existingReservations != null)
                        {
                            // Update only the properties
                            existingReservations.ReservationDate = EditReservationDate.DateTime;
                            existingReservations.Status = ReserveEditStatus.SelectedItem?.ToString();
                        }
                        else
                        {
                            context.Reservations.Add(new Reservations
                            {
                                FK_MemberID = (int)EditMemberID.Value,
                                FK_ResourceID = (int)EditResourceID.Value,
                                ReservationDate = EditReservationDate.DateTime,
                                Status = ReserveEditStatus.SelectedItem?.ToString()
                            });
                        }
                        break;

                    case CRUD.DataType.LibraryBranches:
                        var existingLibraryBranch = context.LibraryBranches.FirstOrDefault(lib => lib.BranchID == (int)LibraryBranchesFields.Tag);
                        if (existingLibraryBranch != null)
                        {
                            // Update only the properties
                            existingLibraryBranch.BranchName = EditBranchName.Text;
                            existingLibraryBranch.Address = EditBranchAddress.Text;
                            existingLibraryBranch.PhoneNumber = EditBranchPhoneNum.Text;
                            existingLibraryBranch.ImageSource = EditBranchImage.Source;
                        }
                        else
                        {
                            context.LibraryBranches.Add(new LibraryBranches
                            {
                                BranchName = EditBranchName.Text,
                                Address = EditBranchAddress.Text,
                                PhoneNumber = EditBranchPhoneNum.Text,
                                ImageSource = EditBranchImage.Source
                            });
                        }
                        break;
                }

                await context.SaveChangesAsync();
            }

            OpenSnackbar("Success", "Data saved successfully.", SymbolRegular.Checkmark24, ControlAppearance.Success);
            CloseEditPanel_Click(sender, e);
            await RefreshDataAsync();
        }
        catch (Exception ex)
        {
            Logger.SaveLog(ex, nameof(SaveEdit_Click), Logger.LogLevel.Error);
            OpenSnackbar("Error", ex.Message, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
        }
    }
    private void ClearFilterPanel()
    {
        MemberFilters.Visibility = Visibility.Collapsed;
        ResourceFilters.Visibility = Visibility.Collapsed;
        ReservationFilters.Visibility = Visibility.Collapsed;

        switch (CRUD.CurrentDataType)
        {
            case CRUD.DataType.Members:
                MemberFilters.Visibility = Visibility.Visible;
                FilterFirstName.Text = "";
                FilterLastName.Text = "";
                FilterNationalCode.Text = "";
                break;

            case CRUD.DataType.Resources:
                ResourceFilters.Visibility = Visibility.Visible;
                FilterResourceName.Text = "";
                FilterCategory.SelectedIndex = -1;
                FilterAuthor.Text = "";
                FilterPublisher.Text = "";
                FilterPublishYear.Text = "";
                FilterISBN.Text = "";
                FilterAvailable.IsChecked = true;
                break;

            case CRUD.DataType.Reservations:
                ReservationFilters.Visibility = Visibility.Visible;
                FilterStartDate.DateTime = DateTime.Now;
                FilterExpiryDate.DateTime = DateTime.Now;
                FilterStatus.SelectedIndex = -1;
                FilterMemberID.Text = "";
                FilterResourceID.Text = "";
                break;
        }
    }
    private void CancelEdit_Click(object sender, RoutedEventArgs e)
    {
        CloseEditPanel_Click(sender, e);
    }
    private async void ShowAddPanel_Click(object sender, RoutedEventArgs e)
    {
        ClearEditPanel();
        await ManagePanelVisibilityAsync(true, EditPanel);
    }
    private async void ShowEditPanel_Click(object sender, RoutedEventArgs e)
    {
        var selectedItem = gridControl.SelectedItem;
        if (selectedItem == null)
        {
            OpenSnackbar("Error", "Please select a record to edit.", SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
            return;
        }

        PopulateEditPanel(selectedItem); // Load data into fields
        await ManagePanelVisibilityAsync(true, EditPanel);
    }

    private async void CloseEditPanel_Click(object sender, RoutedEventArgs e)
    {
        await ManagePanelVisibilityAsync(false, EditPanel);
    }

    private async void ShowFilterPanel_Click(object sender, RoutedEventArgs e)
    {
        ClearFilterPanel();
        await ManagePanelVisibilityAsync(true, FilterPanel);
    }

    private async void CloseFilterPanel_Click(object sender, RoutedEventArgs e)
    {
        await ManagePanelVisibilityAsync(false, FilterPanel);
    }


    private void CancelFilter_Click(object sender, RoutedEventArgs e)
    {
        CloseFilterPanel_Click(sender, e);
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
        _snackbarService.Show(title, message, symbol, controlAppearance);
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