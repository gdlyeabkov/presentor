using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

using System.Speech.Synthesis;

namespace Presentor
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public int currentSlide = 0;
        public bool isSlideFocus = false;
        public List<Dictionary<String, Object>> rawSlides;
        public bool isAppInit = false;
        public SpeechSynthesizer debugger;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

        }

        public void Initialize()
        {
            debugger = new SpeechSynthesizer();
            rawSlides = new List<Dictionary<String, Object>>();
            Dictionary<String, Object>  firstSlide = new Dictionary<String, Object>();
            firstSlide.Add("duration", 1);
            rawSlides.Add(firstSlide);
            isAppInit = true;
        }

        private void CreateSlideHandler(object sender, RoutedEventArgs e)
        {
            CreateSlide();
        }

        public void CreateSlide()
        {
            Rectangle slide = new Rectangle();
            slide.Fill = System.Windows.Media.Brushes.White;
            slide.Stroke = System.Windows.Media.Brushes.SlateGray;
            slide.Margin = new Thickness(25);
            slide.Height = 100;
            slides.Children.Add(slide);
            slide.MouseLeftButtonDown += SelectSlideHandler;
            slide.MouseEnter += StartSlideFocusHandler;
            slide.MouseLeave += StopSlideFocusHandler;
            TabItem slideControlItem = new TabItem();
            Canvas slideControlItemContent = new Canvas();
            slideControlItemContent.Background = System.Windows.Media.Brushes.Green;
            slideControlItem.Content = slideControlItemContent;
            slideControl.Items.Add(slideControlItem);
            SelectSlide(slide);
            Dictionary<String, Object> rawSlide = new Dictionary<String, Object>();
            rawSlide.Add("duration", 1);
            rawSlides.Add(rawSlide);
        }

        public void StartSlideFocusHandler(object sender, RoutedEventArgs e)
        {
            isSlideFocus = true;
        }

        public void StopSlideFocusHandler (object sender, RoutedEventArgs e)
        {
            isSlideFocus = false;
        }

        private void SelectSlideHandler(object sender, MouseButtonEventArgs e)
        {
            Rectangle slide = ((Rectangle)(sender));
            SelectSlide(slide);
        }

        public void SelectSlide(Rectangle slide)
        {
            ClearSelection();
            slide.StrokeThickness = 5;
            UIElementCollection allSlides = slides.Children;
            currentSlide = allSlides.IndexOf(slide);
            slideControl.SelectedIndex = currentSlide;
            TabItem selectedSlide = ((TabItem)(slideControl.Items[currentSlide]));
            activeSlide = ((Canvas)(selectedSlide.Content));
            UpdateSlideStats(slide, allSlides);
        }

        public void UpdateSlideStats(Rectangle slide, UIElementCollection allSlides)
        {
            int countSlides = allSlides.Count;
            string rawCountSlides = countSlides.ToString();
            int currentSlideNumber = currentSlide + 1;
            string rawCurrentSlideNumber = currentSlideNumber.ToString();
            string slidesStatsLabelContent = "Слайд " + rawCurrentSlideNumber + " из " + rawCountSlides;
            slidesStatsLabel.Text = slidesStatsLabelContent;
        }

        public void ClearSelection ()
        {
            foreach (Rectangle someSlide in slides.Children)
            {
                someSlide.StrokeThickness = 2;
            }
            currentSlide = -1;
        }

        private void ClearSelectionHandler(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bool isOuterFocus = !isSlideFocus;
            if (isOuterFocus)
            {
                ClearSelection();
            }
        }

        private void PlayPresentationHandler(object sender, MouseButtonEventArgs e)
        {
            PlayPresentation();
        }

        public void PlayPresentation()
        {
            PresentationWindow window = new PresentationWindow(slideControl, rawSlides);
            window.Show();
        }

        private void PlayPresentationFromMenuHandler(object sender, RoutedEventArgs e)
        {
            PlayPresentation();
        }

        public void SetSlideBackground (Color color)
        {
            activeSlide.Background = new SolidColorBrush(color);
        }

        private void SetSlideBackgroundHandler(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color? color = e.NewValue;
            bool isColorSelected = color != null;
            if (isColorSelected)
            {
                Color selectedColor = ((Color)(color));
                SetSlideBackground(selectedColor);
            }

        }

        private void SetSlideDurationHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (isAppInit)
            {
                TimeSpan time = ((TimeSpan)(e.NewValue));
                Dictionary<String, Object> rawSlide = rawSlides[currentSlide];
                int timeHours = time.Hours;
                string rawTimeHours = timeHours.ToString();
                rawSlide["duration"] = ((int)(timeHours));
            }
        }

    }
}
