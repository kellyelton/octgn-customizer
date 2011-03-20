using System;
using System.Reflection;
using System.Windows;
using Octgn.Data;

namespace ocust
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static GamesRepository GamesRepository { get; set; }

        public static MainWindow MainWindow { get; set; }

        public static Game Game { get; set; }

        public static String gameFilePrefix { get; set; }

        public static void SetStatus(String s)
        {
            Splash sp = App.Current.MainWindow.Content as Splash;
            if (sp != null)
            {
                sp.setstatus(s);
            }
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
                        Application.Current.MainWindow.Close();
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
            GamesRepository = new Octgn.Data.GamesRepository();
            MainWindow = App.Current.MainWindow as MainWindow;
            base.OnStartup(e);
        }
    }
}