// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using LMS.CRM.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Demo.ViewModels;

namespace Wpf.Ui.Demo.Views.Pages;
/// <summary>
/// Interaction logic for BorrowPage.xaml
/// </summary>
public partial class BorrowPage
{
    public BorrowPage()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using (var context = new AppDbContext())
            {

                // Load Members and Resources
                BorrowMember.ItemsSource = context.Members
                    .Select(m => new { m.MemberID, m.FirstName, m.LastName }) // No string formatting here
                    .AsEnumerable() // Move to memory processing
                    .Select(m => new { m.MemberID, FullName = $"{m.FirstName} {m.LastName}" }) // Now apply string interpolation
                    .ToList();

                BorrowResource.ItemsSource = context.Resources
                    .Select(r => new { r.ResourceID, r.Title })
                    .ToList();

                BorrowHistory.ItemsSource = context.BorrowRecords
                    .Join(context.Members, b => b.MemberID, m => m.MemberID, (b, m) => new { b, m })
                    .Join(context.Resources, bm => bm.b.ResourceID, r => r.ResourceID, (bm, r) => new
                    {
                        bm.m.MemberID,
                        bm.m.FirstName,
                        bm.m.LastName,
                        r.ResourceID,
                        r.Title,
                        bm.b.BorrowDate,
                        bm.b.DueDate,
                        bm.b.ReturnDate
                    })
                    .AsEnumerable()
                    .Select(b => new
                    {
                        MemberID = b.MemberID,
                        MemberName = $"{b.FirstName} {b.LastName}",
                        ResourceID = b.ResourceID,
                        ResourceTitle = b.Title,
                        BorrowDate = b.BorrowDate.ToString("yyyy-MM-dd"),
                        DueDate = b.DueDate.ToString("yyyy-MM-dd"),
                        ReturnDate = b.ReturnDate.HasValue ? b.ReturnDate.Value.ToString("yyyy-MM-dd") : "برنگشته"
                    })
                    .ToList();

            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا در بارگذاری اطلاعات: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ConfirmBorrow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using (var context = new AppDbContext())
            {

                if (BorrowMember.SelectedItem == null || BorrowResource.SelectedItem == null)
                {
                    MessageBox.Show("لطفاً یک عضو و یک کتاب انتخاب کنید.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedMember = (dynamic)BorrowMember.SelectedItem;
                var selectedResource = (dynamic)BorrowResource.SelectedItem;

                int memberId = selectedMember.MemberID;
                int resourceId = selectedResource.ResourceID;
                DateTime borrowDate = (DateTime)BorrowDatePicker.EditValue;
                DateTime dueDate = (DateTime)DueDatePicker.EditValue;

                var resource = context.Resources.FirstOrDefault(r => r.ResourceID == resourceId);
                if (resource == null || resource.AvailableCopies < 1)
                {
                    MessageBox.Show("این کتاب در حال حاضر موجود نیست.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var borrowRecord = new BorrowRecords
                {
                    MemberID = memberId,
                    ResourceID = resourceId,
                    BorrowDate = borrowDate,
                    DueDate = dueDate,
                    ReturnDate = null,
                    Fine = 0
                };

                context.BorrowRecords.Add(borrowRecord);
                resource.AvailableCopies--;
                context.SaveChanges();

                MessageBox.Show("واگذاری انجام شد!", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData(); // Refresh Data
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا در ثبت واگذاری: {ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReturnBorrow_Click(object sender, RoutedEventArgs e)
    {
        using (var context = new AppDbContext())
        {
            if (BorrowHistory.SelectedItem is null)
            {
                MessageBox.Show("لطفاً یک مورد را انتخاب کنید.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedRecord = (dynamic)BorrowHistory.SelectedItem;

            if (selectedRecord.ReturnDate != "برنگشته")
            {
                MessageBox.Show("این کتاب قبلاً برگشت داده شده است.", "خطا", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int memberId = selectedRecord.MemberID;
            int resourceId = selectedRecord.ResourceID;
            DateTime borrowDate = DateTime.Parse(selectedRecord.BorrowDate);

            var borrowRecord = context.BorrowRecords.FirstOrDefault(b =>
                b.MemberID == memberId && b.ResourceID == resourceId && b.BorrowDate == borrowDate);

            if (borrowRecord != null)
            {
                borrowRecord.ReturnDate = DateTime.Now;
                context.SaveChanges();
                LoadData();
                MessageBox.Show("کتاب با موفقیت برگشت داده شد.", "موفقیت", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    private void ClearFields_Click(object sender, RoutedEventArgs e)
    {
        BorrowMember.SelectedItem = null;
        BorrowResource.SelectedItem = null;
        BorrowDatePicker.EditValue = null;
        DueDatePicker.EditValue = null;
    }
}