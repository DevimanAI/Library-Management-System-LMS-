// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using LMS.CRM.Core.Data;

namespace LMS.CRM.Core
{
    public class Logger
    {
        private System.Timers.Timer mainTimer;
        public static StringBuilder DataToWrite = new StringBuilder();
        public static Users ActiveUser = null;
        private static string LogBaseDirectory;
        private static string LogPath;

        public enum LogLevel
        {
            Events,
            Info,
            Warning,
            Error,
        }

        public Logger(Users user)
        {
            ActiveUser = user;

            LogBaseDirectory = AppDomain.CurrentDomain.BaseDirectory + "/logs/";
            LogPath = $"{AppDomain.CurrentDomain.BaseDirectory}/logs/Log_{DateTime.Now.ToString("yyyyMMdd")}.txt";

            mainTimer = new System.Timers.Timer(10000);
            mainTimer.AutoReset = true;
            mainTimer.Elapsed += new System.Timers.ElapsedEventHandler(mainTimer_Tick);
            mainTimer.Start();
        }


        private void mainTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!string.IsNullOrEmpty(DataToWrite.ToString()))
            {
                CheckDirectory();
                File.AppendAllText(LogPath, DataToWrite.ToString());
                DataToWrite.Clear();
            }
        }

        private static void CheckDirectory()
        {
            if (!Directory.Exists(LogBaseDirectory))
                Directory.CreateDirectory(LogBaseDirectory);
        }

        public static void SaveLog(string str, string methodName, LogLevel level)
        {
            //try
            //{
            //    if (level == LogLevel.Events)
            //    {
            //        var log = new AppLogs()
            //        {
            //            Timestamp = DateTime.Now,
            //            User = ActiveUser.Username,
            //            Message = str,
            //        };

            //        AppDbContext.InsertLog(log);
            //    }
            //}
            //catch (Exception exc)
            //{
            //    CheckDirectory();
            //    File.AppendAllText(LogPath, $"{DateTime.Now} SaveLog ({level}): {exc.Message}{Environment.NewLine}");
            //}

            try
            {
                DataToWrite.Append($"{DateTime.Now.ToString()} {methodName} ({level}): {str} {Environment.NewLine}");
            }
            catch (Exception exc)
            {
                CheckDirectory();
                File.AppendAllText(LogPath, $"{DateTime.Now} SaveLog ({level}): {exc.Message}\n{exc.StackTrace}{Environment.NewLine}");
            }
        }

        public static void SaveLog(Exception exc, string methodName, LogLevel level)
        {
            try
            {
                DataToWrite.Append($"{DateTime.Now.ToString()} {methodName} ({level}): {exc.Message}\n{exc.StackTrace} {Environment.NewLine}");
            }
            catch (Exception ex)
            {
                CheckDirectory();
                File.AppendAllText(LogPath, $"{DateTime.Now} SaveLog ({level}): {ex.Message}\n{ex.StackTrace}{Environment.NewLine}");
            }
        }
    }
}
