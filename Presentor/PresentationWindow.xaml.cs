using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentor
{
    /// <summary>
    /// Логика взаимодействия для PresentationWindow.xaml
    /// </summary>
    public partial class PresentationWindow : Window
    {

        // public StackPanel slides;
        public List<Dictionary<String, Object>> slides;
        public TabControl slideControl;

        // public PresentationWindow(StackPanel slides)
        // public PresentationWindow(List<Dictionary<String, Object>> slides)
        public PresentationWindow(TabControl slideControl, List<Dictionary<String, Object>> slides)
        {
            InitializeComponent();

            Initialize(slideControl, slides);

        }

        public void Initialize (TabControl slideControl, List<Dictionary<String, Object>> slides)
        {
            this.slideControl = slideControl;
            this.slides = slides;
            StartPresentation();
        }

        public async Task StartPresentation()
        {
            foreach (TabItem slide in slideControl.Items)
            {
                int index = slideControl.Items.IndexOf(slide);
                Dictionary<String, Object> rawSlide = slides[index];
                int duration = ((int)(rawSlide["duration"]));
                TimeSpan timeout = TimeSpan.FromSeconds(duration);
                Canvas slideContent = ((Canvas)(slide.Content));
                this.Content = slideContent;
                foreach (UIElement slideContentItem in slideContent.Children)
                {
                    // slideContentItem.Opacity = 0.0;
                    TimeSpan resetAnimationTime = TimeSpan.FromSeconds(0.1);
                    DoubleAnimation resetAnimation = new DoubleAnimation(0.0, resetAnimationTime);
                    slideContentItem.BeginAnimation(UIElement.OpacityProperty, resetAnimation);
                }
                foreach (UIElement slideContentItem in slideContent.Children)
                {
                    object rawControlData = null;
                    bool isTextAreaItem = slideContentItem is TextBox;
                    bool isPictureItem = slideContentItem is Image;
                    if (isTextAreaItem)
                    {
                        TextBox control = slideContentItem as TextBox;
                        control.IsEnabled = false;
                        rawControlData = control.DataContext;
                    }
                    else if (isPictureItem)
                    {
                        Image control = slideContentItem as Image;
                        rawControlData = control.DataContext;
                    }
                    Dictionary<String, Object> controlData = ((Dictionary<String, Object>)(rawControlData));
                    int animationStartDuration = ((int)(controlData["animationStartDuration"]));
                    int animationEndDuration = ((int)(controlData["animationEndDuration"]));
                    bool isStartAnimationEnabled = animationStartDuration >= 1.0;
                    bool isEndAnimationEnabled = animationEndDuration >= 1.0;
                    if (isStartAnimationEnabled)
                    {
                        TimeSpan animationStartTime = TimeSpan.FromSeconds(animationStartDuration);
                        DoubleAnimation startAnimation = new DoubleAnimation(1.0, animationStartTime);
                        slideContentItem.BeginAnimation(UIElement.OpacityProperty, startAnimation);
                    }
                    else
                    {
                        // slideContentItem.Opacity = 1.0;
                        TimeSpan resetAnimationTime = TimeSpan.FromSeconds(0.1);
                        DoubleAnimation resetAnimation = new DoubleAnimation(1.0, resetAnimationTime);
                        slideContentItem.BeginAnimation(UIElement.OpacityProperty, resetAnimation);
                    }
                    if (isEndAnimationEnabled)
                    {
                        int delay = duration - animationEndDuration;
                        TimeSpan delayTime = TimeSpan.FromSeconds(delay);
                        SetTimeOut(delegate
                        {
                            TimeSpan animationEndTime = TimeSpan.FromSeconds(animationEndDuration);
                            DoubleAnimation endAnimation = new DoubleAnimation(0.0, animationEndTime);
                            slideContentItem.BeginAnimation(UIElement.OpacityProperty, endAnimation);
                        }, delayTime);
                    }
                }
                await Task.Delay(timeout).ConfigureAwait(true);
            }
            /*foreach (TabItem slide in slideControl.Items)
            {
                Canvas slideContent = ((Canvas)(slide.Content));
                foreach (UIElement slideContentItem in slideContent.Children)
                {
                    TimeSpan resetAnimationTime = TimeSpan.FromSeconds(0.0);
                    DoubleAnimation resetAnimation = new DoubleAnimation(1.0, resetAnimationTime);
                    slideContentItem.BeginAnimation(UIElement.OpacityProperty, resetAnimation);
                }
            }*/
            this.Close();
        }

        public static async Task SetTimeOut(Action action, TimeSpan timeout)
        {
            await Task.Delay(timeout).ConfigureAwait(true);

            action();
        }

    }
}
