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
using Xceed.Wpf.Toolkit;
using System.IO;
using System.Web.Script.Serialization;

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
            ImageBrush slideBrush = new ImageBrush();
            slide.Fill = slideBrush;
            slide.Stroke = System.Windows.Media.Brushes.SlateGray;
            slide.Margin = new Thickness(25);
            slide.Height = 100;
            slides.Children.Add(slide);
            System.Windows.Controls.ContextMenu slideContextMenu = new System.Windows.Controls.ContextMenu();
            System.Windows.Controls.MenuItem slideContextMenuItem = new System.Windows.Controls.MenuItem();
            slideContextMenuItem.Header = "Удалить слайд";
            slideContextMenuItem.DataContext = slide;
            slideContextMenuItem.Click += RemoveSlideHandler;
            slideContextMenu.Items.Add(slideContextMenuItem);
            slide.ContextMenu = slideContextMenu;
            slide.MouseLeftButtonDown += SelectSlideHandler;
            slide.MouseEnter += StartSlideFocusHandler;
            slide.MouseLeave += StopSlideFocusHandler;
            TabItem slideControlItem = new TabItem();
            slideControlItem.Visibility = Visibility.Collapsed;
            Canvas slideControlItemContent = new Canvas();
            slideControlItemContent.MouseLeftButtonDown += TouchDownHandler;
            slideControlItemContent.MouseMove += ToolMoveHandler;
            slideControlItemContent.MouseLeftButtonUp += ToolResetHandler;
            slideControlItemContent.Background = System.Windows.Media.Brushes.White;
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
            window.DataContext = slideControl;
            window.Closed += RefreshPresentationHandler;
            window.Show();
        }

        public void RefreshPresentationHandler(object sender, EventArgs e)
        {
            RefreshPresentation();
        }

        public void RefreshPresentation()
        {

            foreach (TabItem slideControlItem in slideControl.Items)
            {
                object rawSlideControlItemContent = slideControlItem.Content;
                Canvas slideControlItemContent = ((Canvas)(rawSlideControlItemContent));
                foreach (UIElement slideControlItemContentElement in slideControlItemContent.Children)
                {
                    // slideControlItemContentElement.Opacity = 1.0;
                    TimeSpan resetAnimationTime = TimeSpan.FromSeconds(0.1);
                    DoubleAnimation resetAnimation = new DoubleAnimation(1.0, resetAnimationTime);
                    slideControlItemContentElement.BeginAnimation(UIElement.OpacityProperty, resetAnimation);
                    bool isTextAreaItem = slideControlItemContentElement is System.Windows.Controls.TextBox;
                    if (isTextAreaItem)
                    {
                        System.Windows.Controls.TextBox control = slideControlItemContentElement as System.Windows.Controls.TextBox;
                        slideControlItemContentElement.IsEnabled = true;
                    }
                }
            }
            CreateSlide();
            UIElementCollection allSlides = slides.Children;
            int countSlides = allSlides.Count;
            int lastSlideIndex = countSlides - 1;
            RemoveSlide(lastSlideIndex);
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
                    textArea.ContextMenu = contextMenu;
                    textArea.GotFocus += ActivateControlHandler;
                    widget = textArea;
                    history.Add(widget);
                    Dictionary<String, Object> controlData = new Dictionary<String, Object>();
                    controlData.Add("animationStartDuration", 0);
                    controlData.Add("animationEndDuration", 0);
                    textArea.DataContext = ((Dictionary<String, Object>)(controlData));
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
                    Dictionary<String, Object> controlData = new Dictionary<String, Object>();
                    controlData.Add("animationStartDuration", 0);
                    controlData.Add("animationEndDuration", 0);
                    picture.DataContext = ((Dictionary<String, Object>)(controlData));
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
                // widget = null;
            }
            else if (isPictureTool)
            {
                bool isPictureExists = picture != null;
                if (isPictureExists)
                {
                    picture.Focus();
                }
                picture = null;
                // widget = null;
            }
            try
            {
                RefreshThumbnail();
            }
            catch (Exception)
            {

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
                startAnimationDurationPicker.Value = TimeSpan.FromHours(((int)(((Dictionary<String, Object>)(textArea.DataContext))["animationStartDuration"])));
                endAnimationDurationPicker.Value = TimeSpan.FromHours(((int)(((Dictionary<String, Object>)(textArea.DataContext))["animationEndDuration"])));
            }
            else if (isPictureTool)
            {
                picture = ((Image)(sender));
                widget = picture;
                startAnimationDurationPicker.Value = TimeSpan.FromHours(((int)(((Dictionary<String, Object>)(picture.DataContext))["animationStartDuration"])));
                endAnimationDurationPicker.Value = TimeSpan.FromHours(((int)(((Dictionary<String, Object>)(picture.DataContext))["animationEndDuration"])));
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
            /*bool isWidgetExists = widget != null;
            if (isWidgetExists)
            {
                double toValue = 0.0;
                TimeSpan duration = TimeSpan.FromSeconds(3);
                DoubleAnimation animation = new DoubleAnimation(toValue, duration);
                widget.BeginAnimation(UIElement.OpacityProperty, animation);
            }*/
            Visibility animationBarVisibility = animationBar.Visibility;
            Visibility visible = Visibility.Visible;
            Visibility invisible = Visibility.Collapsed;
            bool isVisible = animationBarVisibility == visible;
            if (isVisible)
            {
                animationBar.Visibility = invisible;
            }
            else
            {
                animationBar.Visibility = visible;
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

        private void OpenPresentationHandler(object sender, MouseButtonEventArgs e)
        {
            OpenPresentation();
        }

        public void OpenPresentation()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Title = "Выберите презентацию для открытия";
            ofd.Filter = "Office Ware Presentor documents (.ptr)|*.ptr";
            bool? res = ofd.ShowDialog();
            bool isOpened = res != false;
            if (isOpened)
            {
                string fullPath = ofd.FileName;
                JavaScriptSerializer js = new JavaScriptSerializer();
                SavedContent savedContent = js.Deserialize<SavedContent>(File.ReadAllText(fullPath));
                ClosePresentation();
                for (int i = 1; i < savedContent.count; i++)
                {
                    CreateSlide();
                }
                for (int i = 0; i < savedContent.count; i++)
                {
                    ItemCollection slideControlItems = slideControl.Items;
                    TabItem slide = ((TabItem)(slideControlItems[i]));
                    object rawSlideContent = slide.Content;
                    Canvas slideContent = ((Canvas)(rawSlideContent));
                    BrushConverter brushConverter = new BrushConverter();
                    List<string> backgrounds = savedContent.backgrounds;
                    string background = backgrounds[i];
                    object rawBrush = brushConverter.ConvertFrom(background);
                    Brush brush = ((Brush)(rawBrush));
                    slideContent.Background = brush;
                }
                for (int i = 0; i < savedContent.count; i++)
                {
                    List<int> durations = savedContent.durations;
                    rawSlides[i]["duration"] = durations[i];
                }
                foreach (Dictionary<String, Object> item in savedContent.items)
                {
                    object cachedX = item["x"];
                    int rawItemX = ((int)(cachedX));
                    string parsedItemX = rawItemX.ToString();
                    double itemX = Double.Parse(parsedItemX);
                    object cachedY = item["y"];
                    int rawItemY = ((int)(cachedY));
                    string parsedItemY = rawItemY.ToString();
                    double itemY = Double.Parse(parsedItemY);
                    object cachedWidth = item["width"];
                    int rawItemWidth = ((int)(cachedWidth));
                    string parsedItemWidth = rawItemWidth.ToString();
                    double itemWidth = Double.Parse(parsedItemWidth);
                    object cachedHeight = item["height"];
                    int rawItemHeight = ((int)(cachedHeight));
                    string parsedItemHeight = rawItemHeight.ToString();
                    double itemHeight = Double.Parse(parsedItemHeight);
                    string itemSource = ((string)(item["source"]));
                    string itemType = ((string)(item["type"]));
                    int itemSlide = ((int)(item["slide"]));
                    object rawItemAnimation = item["animation"];
                    Dictionary<String, Object> itemAnimation = ((Dictionary<String, Object>)(rawItemAnimation));
                    bool isTextAreaItem = itemType == "TextArea";
                    bool isPictureItem = itemType == "Picture";
                    UIElement control = null;
                    Dictionary<String, Object> controlData = new Dictionary<String, Object>();
                    controlData = itemAnimation;
                    Dictionary<String, Object> parsedControlData = ((Dictionary<String, Object>)(controlData));
                    if (isTextAreaItem)
                    {
                        control = new System.Windows.Controls.TextBox();
                        System.Windows.Controls.TextBox element = control as System.Windows.Controls.TextBox;
                        element.Width = itemWidth;
                        element.Height = itemHeight;
                        element.Text = itemSource;
                        element.Background = System.Windows.Media.Brushes.Transparent;
                        element.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                        element.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                        System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();
                        System.Windows.Controls.MenuItem contextMenuItem = new System.Windows.Controls.MenuItem();
                        contextMenuItem.Header = "Удалить";
                        contextMenuItem.DataContext = ((UIElement)(textArea));
                        contextMenuItem.Click += RemoveControlHandler;
                        contextMenu.Items.Add(contextMenuItem);
                        element.ContextMenu = contextMenu;
                        element.GotFocus += ActivateControlHandler;
                        element.DataContext = parsedControlData;
                    }
                    else if (isPictureItem)
                    {
                        control = new Image();
                        Image element = control as Image;
                        element.Width = itemWidth;
                        element.Height = itemHeight; 
                        element.BeginInit();
                        Uri itemSourceUri = new Uri(itemSource);
                        element.Source = new BitmapImage(itemSourceUri);
                        element.EndInit();
                        System.Windows.Controls.ContextMenu contextMenu = new System.Windows.Controls.ContextMenu();
                        System.Windows.Controls.MenuItem contextMenuItem = new System.Windows.Controls.MenuItem();
                        contextMenuItem.Header = "Удалить";
                        contextMenuItem.DataContext = ((UIElement)(textArea));
                        contextMenuItem.Click += RemoveControlHandler;
                        contextMenu.Items.Add(contextMenuItem);
                        element.ContextMenu = contextMenu;
                        element.MouseLeftButtonDown += ActivateControlHandler;
                        element.DataContext = parsedControlData;
                    }
                    ItemCollection slideControlItems = slideControl.Items;
                    object rawSlideControlItem = slideControlItems[itemSlide];
                    TabItem slideControlItem = ((TabItem)(rawSlideControlItem));
                    object rawSlideControlItemContent = slideControlItem.Content;
                    Canvas slideControlItemContent = ((Canvas)(rawSlideControlItemContent));
                    UIElementCollection slideControlItemContentChildren = slideControlItemContent.Children;
                    slideControlItemContentChildren.Add(control);
                    Canvas.SetLeft(control, itemX);
                    Canvas.SetTop(control, itemY);
                }
            }
        }

        private void SavePresentationHandler(object sender, MouseButtonEventArgs e)
        {
            SavePresentation();
        }

        public void SavePresentation ()
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Title = "Выберите файл для сохранения";
            sfd.Filter = "Office Ware Presentor documents (.ptr)|*.ptr";
            bool? res = sfd.ShowDialog();
            bool isSaved = res != false;
            if (isSaved)
            {
                string fullPath = sfd.FileName;
                List<string> backgrounds = new List<string>();
                List<int> durations = new List<int>();
                foreach (TabItem slideControlItem in slideControl.Items)
                {
                    object rawSlideControlItemContent = slideControlItem.Content;
                    Canvas slideControlItemContent = ((Canvas)(rawSlideControlItemContent));
                    Brush slideControlItemContentBackground = slideControlItemContent.Background;
                    string rawSlideControlItemContentBackground = slideControlItemContentBackground.ToString();
                    backgrounds.Add(rawSlideControlItemContentBackground);
                }
                foreach (Dictionary<String, Object> rawSlide in rawSlides)
                {
                    object rawSlideDuration = rawSlide["duration"];
                    int duration = ((int)(rawSlideDuration));
                    durations.Add(duration);
                }
                List<Dictionary<String, Object>> items = new List<Dictionary<String, Object>>();
                foreach (TabItem slideControlItem in slideControl.Items)
                {
                    object rawSlideControlItemContent = slideControlItem.Content;
                    Canvas slideControlItemContent = ((Canvas)(rawSlideControlItemContent));
                    foreach (UIElement slideControlItemContentElement in slideControlItemContent.Children)
                    {
                        bool isTextAreaItem = slideControlItemContentElement is System.Windows.Controls.TextBox;
                        bool isPictureItem = slideControlItemContentElement is Image;
                        string itemType = "";
                        double itemWidth = 0;
                        double itemHeight = 0;
                        string itemSource = "";
                        int itemSlide = slideControl.Items.IndexOf(slideControlItem);
                        Dictionary<String, Object> itemAnimation = null;
                        object rawItemAnimation = null;
                        if (isTextAreaItem)
                        {
                            itemType = "TextArea";
                            System.Windows.Controls.TextBox control = slideControlItemContentElement as System.Windows.Controls.TextBox;
                            itemWidth = ((int)(control.Width));
                            itemHeight = ((int)(control.Height));
                            itemSource = control.Text;
                            rawItemAnimation = control.DataContext;
                        }
                        else if (isPictureItem)
                        {
                            itemType = "Picture";
                            Image control = slideControlItemContentElement as Image;
                            itemWidth = ((int)(control.Width));
                            itemHeight = ((int)(control.Height)); 
                            itemSource = control.Source.ToString();
                            rawItemAnimation = control.DataContext;
                        }
                        itemAnimation = ((Dictionary<String, Object>)(rawItemAnimation));
                        Dictionary<String, Object>  item = new Dictionary<String, Object>();
                        item.Add("x", ((int)(Canvas.GetLeft(slideControlItemContentElement))));
                        item.Add("y", ((int)(Canvas.GetTop(slideControlItemContentElement))));
                        item.Add("width", itemWidth);
                        item.Add("height", itemHeight);
                        item.Add("type", itemType);
                        item.Add("source", itemSource);
                        item.Add("slide", itemSlide);
                        item.Add("animation", itemAnimation);
                        items.Add(item);
                    }
                }
                JavaScriptSerializer js = new JavaScriptSerializer();
                string savedContent = js.Serialize(new SavedContent
                {
                    count = slides.Children.Count,
                    backgrounds = backgrounds,
                    durations = durations,
                    items = items
                });
                File.WriteAllText(fullPath, savedContent);
            }
        }

        private void ClosePresentationHandler(object sender, MouseButtonEventArgs e)
        {
            ClosePresentation();
        }

        public void ClosePresentation()
        {
            int countRecords = history.Count;
            bool isHaveRecords = countRecords >= 1;
            if (isHaveRecords)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Мы заметили что вы не сохранили изменения. Сохранить?", "Внимание", MessageBoxButton.OKCancel);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        SavePresentation();
                        break;
                }
                history.Clear();
                ClosePresentation();
            }
            else
            {
                history.Clear();
                ItemCollection allSlides = slideControl.Items;
                int countSlides = allSlides.Count;
                currentSlide = 0;
                slideControl.SelectedIndex = currentSlide;
                UIElementCollection slideItems = activeSlide.Children;
                int countSlideItems = slideItems.Count;
                for (int i = countSlideItems - 1; i >= 0; i--)
                {
                    UIElement slideItem = slideItems[i];
                    activeSlide.Children.RemoveAt(i);
                }
                for (int i = countSlides - 1; i > 0; i--)
                {
                    slideControl.Items.RemoveAt(i);
                }
                for (int i = countSlides - 1; i > 0; i--)
                {
                    slides.Children.RemoveAt(i);
                }
            }
        }

        private void SetAnimationDurationHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (isAppInit)
            {
                TimeSpanUpDown picker = ((TimeSpanUpDown)(sender));
                object rawPickerData = picker.DataContext;
                string pickerData = rawPickerData.ToString();
                SetAnimationDuration(picker, pickerData);
            }
        }

        public void SetAnimationDuration (TimeSpanUpDown picker, string pickerData)
        {
            TimeSpan? possiblePickerValue = picker.Value;
            bool isDurationSelected = possiblePickerValue != null;
            if (isDurationSelected)
            {
                TimeSpan pickerValue = ((TimeSpan)(possiblePickerValue));
                int duration = pickerValue.Hours;
                bool isStartPickerData = pickerData == "start";
                if (isStartPickerData)
                {
                    bool isTextAreaItem = widget is System.Windows.Controls.TextBox;
                    bool isPictureItem = widget is Image;
                    object controlData = null;
                    if (isTextAreaItem)
                    {
                        System.Windows.Controls.TextBox control = widget as System.Windows.Controls.TextBox;
                        controlData = control.DataContext;
                    }
                    else if (isPictureItem)
                    {
                        Image control = widget as Image;
                        controlData = control.DataContext;
                    }
                    Dictionary<String, Object> parsedControlData = ((Dictionary<String, Object>)(controlData));
                    parsedControlData["animationStartDuration"] = duration;
                    /*if (isTextAreaItem)
                    {
                        System.Windows.Controls.TextBox control = widget as System.Windows.Controls.TextBox;
                        control.DataContext = parsedControlData;
                    }
                    else if (isPictureItem)
                    {
                        Image control = widget as Image;
                        control.DataContext = parsedControlData;
                    }*/
                }
                else
                {
                    bool isTextAreaItem = widget is System.Windows.Controls.TextBox;
                    bool isPictureItem = widget is Image;
                    object controlData = null;
                    if (isTextAreaItem)
                    {
                        System.Windows.Controls.TextBox control = widget as System.Windows.Controls.TextBox;
                        controlData = control.DataContext;
                    }
                    else if (isPictureItem)
                    {
                        Image control = widget as Image;
                        controlData = control.DataContext;
                    }
                    Dictionary<String, Object> parsedControlData = ((Dictionary<String, Object>)(controlData));
                    parsedControlData["animationEndDuration"] = duration;
                    /*if (isTextAreaItem)
                    {
                        System.Windows.Controls.TextBox control = widget as System.Windows.Controls.TextBox;
                        control.DataContext = parsedControlData;
                    }
                    else if (isPictureItem)
                    {
                        Image control = widget as Image;
                        control.DataContext = parsedControlData;
                    }*/
                }
            }
        }

        public void RemoveSlideHandler (object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.MenuItem menuItem = ((System.Windows.Controls.MenuItem)(sender));
            object menuItemData = menuItem.DataContext;
            Rectangle slide = ((Rectangle)(menuItemData));
            int slideIndex = slides.Children.IndexOf(slide);
            UIElementCollection allSlides = slides.Children;
            int slidesCount = allSlides.Count;
            bool isMoreSlides = slidesCount >= 2;
            if (isMoreSlides)
            {
                RemoveSlide(slideIndex);
            }
        }

        public void RemoveSlide (int slideIndex)
        {
            slides.Children.RemoveAt(slideIndex);
            UIElementCollection allSlides = slides.Children;
            UIElement slide = allSlides[0];
            Rectangle firstSlide = ((Rectangle)(slide));
            SelectSlide(firstSlide);
        }

        public void RefreshThumbnail()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)activeSlide.RenderSize.Width, (int)activeSlide.RenderSize.Height, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(activeSlide);
            var crop = new CroppedBitmap(rtb, new Int32Rect(0, 0, ((int)(activeSlide.ActualWidth)), ((int)(activeSlide.ActualHeight))));
            BitmapEncoder imageEncoder = null;
            BitmapFrame frame = BitmapFrame.Create(rtb);
            imageEncoder = new PngBitmapEncoder();
            imageEncoder.Frames.Add(frame);
            Rectangle currentPage = ((Rectangle)(slides.Children[currentSlide]));
            Brush currentPageBrush = currentPage.Fill;
            ImageBrush thumbnail = ((ImageBrush)(currentPageBrush));
            thumbnail.ImageSource = crop;
        }

        private void DetectUpdateSlideHandler(object sender, SelectionChangedEventArgs e)
        {
            UIElementCollection allSlides = slides.Children;
            int countSlides = allSlides.Count;
            bool isMoreSlides = countSlides >= 2;
            if (isMoreSlides)
            {
                try
                {
                    RefreshThumbnail();
                }
                catch (Exception)
                {

                }
            }
        }

        private void WindowLoadedHandler(object sender, RoutedEventArgs e)
        {
            RefreshThumbnail();
        }

    }

    public class SavedContent
    {
        public int count;
        public List<string> backgrounds;
        public List<int> durations;
        public List<Dictionary<String, Object>> items;
    }

}
