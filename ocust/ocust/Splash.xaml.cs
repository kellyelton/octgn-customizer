using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void setstatus(String s)
        {
            label2.Content = s;
        }

        private void Splash_Loaded(object sender, RoutedEventArgs e)
        {
            setstatus("Getting Octgn Master Database...");
            String opath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Octgn");
            if (!Directory.Exists(opath))
            {
                MessageBox.Show("You must run OCTGN and install a game first!");
                Application.Current.Shutdown();
                return;
            }
        }
    }
}