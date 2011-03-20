using System.Windows;
using System.Windows.Controls;

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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            btnStart.IsEnabled = false;

            listBox1.Items.Add("Game File Location Correct :" + App.Game.Filename);
            listBox1.Items.Add("Set File Locations Correct");

            btnStart.IsEnabled = true;
        }
    }
}