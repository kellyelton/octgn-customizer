using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ocust
{
    /// <summary>
    /// Interaction logic for DebugWindow.xaml
    /// </summary>
    public partial class DebugWindow : Window
    {
        private bool justScrolledToBottom = false;

        public DebugWindow()
        {
            InitializeComponent();
            this.richTextBox1.SetValue(Paragraph.LineHeightProperty, .5);
        }

        public void AddLine(string s)
        {
            Application.Current.Dispatcher.Invoke
            (
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action
                (
                    delegate()
                    {
                        Paragraph p = new Paragraph(new Run(s));
                        richTextBox1.Document.Blocks.Add(p);

                        bool rtbatbottom = true;
                        //check to see if the richtextbox is scrolled to the bottom.
                        //----------------------------------------------------------------------------------
                        double dVer = richTextBox1.VerticalOffset;

                        //get the vertical size of the scrollable content area
                        double dViewport = richTextBox1.ViewportHeight;

                        //get the vertical size of the visible content area
                        double dExtent = richTextBox1.ExtentHeight;

                        if (dVer != 0)
                        {
                            if (dVer + dViewport == dExtent)
                            {
                                rtbatbottom = true;
                                justScrolledToBottom = false;
                            }
                            else
                            {
                                if (!justScrolledToBottom)
                                {
                                    Paragraph pa = new Paragraph();
                                    Run ru = new Run("------------------------------");
                                    ru.Foreground = Brushes.Red;
                                    pa.Inlines.Add(new Bold(ru));
                                    richTextBox1.Document.Blocks.Add(pa);
                                    justScrolledToBottom = true;
                                }
                            }
                        }
                        //----------------------------------------------------------------------------------
                        if (rtbatbottom)
                        {
                            richTextBox1.ScrollToEnd();
                        }
                    }
                )
            );
        }

        private void richTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
    }
}