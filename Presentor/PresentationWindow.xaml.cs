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
                this.Content = slide.Content;
                await Task.Delay(timeout).ConfigureAwait(true);
            }
            this.Close();
        }

    }
}
