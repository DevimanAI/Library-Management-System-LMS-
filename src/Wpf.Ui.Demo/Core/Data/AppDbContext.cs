// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using Windows.UI;
using Wpf.Ui.Demo.ViewModels;

namespace LMS.CRM.Core.Data;

public class AppDbContext : DbContext
{
    public DbSet<Users> Users { get; set; }
    public DbSet<Members> Members { get; set; }
    public DbSet<Resources> Resources { get; set; }
    public DbSet<Reservations> Reservations { get; set; }
    public DbSet<BorrowRecords> BorrowRecords { get; set; }
    public DbSet<LibraryBranches> LibraryBranches { get; set; }

    public AppDbContext() : base("name=AppDbContext")
    {
        Configuration.LazyLoadingEnabled = false;
        //Configuration.ProxyCreationEnabled = false;
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

        // Table configurations
        modelBuilder.Entity<Users>()
            .HasKey(u => u.UserID)
            .Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Users>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        modelBuilder.Entity<BorrowRecords>()
            .HasRequired(br => br.Member)
            .WithMany()
            .HasForeignKey(br => br.MemberID)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<BorrowRecords>()
            .HasRequired(br => br.Resource)
            .WithMany()
            .HasForeignKey(br => br.ResourceID)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<Reservations>()
            .HasRequired(r => r.Member)
            .WithMany()
            .HasForeignKey(r => r.MemberID)
            .WillCascadeOnDelete(false);

        modelBuilder.Entity<Reservations>()
            .HasRequired(r => r.Resource)
            .WithMany()
            .HasForeignKey(r => r.ResourceID)
            .WillCascadeOnDelete(false);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    public static bool IsDatabaseConnected()
    {
        using (var context = new AppDbContext())
        {
            try
            {
                context.Database.Connection.Open();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (context.Database.Connection.State != System.Data.ConnectionState.Closed)
                {
                    context.Database.Connection.Close();
                }
            }
        }
    }



    private static void HandleError(Exception ex)
    {
        if (IsTransientError(ex))
        {
            //MessageBox.Show($"Transient error occurred (e.g., network issue, server restart):\n {ex.Message}", "خطای ارتباط دیتابیس");
            Logger.SaveLog($"{ex.Message}\n{ex.StackTrace}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
        }
        else
        {
            //MessageBox.Show(ex.Message);
            Logger.SaveLog($"{ex.Message}\n{ex.StackTrace}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
        }
    }
    private static bool IsTransientError(Exception ex)
    {
        return ex is TimeoutException || ex is SqlException;
    }




    #region Users
    public static void UpdateUser(Users user)
    {
        using (var context = new AppDbContext())
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Fetch the existing role from the database
                    var existingUser = context.Users
                        .SingleOrDefault(u => u.UserID == user.UserID || u.Username.Trim() == user.Username.Trim());

                    var isNewUser = existingUser == null;

                    if (isNewUser)
                    {
                        Logger.SaveLog($"INSERTING User ID: {user.UserID}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Info);

                        context.Users.Add(user);
                        context.SaveChanges();
                        existingUser = user;
                    }
                    else
                    {
                        Logger.SaveLog($"UPDATING User ID: {existingUser.UserID}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Info);

                        context.Entry(existingUser).CurrentValues.SetValues(user);
                        context.Entry(existingUser).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                    transaction.Commit();
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.SaveLog(dbEx, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                    if (dbEx.InnerException != null)
                    {
                        Logger.SaveLog(dbEx.InnerException, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.SaveLog(ex, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                }
            }
        }
    }
    public static void DeleteUser(Users user)
    {
        using (var context = new AppDbContext())
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var existingUser = context.Users
                        .SingleOrDefault(u => u.UserID == user.UserID);

                    if (existingUser != null)
                    {
                        Logger.SaveLog($"DELETING User ID: {existingUser.UserID}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Info);

                        context.Users.Remove(existingUser);
                        context.SaveChanges();

                        transaction.Commit();
                    }
                    else
                    {
                        Logger.SaveLog($"(Delete User) User Not Found ID: {existingUser.UserID}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Info);
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    Logger.SaveLog(dbEx, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                    if (dbEx.InnerException != null)
                    {
                        Logger.SaveLog(dbEx.InnerException, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.SaveLog(ex, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                }
            }
        }
    }

    public static void ReadAllUsers(out List<Users> users)
    {
        users = null;
        try
        {
            using (var context = new AppDbContext())
            {
                users = context.Users.ToList();
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }
    #endregion

    #region Members
    public static void UpdateMember(Members member)
    {
        using (var context = new AppDbContext())
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var existingMember = context.Members.SingleOrDefault(m => m.MemberID == member.MemberID);
                    if (existingMember == null)
                    {
                        Logger.SaveLog($"INSERTING Member ID: {member.MemberID}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Info);
                        context.Members.Add(member);
                    }
                    else
                    {
                        Logger.SaveLog($"UPDATING Member ID: {existingMember.MemberID}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Info);
                        context.Entry(existingMember).CurrentValues.SetValues(member);
                    }

                    context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.SaveLog(ex, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                }
            }
        }
    }

    public static void DeleteMember(Members member)
    {
        using (var context = new AppDbContext())
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var existingMember = context.Members.SingleOrDefault(m => m.MemberID == member.MemberID);
                    if (existingMember != null)
                    {
                        Logger.SaveLog($"DELETING Member ID: {existingMember.MemberID}", MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Info);
                        context.Members.Remove(existingMember);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Logger.SaveLog(ex, MethodBase.GetCurrentMethod().Name, Logger.LogLevel.Error);
                }
            }
        }
    }

    public static void ReadAllMembers(out List<Members> members)
    {
        members = null;
        try
        {
            using (var context = new AppDbContext())
            {
                members = context.Members.ToList();
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }
    #endregion
}