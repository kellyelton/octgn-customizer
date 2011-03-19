using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace ocust
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NavigationWindow
    {
        private static readonly Duration TransitionDuration = new Duration(TimeSpan.FromMilliseconds(500));
        private readonly AnimationTimeline OutAnimation = new DoubleAnimation(0, TransitionDuration);
        private readonly AnimationTimeline InAnimation = new DoubleAnimation(0, 1, TransitionDuration);
        private static readonly object BackTarget = new object();
        private bool isInTransition = false;
        private object transitionTarget;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
            OutAnimation.Completed += delegate
            {
                isInTransition = false;
                if (transitionTarget == BackTarget)
                    GoBack();
                else
                    Navigate(transitionTarget);
            };
            OutAnimation.Freeze();

            Navigating += delegate(object sender, NavigatingCancelEventArgs e)
            {
                if (isInTransition)
                { e.Cancel = true; return; }

                if (transitionTarget != null)
                {
                    transitionTarget = null;
                    return;
                }

                var page = Content as Page;
                if (page == null) return;

                e.Cancel = true;
                isInTransition = true;
                if (e.NavigationMode == NavigationMode.Back)
                    transitionTarget = BackTarget;
                else
                    transitionTarget = e.Content;
                page.BeginAnimation(UIElement.OpacityProperty, OutAnimation, HandoffBehavior.SnapshotAndReplace);
            };

            Navigated += delegate
            {
                var page = Content as Page;
                if (page == null) return;

                page.BeginAnimation(UIElement.OpacityProperty, InAnimation);
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Navigate(new Splash());
        }
    }
}