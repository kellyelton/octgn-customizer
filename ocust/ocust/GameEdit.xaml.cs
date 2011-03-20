using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;
using Ionic.Zip;
using Microsoft.Win32;
using ocust.Structure;

namespace ocust
{
    /// <summary>
    /// Interaction logic for GameEdit.xaml
    /// </summary>
    public partial class GameEdit : Page
    {
        private String UnzipPath { get; set; }

        public GameEdit()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(GameEdit_Loaded);
        }

        private void Unzip_Files()
        {
            ZipFile file = new ZipFile(App.Game.Filename);
            UnzipPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "oconfig");
            UnzipPath = Path.Combine(UnzipPath, "temp");
            Directory.Delete(UnzipPath, true);
            Directory.CreateDirectory(UnzipPath);
            file.ExtractAll(UnzipPath);
            App.addDebugLine("All files extracted to : " + UnzipPath);

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

        private void Load_Images()
        {
            String[] ls = DirSearch(UnzipPath, "*.png");
            String[] ls2 = DirSearch(UnzipPath, "*.jpg");
            cbImageList.Items.Clear();
            foreach (String s in ls)
            {
                String r = s.Replace(UnzipPath, "");
                r = r.Replace('\\', '/');
                cbImageList.Items.Add(r);
                App.addDebugLine(r);
            }
            foreach (String s in ls2)
            {
                String r = s.Replace(UnzipPath, "");
                r = r.Replace('\\', '/');
                cbImageList.Items.Add(r);
                App.addDebugLine(r);
            }
        }

        private void Load_Relationships()
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

        private void Save_Game_File()
        {
            Ionic.Zip.ZipFile z = new ZipFile();
            z.AddDirectory(UnzipPath);
            try
            {
                z.Save(App.Game.Filename);
            }
            catch (Exception e)
            {
                MessageBox.Show("Please make sure you have Octgn closed first.");
            }
        }

        private void Save_Relationships()
        {
            String rpath = Path.Combine(UnzipPath, "_rels");
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
            Unzip_Files();
            App.SetStatus("Main file name='" + App.gameFilePrefix + "'");
            App.SetStatus("Loading relationships...");
            Load_Relationships();
            App.SetStatus("Loading image list...");
            Load_Images();
            String bfile = g.Definition.TableDefinition.background.Replace('/', '\\');
            cbImageList.IsEditable = true;
            cbImageList.IsReadOnly = true;
            cbImageList.Text = bfile;
            String f = cbImageList.Text.Replace('/', '\\');
            f = UnzipPath + f;
            Uri uri = new Uri("file://" + f);
            imgBackground.Source = new BitmapImage(uri);
            App.SetStatus("");
        }

        private void GameEdit_Loaded(object sender, RoutedEventArgs e)
        {
            App.SetStatus("Getting main file name...");

            Octgn.Game g = new Octgn.Game(Octgn.Definitions.GameDef.FromO8G(App.Game.Filename));
            App.addDebugLine(g.Definition.TableDefinition.background);
            Unzip_Files();
            App.SetStatus("Main file name='" + App.gameFilePrefix + "'");
            App.SetStatus("Loading relationships...");
            Load_Relationships();
            App.SetStatus("Loading image list...");
            Load_Images();
            String bfile = g.Definition.TableDefinition.background.Replace('/', '\\');
            cbImageList.IsEditable = true;
            cbImageList.IsReadOnly = true;
            cbImageList.Text = bfile;
            String f = cbImageList.Text.Replace('/', '\\');
            f = UnzipPath + f;
            Uri uri = new Uri("file://" + f);
            imgBackground.Source = new BitmapImage(uri);
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
            f = UnzipPath + f;
            Uri uri = new Uri("file://" + f);
            imgBackground.Source = new BitmapImage(uri);
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
                string path = UnzipPath + "\\images";
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
            Save_Relationships();
            Save_Game_File();
            App.SetStatus("");
        }
    }
}