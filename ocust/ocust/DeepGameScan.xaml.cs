using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ocust.Structure;

namespace ocust
{
    /// <summary>
    /// Interaction logic for DeepGameScan.xaml
    /// </summary>
    public partial class DeepGameScan : Page
    {
        public DeepGameScan()
        {
            InitializeComponent();
        }

        private void echo(String s)
        {
            ListBoxItem li = new ListBoxItem();
            li.Foreground = Brushes.Black;
            li.Content = s;
            listBox1.Items.Add(li);
        }

        private void echo(string s, Brush c)
        {
            ListBoxItem li = new ListBoxItem();
            li.Foreground = c;
            li.Content = s;
            listBox1.Items.Add(li);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;
            listBox1.Items.Clear();
            echo("Game File Location Correct :" + App.Game.Filename, Brushes.Green);
            echo("Set File Locations Correct", Brushes.Green);
            echo("Unzipping game file");
            App.Unzip_Files();

            Octgn.Game g = new Octgn.Game(Octgn.Definitions.GameDef.FromO8G(App.Game.Filename));
            echo("Loading Relationship File...");
            App.Load_Relationships();
            echo("Verifying Relationship file targets...");
            foreach (Relationship r in App.Relationships)
            {
                string checkloc = App.UnzipPath + r.Target.Replace("/", "\\");
                if (File.Exists(checkloc))
                {
                    echo("    " + "Target: " + r.Target + " Exists.", Brushes.Green);
                }
                else
                {
                    echo("    " + "Target: " + r.Target + " Doesn't Exist!", Brushes.Red);
                }
            }
            echo("Done Verifying Relationship File Targets.");
            //XmlStuff x = new XmlStuff();
            //if (x.validateXml(App.UnzipPath + "\\" + App.gameFilePrefix + ".xml"))
            //{
            //    echo(App.gameFilePrefix + ".xml Is valid.");
            //}
            //else
            //{
            //    echo(App.gameFilePrefix + ".xml is corrupt, you should redownload this file.", Brushes.Red);
            //}

            btnStart.IsEnabled = true;
        }
    }
}