using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using Octgn.Data;
using ocust.Structure;

namespace ocust
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static GamesRepository GamesRepository { get; set; }

        public static MainWindow MainWindow { get; set; }

        public static DebugWindow DebugWindow { get; set; }

        public static Game Game { get; set; }

        public static String gameFilePrefix { get; set; }

        public static List<Relationship> Relationships { get; set; }

        private static Thread ochecker;

        public Boolean isOctgnRunning
        {
            get
            {
                //OCTGNwLobby
                //OCTGN
                Process[] processes = Process.GetProcessesByName("OCTGNwLobby");
                if (processes.Length > 0)
                {
                    return true;
                }
                processes = Process.GetProcessesByName("OCTGNwLobby.vhost");
                if (processes.Length > 0)
                {
                    return true;
                }
                processes = Process.GetProcessesByName("OCTGN");
                if (processes.Length > 0)
                {
                    return true;
                }
                processes = Process.GetProcessesByName("OCTGN.vhost");
                if (processes.Length > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public static void SetStatus(String s)
        {
            Splash sp = App.Current.MainWindow.Content as Splash;
            if (sp != null)
            {
                sp.setstatus(s);
            }
        }

        public static void addDebugLine(String s)
        {
#if(DEBUG)
            DebugWindow.AddLine(s);
#endif
        }

        public static Version Version
        {
            get
            {
                Assembly asm = typeof(App).Assembly;
                AssemblyProductAttribute at = (AssemblyProductAttribute)asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
                return asm.GetName().Version;
            }
        }

        public static void HardShutDown()
        {
            Application.Current.Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action
                (
                    delegate()
                    {
                        try
                        {
                            Application.Current.MainWindow.Close();
                        }
                        catch { }
                        Application.Current.Shutdown(0);
                    }
                )
            );
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            /*if (!System.Diagnostics.Debugger.IsAttached)
                AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
                {
                    Exception ex = args.ExceptionObject as Exception;
                    var wnd = new ErrorWindow(ex);
                    wnd.ShowDialog();
                    ErrorLog.WriteError(ex, "Unhandled Exception main", false);
                    if (ErrorLog.CheckandUpload())
                        MessageBox.Show("Uploaded error log.");
                };
            AppDomain.CurrentDomain.ProcessExit += delegate(object sender, EventArgs ea)
            {
                if (ErrorLog.CheckandUpload())
                    MessageBox.Show("Uploaded error log.");
            };
            Updates.PerformHouskeeping();
            */

            string proc = Process.GetCurrentProcess().ProcessName;
            Process[] processes = Process.GetProcessesByName(proc);
            if (processes.Length > 1)
            {
                HardShutDown();
                return;
            }
            if (isOctgnRunning)
            {
                MessageBox.Show("You must shut down OCTGN BEFORE you run this program!");
                HardShutDown();
                return;
            }
            Exit += new ExitEventHandler(App_Exit);
            ochecker = new Thread(new ThreadStart(delegate()
                {
                    int a = 0;
                    while (a == 0)
                    {
                        Thread.Sleep(1000);

                        if (isOctgnRunning)
                        {
                            try
                            {
                                App.Current.Dispatcher.Invoke
                                (
                                    System.Windows.Threading.DispatcherPriority.Normal,
                                    new Action
                                    (
                                        delegate()
                                        {
                                            MainWindow.IsEnabled = false;
                                        }
                                    )
                                );
                                //App.Current.MainWindow.IsEnabled = false;
                                MessageBox.Show("You must shut down OCTGN BEFORE you run this program!");
                            }
                            catch { }
                        }
                        else
                        {
                            App.Current.Dispatcher.Invoke
                            (
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new Action
                                (
                                    delegate()
                                    {
                                        if (!MainWindow.IsEnabled)
                                            MainWindow.IsEnabled = true;
                                    }
                                )
                            );
                        }
                    }
                }
            ));
            ochecker.Start();
            GamesRepository = new Octgn.Data.GamesRepository();
            Relationships = new List<Relationship>();
            DebugWindow = new DebugWindow();
#if(DEBUG)
            DebugWindow.Show();
            App.Current.MainWindow = MainWindow;
#endif
            base.OnStartup(e);
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            ochecker.Abort();
        }
    }
}