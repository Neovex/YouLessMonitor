using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Threading;
using System.Reflection;

namespace YouLessMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _SINGLE_INSTANCE_MUTEX;
        private static bool _DEBUG_LOGGING;

        [STAThread]
        public static void Main()
        {
#if !DEBUG
            Failsafe.Enable();
#endif
            try
            {
                _SINGLE_INSTANCE_MUTEX = new Mutex(true, "{185898E6-6BA9-46F9-A381-0725E6F73321}");
                if (_SINGLE_INSTANCE_MUTEX.WaitOne(100, true))
                {
                    // Setup Logging
                    _DEBUG_LOGGING = File.Exists("debug");
                    Log.OnLog += LogHandler;
                    LogHandler($"{Environment.NewLine}{String.Concat(Enumerable.Repeat('#', 140))}{Environment.NewLine}", LogLevel.Info);
                    Log.Info("New Session");

                    // Start Application
                    var app = new App();
#if !DEBUG
                    Failsafe.Observe(app);
#endif
                    app.InitializeComponent();
                    app.Run();
                    _SINGLE_INSTANCE_MUTEX.ReleaseMutex();
                }
                else
                {
                    MessageBox.Show("Application is already running!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "MAIN ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.ToString(), "MAIN ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Log.Fatal(ex);
                Log.Fatal(ex.InnerException);
                throw;
            }
        }

        private static void LogHandler(string msg, LogLevel lvl)
        {
            if (_DEBUG_LOGGING || lvl >= LogLevel.Info)
            {
                File.AppendAllText("log.txt", msg + Environment.NewLine);
            }
        }



        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info("Shutdown", e.ApplicationExitCode);
            base.OnExit(e);
        }
    }
}