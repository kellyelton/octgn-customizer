using System;
using System.IO.Packaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ocust
{
    /// <summary>
    /// Interaction logic for GameEdit.xaml
    /// </summary>
    public partial class GameEdit : Page
    {
        public GameEdit()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(GameEdit_Loaded);
        }

        private void GameEdit_Loaded(object sender, RoutedEventArgs e)
        {
            App.SetStatus("Getting main file name...");
            Package p = ZipPackage.Open(App.Game.Filename);
            Regex r = new Regex(@"^/([^/.]+){1}.xml$");
            StringBuilder sb = new StringBuilder();

            foreach (PackagePart pp in p.GetParts())
            {
                String uri = pp.Uri.ToString();
                try
                {
                    Match m = r.Match(@uri);
                    if (m.Groups.Count >= 2)
                    {
                        App.gameFilePrefix = m.Groups[1].Value;
                    }
                }
                catch
                { }
            }
            App.SetStatus("Main file name='" + App.gameFilePrefix + "'");
        }
    }
}