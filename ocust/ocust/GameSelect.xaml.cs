using System.Windows;
using System.Windows.Controls;

namespace ocust
{
    /// <summary>
    /// Interaction logic for GameSelect.xaml
    /// </summary>
    public partial class GameSelect : Page
    {
        public GameSelect()
        {
            InitializeComponent();
            MessageBox.Show(gameSelector.Game.Filename);
        }

        private void GameSelect_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(gameSelector.Game.Definition.Name);
        }
    }
}