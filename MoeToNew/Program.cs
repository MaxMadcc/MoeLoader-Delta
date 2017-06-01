﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace MoeLoader
{
    class Program
    {
        private const string necessaryDll = "HtmlTextBlock.dll";

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [STAThread()]
        [DebuggerNonUserCode()]
        public static void Main(string[] args)
        {
            bool isRuned;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "MoeToNewΔ", out isRuned);
            if (isRuned)
            {
                RecoveryConfig();
                ServicePointManager.DefaultConnectionLimit = 100;
                ServicePointManager.Expect100Continue = false;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                App app = new App();
                app.InitializeComponent();
                app.Run();
                mutex.ReleaseMutex();
            }
            else
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// 恢复必要文件
        /// </summary>
        private static void RecoveryConfig()
        {
            if (!File.Exists(necessaryDll))
                File.WriteAllBytes(necessaryDll, Properties.Resources.HtmlTextBlockDLL);
        }
    }
}
