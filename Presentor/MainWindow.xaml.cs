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
using System.Windows.Media.Animation;

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
        public string activeTool = "Текстовая область";
        public System.Windows.Controls.TextBox textArea;
        public Image picture;
        public UIElement widget;
        public string pictureSource = "";
        public List<UIElement> history;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();

        }

        public void Initialize()
        {
            debugger = new SpeechSynthesizer();
            history = new List<UIElement>();
            rawSlides = new List<Dictionary<String, Object>>();
            Dictionary<String, Object> firstSlide = new Dictionary<String, Object>();
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
            slideControlItem.Visibility = Visibility.Collapsed;
            Canvas slideControlItemContent = new Canvas();
            slideControlItemContent.MouseLeftButtonDown += TouchDownHandler;
            slideControlItemContent.MouseMove += ToolMoveHandler;
            slideControlItemContent.MouseLeftButtonUp += ToolResetHandler;
            slideControlItemContent.Background = System.Windows.Media.Brushes.Green;
            slideControlItem.Content = slideControlItemContent;
            slideControl.Items.Add(slideControlItem);

            Dictionary<String, Object> rawSlide = new Dictionary<String, Object>();
            rawSlide.Add("duration", 1);
            rawSlides.Add(rawSlide);

            SelectSlide(slide);
        }

        public void StartSlideFocusHandler(object sender, RoutedEventArgs e)
        {
            isSlideFocus = true;
        }

        public void StopSlideFocusHandler(object sender, RoutedEventArgs e)
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
            backgroundPicker.SelectedColor = ((SolidColorBrush)(activeSlide.Background)).Color;
            durationPicker.Value = TimeSpan.FromHours(((int)(((Dictionary<String, Object>)(rawSlides[currentSlide]))["duration"])));
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

        public void ClearSelection()
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

        public void SetSlideBackground(Color color)
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

        private void SelectToolHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel tool = ((StackPanel)(sender));
            object toolData = tool.DataContext;
            string toolName = toolData.ToString();
            SelectTool(toolName);
        }

        public void SelectTool(string toolName)
        {
            activeTool = toolName;
        }

        private void ToolMoveHandler(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(activeSlide);
            MouseButtonState mouseLeftBtnState = e.LeftButton;
            ToolMove(currentPoint, mouseLeftBtnState);
        }

        public void ToolMove(Point currentPoint, MouseButtonState mouseLeftBtnState)
        {
            MouseButtonState mouseBtnPressed = MouseButtonState.Pressed;
            bool isLeftMouseBtnPressed = mouseLeftBtnState == mouseBtnPressed;
            if (isLeftMouseBtnPressed)
            {
                double coordX = currentPoint.X;
                double coordY = currentPoint.Y;
                bool isTextAreaTool = activeTool == "Текстовая область";
                bool isPictureTool = activeTool == "Картинка";
                if (isTextAreaTool)
                {
                    bool isTextAreaExists = textArea != null;
                    if (isTextAreaExists)
                    {
                        double controlLeft = Canvas.GetLeft(textArea);
                        double controlTop = Canvas.GetTop(textArea);
                        double textAreaWidth = coordX - controlLeft;
                        double textAreaHeight = coordY - controlTop;
                        try
                        {
                            textArea.Width = textAreaWidth;
                            textArea.Height = textAreaHeight;
                        }
                        catch (System.ArgumentException)
                        {

                        }
                    }
                }
                else if (isPictureTool)
                {
                    bool isPictureExists = picture != null;
                    if (isPictureExists)
                    {
                        double controlLeft = Canvas.GetLeft(picture);
                        double controlTop = Canvas.GetTop(picture);
                        double pictureWidth = coordX - controlLeft;
                        double pictureHeight = coordY - controlTop;
                        try
                        {
                            picture.Width = pictureWidth;
                            picture.Height = pictureHeight;
                        }
                        catch (System.ArgumentException)
                        {

                        }
                    }
                }
            }
        }

        private void TouchDownHandler (object sender, MouseButtonEventArgs e)
        {
            Point currentPoint = e.GetPosition(activeSlide);
            TouchDown(currentPoint);
        }

        public void TouchDown (Point currentPoint)
        {
            double coordX = currentPoint.X;
            double coordY = currentPoint.Y;
            bool isTextAreaTool = activeTool == "Текстовая область";
            bool isPictureTool = activeTool == "Картинка";
            if (isTextAreaTool)
            {
                bool isTextAreaNotExists = textArea == null;
                if (isTextAreaNotExists)
                {
                    textArea = new System.Windows.Controls.TextBox();
                    Canvas.SetLeft(textArea, coordX);
                    Canvas.SetTop(textArea, coordY);
                    textArea.Width = 275;
                    textArea.Height = 35;
                    textArea.Background = System.Windows.Media.Brushes.Transparent;
                    textArea.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                    textArea.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    activeSlide.Children.Add(textArea);
                    System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();
                    System.Windows.Controls.MenuItem contextMenuItem = new System.Windows.Controls.MenuItem();
                    contextMenuItem.Header = "Удалить";
                    contextMenuItem.DataContext = ((UIElement)(textArea));
                    contextMenuItem.Click += RemoveControlHandler;
                    contextMenu.Items.Add(contextMenuItem);
                    contextMenuItem = new System.Windows.Controls.MenuItem();
                    contextMenuItem.Header = "Добавить анимацию";
                    contextMenuItem.DataContext = ((UIElement)(textArea));
                    contextMenuItem.Click += AddAnimationHandler;
                    contextMenu.Items.Add(contextMenuItem);
                    textArea.ContextMenu = contextMenu;
                    textArea.GotFocus += ActivateControlHandler;
                    widget = textArea;
                    history.Add(widget);
                }
            }
            else if (isPictureTool)
            {
                bool isPictureExists = picture == null;
                if (isPictureExists)
                {
                    picture = new Image();
                    Canvas.SetLeft(picture, coordX);
                    Canvas.SetTop(picture, coordY);
                    picture.Width = 275;
                    picture.Height = 35;
                    Uri pictureUri = new Uri(pictureSource);
                    picture.Source = new BitmapImage(pictureUri);
                    activeSlide.Children.Add(picture);
                    System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();
                    System.Windows.Controls.MenuItem contextMenuItem = new System.Windows.Controls.MenuItem();
                    contextMenuItem.Header = "Удалить";
                    contextMenuItem.DataContext = ((UIElement)(picture));
                    contextMenuItem.Click += RemoveControlHandler;
                    contextMenu.Items.Add(contextMenuItem);
                    picture.ContextMenu = contextMenu;
                    picture.MouseLeftButtonDown += ActivateControlHandler;
                    widget = picture;
                    history.Add(widget);
                }
            }
        }

        private void ToolResetHandler(object sender, MouseButtonEventArgs e)
        {
            ToolReset();
        }

        public void ToolReset ()
        {
            bool isTextAreaTool = activeTool == "Текстовая область";
            bool isPictureTool = activeTool == "Картинка";
            if (isTextAreaTool)
            {
                bool isTextAreaExists = textArea != null;
                if (isTextAreaExists)
                {
                    textArea.Focus();
                }
                textArea = null;
                widget = null;
            }
            else if (isPictureTool)
            {
                bool isPictureExists = picture != null;
                if (isPictureExists)
                {
                    picture.Focus();
                }
                picture = null;
                widget = null;
            }
        }

        public void ActivateControlHandler(object sender, RoutedEventArgs e)
        {
            bool isTextAreaTool = activeTool == "Текстовая область";
            bool isPictureTool = activeTool == "Картинка";
            if (isTextAreaTool)
            {
                textArea = ((System.Windows.Controls.TextBox)(sender));
                widget = textArea;
            }
            else if (isPictureTool)
            {
                picture = ((Image)(sender));
                widget = picture;
            }
        }

        private void SelectPictureToolHandler(object sender, MouseButtonEventArgs e)
        {
            StackPanel tool = ((StackPanel)(sender));
            object toolData = tool.DataContext;
            string toolName = toolData.ToString();
            SelectPictureTool(toolName);
        }

        public void SelectPictureTool(string toolName)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Выберите картинку";
            ofd.Filter = "Png documents (.png)|*.png";
            bool? res = ofd.ShowDialog();
            bool isOpened = res != false;
            if (isOpened)
            {
                pictureSource = ofd.FileName;
                SelectTool(toolName);
            }
        }

        public void RemoveControlHandler (object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = ((System.Windows.Controls.MenuItem)(sender));
            /*
            DependencyObject menuItemParent = menuItem.Parent;
            System.Windows.Controls.ContextMenu contextMenu = ((System.Windows.Controls.ContextMenu)(menuItemParent));
            DependencyObject contextMenuParent = contextMenu.Parent;
            UIElement control = ((UIElement)(contextMenuParent));
            */
            object menuItemData = menuItem.DataContext;
            UIElement control = ((UIElement)(menuItemData));
            RemoveControl(control);
        }

        public void RemoveControl(UIElement control)
        {
            activeSlide.Children.Remove(control);
        }

        private void AddAnimationHandler(object sender, RoutedEventArgs e)
        {
            AddAnimation();
        }

        public void AddAnimation()
        {
            bool isWidgetExists = widget != null;
            if (isWidgetExists)
            {
                double toValue = 0.0;
                TimeSpan duration = TimeSpan.FromSeconds(3);
                DoubleAnimation animation = new DoubleAnimation(toValue, duration);
                widget.BeginAnimation(UIElement.OpacityProperty, animation);
            }
        }

        private void GlobakHotKeyHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            Key currentKey = e.Key;
            Key zKey = Key.Z;
            bool isZKey = currentKey == zKey;
            bool isCtrlModifier = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            bool isShiftModifier = (Keyboard.Modifiers & ModifierKeys.Shift) > 0;
            bool isUndo = isZKey && isCtrlModifier;
            bool isRedo = isZKey && isCtrlModifier && isShiftModifier;
            if (isRedo)
            {
                // вперед по истории
                int countHistoryRecords = history.Count;
                UIElementCollection slideItems = activeSlide.Children;
                int countSlideItems = slideItems.Count;
                bool isHaveRecords = countHistoryRecords != countSlideItems;
                if (isHaveRecords)
                {
                    int lastWidgetIndex = countHistoryRecords - 1;
                    UIElement control = history[lastWidgetIndex];
                    activeSlide.Children.Add(control);
                    history.RemoveAt(lastWidgetIndex);
                }
            }
            else if(isUndo)
            {
                // откатываемся по истории
                int countSlideItems = activeSlide.Children.Count;
                bool isHaveItems = countSlideItems >= 2;
                if (isHaveItems)
                {
                    int lastWidgetIndex = countSlideItems - 1;
                    UIElement control = activeSlide.Children[lastWidgetIndex];
                    activeSlide.Children.RemoveAt(lastWidgetIndex);
                    history.Add(control);
                }
            }
        }

    }
}
