using System;
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
            String opath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Octgn");
        }
    }
}