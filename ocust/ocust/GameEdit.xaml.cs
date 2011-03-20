using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ocust
{
    /// <summary>
    /// Interaction logic for GameEdit.xaml
    /// </summary>
    public partial class GameEdit : Page
    {
        BitmapImage bi;

        public GameEdit()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(GameEdit_Loaded);
        }

        private void Load_Images()
        {
            String[] ls = DirSearch(App.UnzipPath, "*.png");
            String[] ls2 = DirSearch(App.UnzipPath, "*.jpg");
            cbImageList.Items.Clear();
            foreach (String s in ls)
            {
                String r = s.Replace(App.UnzipPath, "");
                r = r.Replace('\\', '/');
                cbImageList.Items.Add(r);
                App.addDebugLine(r);
            }
            foreach (String s in ls2)
            {
                String r = s.Replace(App.UnzipPath, "");
                r = r.Replace('\\', '/');
                cbImageList.Items.Add(r);
                App.addDebugLine(r);
            }
        }

        private void getRelationshipTarget(String ID)
        {
        }

        private void getRelationshipID(String Target)
        {
        }

        private void Reload_Stuff()
        {
            App.SetStatus("Getting main file name...");

            Octgn.Game g = new Octgn.Game(Octgn.Definitions.GameDef.FromO8G(App.Game.Filename));
            App.addDebugLine(g.Definition.TableDefinition.background);
            App.Unzip_Files();
            App.SetStatus("Main file name='" + App.gameFilePrefix + "'");
            App.SetStatus("Loading relationships...");
            App.Load_Relationships();
            App.SetStatus("Loading image list...");
            Load_Images();
            String bfile = g.Definition.TableDefinition.background.Replace('/', '\\');
            cbImageList.IsEditable = true;
            cbImageList.IsReadOnly = true;
            cbImageList.Text = bfile;
            String f = cbImageList.Text.Replace('/', '\\');
            f = App.UnzipPath + f;
            Uri uri = new Uri("file://" + f);
            imgBackground.Source = new BitmapImage(uri);
            App.SetStatus("");
        }

        private void GameEdit_Loaded(object sender, RoutedEventArgs e)
        {
            App.SetStatus("Getting main file name...");

            Octgn.Game g = new Octgn.Game(Octgn.Definitions.GameDef.FromO8G(App.Game.Filename));
            App.addDebugLine(g.Definition.TableDefinition.background);
            App.Unzip_Files();
            App.SetStatus("Main file name='" + App.gameFilePrefix + "'");
            App.SetStatus("Loading relationships...");
            App.Load_Relationships();
            App.SetStatus("Loading image list...");
            Load_Images();
            String bfile = g.Definition.TableDefinition.background.Replace('/', '\\');
            cbImageList.IsEditable = true;
            cbImageList.IsReadOnly = true;
            cbImageList.Text = bfile;
            String f = cbImageList.Text.Replace('/', '\\');
            f = App.UnzipPath + f;
            Uri uri = new Uri("file://" + f);
            System.IO.Stream stream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (bi != null)
            {
                bi.StreamSource.Close();
                bi.StreamSource.Dispose();
                bi = null;
            }
            bi = new BitmapImage();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = null;
            bi.BeginInit();
            bi.StreamSource = stream;
            bi.EndInit();
            imgBackground.Source = bi;
            //bi.StreamSource.Close();
            App.SetStatus("");
        }

        private String[] DirSearch(string sDir, string sFile)
        {
            List<String> list = new List<string>();
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, sFile))
                    {
                        list.Add(f);
                    }
                    String[] ls = DirSearch(d, sFile);
                    foreach (String s in ls)
                    {
                        list.Add(s);
                    }
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
            return list.ToArray();
        }

        private void cbImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            String f = (String)e.AddedItems[0].ToString().Replace('/', '\\');
            f = App.UnzipPath + f;
            Uri uri = new Uri("file://" + f);
            System.IO.Stream stream = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (bi != null)
            {
                bi.StreamSource.Close();
                bi.StreamSource.Dispose();
                bi = null;
            }
            bi = new BitmapImage();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.UriSource = null;
            bi.BeginInit();
            bi.StreamSource = stream;
            bi.EndInit();
            imgBackground.Source = bi;
        }

        private void btnAddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image File (*.png, *.jpg)|*.png;*.jpg";
            bool dr = (bool)ofd.ShowDialog();
            if (dr == true)
            {
                string filePath = ofd.FileName;
                string safeFilePath = ofd.SafeFileName;
                string path = App.UnzipPath + "\\images";
                Directory.CreateDirectory(path);
                File.Copy(filePath, path + "\\" + safeFilePath, true);
            }
        }

        private void btnSaveBackground_Click(object sender, RoutedEventArgs e)
        {
            App.SetStatus("Saving...");
            Octgn.Game g = new Octgn.Game(Octgn.Definitions.GameDef.FromO8G(App.Game.Filename));
            for (int i = 0; i < App.Relationships.Count; i++)
            {
                if (App.Relationships[i].Target.Equals(g.Table.Definition.background))
                {
                    App.Relationships[i].Target = cbImageList.Text;
                    break;
                }
            }
            App.Save_Relationships();
            App.Save_Game_File();
            App.SetStatus("");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            bi.StreamSource.Close();
            bi.StreamSource.Dispose();
            imgBackground = null;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false)
            {
                bi.StreamSource.Close();
                bi.StreamSource.Dispose();
                imgBackground = null;
            }
        }
    }
}