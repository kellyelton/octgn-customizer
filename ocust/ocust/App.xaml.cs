using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Xml;
using Ionic.Zip;
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

        public static String UnzipPath { get; set; }

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

        public static void Load_Relationships()
        {
            App.Relationships = new List<Relationship>();
            String rpath = Path.Combine(UnzipPath, "_rels");
            if (Directory.Exists(rpath))
            {
                if (File.Exists(Path.Combine(rpath, App.gameFilePrefix + ".xml.rels")))
                {
                    XmlDocument doc = new XmlDocument();
                    XmlTextReader reader = new XmlTextReader(Path.Combine(rpath, App.gameFilePrefix + ".xml.rels"));
                    reader.Read();
                    doc.Load(reader);
                    foreach (XmlNode n in doc.ChildNodes)
                    {
                        if (n.Name.Equals("Relationships"))
                        {
                            foreach (XmlNode n2 in n.ChildNodes)
                            {
                                Relationship r = new Relationship();

                                foreach (XmlAttribute a in n2.Attributes)
                                {
                                    switch (a.Name)
                                    {
                                        case "Type":
                                            r.Type = a.Value;
                                            break;
                                        case "Id":
                                            r.ID = a.Value;
                                            break;
                                        case "Target":
                                            r.Target = a.Value;
                                            break;
                                    }
                                }
                                App.Relationships.Add(r);
                            }
                            break;
                        }
                    }
                }
            }
        }

        public static void Unzip_Files()
        {
            ZipFile file = new ZipFile(App.Game.Filename);
            App.UnzipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "oconfig");
            App.UnzipPath = Path.Combine(App.UnzipPath, "temp");
            Directory.Delete(App.UnzipPath, true);
            Directory.CreateDirectory(App.UnzipPath);
            file.ExtractAll(App.UnzipPath);
            App.addDebugLine("All files extracted to : " + App.UnzipPath);

            Regex r = new Regex(@"^([^\.\[\]]+){1}.xml$");
            App.gameFilePrefix = null;
            foreach (String s in file.EntryFileNames)
            {
                String uri = s;
                try
                {
                    if (App.gameFilePrefix == null)
                    {
                        Match m = r.Match(@uri);
                        if (m.Groups.Count >= 2)
                        {
                            App.gameFilePrefix = m.Groups[1].Value;
                            continue;
                        }
                    }
                }
                catch
                { }
            }
        }

        public static void Save_Game_File()
        {
            Ionic.Zip.ZipFile z = new ZipFile();
            z.AddDirectory(App.UnzipPath);
            try
            {
                z.Save(App.Game.Filename);
            }
            catch (Exception e)
            {
                MessageBox.Show("Please make sure you have Octgn closed first.");
            }
        }

        public static void Save_Relationships()
        {
            String rpath = Path.Combine(App.UnzipPath, "_rels");
            XmlDocument doc = new XmlDocument();
            FileStream f = File.Open(Path.Combine(rpath, App.gameFilePrefix + ".xml.rels"), FileMode.Create, FileAccess.Write, FileShare.Read);
            StreamWriter writer = new StreamWriter(f);

            writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            writer.WriteLine("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            foreach (Relationship r in App.Relationships)
            {
                writer.WriteLine();
                writer.Write("    <Relationship ");
                writer.Write("Target=\"");
                writer.Write(r.Target);
                writer.Write("\" ");
                writer.Write("Id=\"");
                writer.Write(r.ID);
                writer.Write("\" ");
                writer.Write("Type=\"");
                writer.Write(r.Type);
                writer.Write("\" ");
                writer.Write("/>");
            }
            writer.WriteLine("</Relationships>");
            writer.Flush();
            writer.Close();
        }
    }
}