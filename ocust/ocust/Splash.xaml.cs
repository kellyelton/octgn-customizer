using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Octgn.Data;
using VistaDB;
using VistaDB.DDA;

namespace ocust
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    public partial class Splash : Page
    {
        public Splash()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Splash_Loaded);
            this.versionText.Text = string.Format("Version {0}", App.Version.ToString(4));
        }

        private void setstatus(String s)
        {
            Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action
                (
                    delegate()
                    {
                        label2.Content = s;
                    }
                )
            );
        }

        private void Splash_Loaded(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(new ThreadStart(LoadVerifyInstall));
            t.Start();
        }

        private void LoadVerifyInstall()
        {
            Thread.Sleep(2000);
            setstatus("Checking for directory...");
            String opath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Octgn");
            if (!Directory.Exists(opath))
            {
                MessageBox.Show("You must run OCTGN and install a game first!");
                Application.Current.Shutdown();
                return;
            }
            setstatus("Checking for master database...");
            if (!File.Exists(opath + "\\" + "master.vdb3"))
            {
                MessageBox.Show("You must run OCTGN and install a game first!");
                Application.Current.Shutdown();
                return;
            }
            setstatus("Making sure game paths are correct...");
            foreach (Game g in App.GamesRepository.Games)
            {
                if (!File.Exists(g.Filename))
                {
                    MessageBoxResult mb = MessageBox.Show("Game " + g.Name + " can not be found at the location '" + g.Filename + "'. Search for the game file?", "Game Error", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.None);
                    if (mb == MessageBoxResult.Yes)
                    {
                        OpenFileDialog ofd = new OpenFileDialog();
                        ofd.Filter = "Game File (*.o8g)|*.o8g";
                        bool dr = (bool)ofd.ShowDialog();
                        if (dr == true)
                        {
                            string filePath = ofd.FileName;
                            string safeFilePath = ofd.SafeFileName;
                            g.Filename = filePath;
                            ChangeGamePath(g, filePath);
                        }
                        else
                        {
                            mb = MessageBox.Show("Game " + g.Name + " can not be found at the location '" + g.Filename + "'. Delete game file?", "Game Error", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.None);
                            if (mb == MessageBoxResult.Yes)
                            {
                                DeleteGame(g);
                                MessageBox.Show("Game " + g.Name + " Deleted from the DB.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                App.HardShutDown();
                                return;
                            }
                        }
                    }
                    else
                    {
                        mb = MessageBox.Show("Game " + g.Name + " can not be found at the location '" + g.Filename + "'. Delete game file?", "Game Error", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.None);
                        if (mb == MessageBoxResult.Yes)
                        {
                            DeleteGame(g);
                            MessageBox.Show("Game " + g.Name + " Deleted from the DB.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            App.HardShutDown();
                            return;
                        }
                    }
                }
            }
            setstatus("Making sure set files exist...");
            foreach (Game g in App.GamesRepository.Games)
            {
                foreach (Set s in g.Sets)
                {
                    String uri = s.GetPackUri();
                    uri = uri.Replace("pack://file:,,,", "");
                    uri = uri.Replace(',', '\\');

                    if (!File.Exists(uri))
                    {
                        MessageBoxResult mb = MessageBox.Show("Set " + s.Name + " can not be found at the location '" + uri + "'. This set will be deleted. You must go into OCTGN to reinstall this set.", "Game Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None, MessageBoxOptions.None);
                        g.DeleteSet(s);
                    }
                }
            }
            setstatus("Opening program...");
        }

        private void ChangeGamePath(Octgn.Data.Game game, String path)
        {
            String opath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Octgn");
            String masterDbPath = Path.Combine(opath, "master.vdb3"); ;
            using (var dda = VistaDBEngine.Connections.OpenDDA())
            using (var masterDb = dda.OpenDatabase(masterDbPath, VistaDBDatabaseOpenMode.NonexclusiveReadWrite, null))
            using (var gamesTable = masterDb.OpenTable("Game", false, false))
            {
                masterDb.BeginTransaction();
                bool previousCompatibleVersion = false;
                try
                {
                    if (gamesTable.Find("id:'" + game.Id.ToString() + "'", "GamePK", false, false))
                        gamesTable.PutString("filename", path);
                    gamesTable.Post();
                    masterDb.CommitTransaction();
                }
                catch
                {
                    masterDb.RollbackTransaction();
                    throw;
                }
            }
        }

        private void DeleteGame(Octgn.Data.Game game)
        {
            String opath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Octgn");
            String masterDbPath = Path.Combine(opath, "master.vdb3"); ;
            using (var dda = VistaDBEngine.Connections.OpenDDA())
            using (var masterDb = dda.OpenDatabase(masterDbPath, VistaDBDatabaseOpenMode.NonexclusiveReadWrite, null))
            using (var gamesTable = masterDb.OpenTable("Game", false, false))
            {
                masterDb.BeginTransaction();
                try
                {
                    if (gamesTable.Find("id:'" + game.Id.ToString() + "'", "GamePK", false, false))
                    {
                        gamesTable.Delete();
                        foreach (Set s in game.Sets)
                        {
                            game.DeleteSet(s);
                        }
                    }
                }
                catch (Exception e)
                {
                    masterDb.RollbackTransaction();
                }
            }
        }
    }
}