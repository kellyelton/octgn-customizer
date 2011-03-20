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
            App.Game = gameSelector.Game;
            //MessageBox.Show(gameSelector.Game.Filename);
        }

        private void GameSelect_Loaded(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(gameSelector.Game.Definition.Name);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            App.Game = gameSelector.Game;
            NavigationService.Navigate(new GameEdit());
        }

        private void btnDeepScan_Click(object sender, RoutedEventArgs e)
        {
            App.Game = gameSelector.Game;
            NavigationService.Navigate(new DeepGameScan());
        }
    }
}