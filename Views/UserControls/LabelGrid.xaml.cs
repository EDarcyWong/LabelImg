using LabelImg.Helpers;
using LabelImg.Models;
using LabelImg.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace LabelImg.Views.UserControls
{
    /// <summary>
    /// LabelGrid.xaml 的交互逻辑
    /// </summary>
    public partial class LabelGrid : UserControl
    {
        private ImageCutWindow cutWindow;
        private bool isShowCutWindow = false;

        private bool isDragging;
        private bool isResizing;
        private bool isAnchorDown;
        private bool isSizeChanging;
        private Point startPoint;
        private Point lastMousePosition;
        private Point lastMousePositionSub;
        private double originalWidth;
        private double originalHeight;
        private string? activeAnchor;
        private BitmapSource cutImgSource;
        private Label currentLabel;
        //private ObservableCollection<CutLabelModel> cutList;
        private double lblMarginOffsetLeft = 0;
        private double lblMarginOffsetTop = 0;
        private bool isCutModel = false;
        private CutLabelModel cutPicModel;
        //private Grid lblCon;
        public LGViewModel viewModel { get; private set; }
        private List<CutLabelModel> oldList = new List<CutLabelModel>();


        public BitmapSource ImageSource
        {
            get { return (BitmapSource)image.Source; }
            set { image.Source = value; }
        }

        public string ImagePath { get; set; }

        public string LabelPath { get; set; }

        public static readonly RoutedEvent ActiveLabelChangedEvent = EventManager.RegisterRoutedEvent(
                "ActiveLabelChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LabelGrid));

        public event RoutedEventHandler ActiveLabelChanged
        {
            add { AddHandler(ActiveLabelChangedEvent, value); }
            remove { RemoveHandler(ActiveLabelChangedEvent, value); }
        }


        protected void OnActiveLabelChanged(CutLabelModel cutLabel)
        {
            ActiveLabelChangedEventArgs newEventArgs = new ActiveLabelChangedEventArgs(ActiveLabelChangedEvent, cutLabel);
            RaiseEvent(newEventArgs);
        }

        public static readonly RoutedEvent LabelDataChangedEvent = EventManager.RegisterRoutedEvent(
            "LabelDataChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof (LabelGrid));

        public event RoutedEventHandler LabelDataChanged
        {
            add { AddHandler(LabelDataChangedEvent, value); }
            remove { RemoveHandler(LabelDataChangedEvent, value); }
        }

        protected void OnLabelDataChanged(List<CutLabelModel> list)
        {
            LabelDataChangedEventArgs newEventArgs = new(LabelDataChangedEvent, list, oldList);
            RaiseEvent(newEventArgs);
        }


        public LabelGrid()
        {
            InitializeComponent();
            viewModel = new LGViewModel();
            this.DataContext = viewModel;
            //viewModel = (LGViewModel)this.DataContext;

            cutWindow = new ImageCutWindow();
            cutWindow.Closed += CutWindow_Closed; // Add event handler for window closed event
            lblMarginOffsetLeft = border.Margin.Left;
            lblMarginOffsetTop = border.Margin.Top;

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public void AddCut(int classIndex, double xCenter=0, double yCenter = 0, double xWeight = 0, double yWeight = 0)
        {
            //深拷贝，避免地址引用。CutLabelModel中含有clone方法。
            oldList = viewModel.CutList.Select(item => item.Clone()).ToList();
            double width = 100;
            double height = 100;
            double marginLeft = 100;
            double marginTop = 100;

            //if ((xCenter > 0 && xCenter < 1) &&
            //    (yCenter > 0 && yCenter < 1) &&
            //    (xWeight > 0 && xWeight <= 1) &&
            //    (yWeight > 0 && yWeight <= 1))
            //{
            //    width = image.ActualWidth * xWeight;
            //    height = image.ActualHeight * yWeight;
            //    marginLeft = xCenter * image.ActualWidth + border.Margin.Left + border.BorderThickness.Left - (width + 4) / 2;
            //    marginTop = yCenter * image.ActualHeight + border.Margin.Top + border.BorderThickness.Top - (height + 4) / 2;
            //}
            // Create a new Label instance
            string name = $"LBL_{RandomStringGenerator.Generate(6)}";
            Label newLabel = addNewLabel(name);
            newLabel.Margin = new Thickness(marginLeft, marginTop, 0, 0);
            newLabel.Width = width - 4;
            newLabel.Height = height - 4;
            newLabel.Content = name;
            newLabel.Template = (ControlTemplate)FindResource("ResizableLabelTemplate2");
            
            foreach (var kvp in lblCon.Children)
            {
                if (kvp is Label)
                {
                    ((Label)kvp).Template = (ControlTemplate)FindResource("ResizableLabelTemplate");
                }
            }

            // Add the new label to the Content control
            lblCon.Children.Add(newLabel);
            CutLabelModel item = new CutLabelModel();
            item.Name = newLabel.Name;
            item.ClassIndex = classIndex;
            item.Width = newLabel.Width;
            item.Height = newLabel.Height;
            //item.Margin = new MarginModel(newLabel.Margin.Left - lblMarginOffsetLeft + newLabel.BorderThickness.Left, newLabel.Margin.Top - lblMarginOffsetTop + newLabel.BorderThickness.Top, newLabel.Margin.Right, newLabel.Margin.Bottom);
            //dataGrid.ItemsSource = null; // 先将数据源设置为 null
            //dataGrid.ItemsSource = cutList; // 然后重新设置为更新后的数据源


            var bitmapSource = image.Source as BitmapSource;
            //if (bitmapSource == null) return;
            // Calculate scale factors
            double scaleX = bitmapSource.PixelWidth / image.ActualWidth;
            double scaleY = bitmapSource.PixelHeight / image.ActualHeight;

            item.XCenter = (newLabel.Margin.Left - border.Margin.Left - border.BorderThickness.Left + item.Width / 2) / image.ActualWidth;
            item.YCenter = (newLabel.Margin.Top - border.Margin.Top - border.BorderThickness.Top + item.Height / 2) / image.ActualHeight;
            item.XWeight = (item.Width - 2 * newLabel.BorderThickness.Left) / image.ActualWidth;
            item.YWeight = (item.Height - 2 * newLabel.BorderThickness.Top) / image.ActualHeight;
            
            viewModel.CutList.Add(item);
            currentLabel = newLabel;
            UpdateImgCut();
        }

        private RoutedEventHandler NewLabelRightHandler(Label newLabel)
        {
            return (s, ev) =>
            {
                //深拷贝，避免地址引用。CutLabelModel中含有clone方法。
                oldList = viewModel.CutList.Select(item => item.Clone()).ToList();
                var itemsToDelete = viewModel.CutList.Where(cut => cut.Name == newLabel.Name).ToList();
                // Remove the items from the collection
                foreach (var item in itemsToDelete)
                {
                    viewModel.CutList.Remove(item);
                }
                lblCon.Children.Remove(newLabel);

                OnLabelDataChanged(viewModel.CutList.ToList());
            };
        }

        public void ActiveLabelByName(string labelName)
        {
            foreach (var kvp in lblCon.Children)
            {
                if (kvp is Label)
                {
                    if (((Label)kvp).Name == labelName)
                    {
                        ActiveLabel((Label)kvp);
                        UpdateImgCut();
                        break;
                    }
                }
            }
        }
        private void ActiveLabel(Label label)
        {
            currentLabel = label;
            lblCon.Children.Remove(label);
            lblCon.Children.Add(label);
            foreach (var kvp in lblCon.Children)
            {
                if (kvp is Label)
                {
                    ((Label)kvp).Template = (ControlTemplate)FindResource("ResizableLabelTemplate");
                }
            }
            label.Template = (ControlTemplate)FindResource("ResizableLabelTemplate2");

            //Debug.WriteLine("activelabel change");
            foreach (var item in viewModel.CutList)
            {
                if (label.Name == item.Name)
                {
                    OnActiveLabelChanged(item);
                    break;
                }
            }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //深拷贝，避免地址引用。CutLabelModel中含有clone方法。
            oldList = viewModel.CutList.Select(item => item.Clone()).ToList();
            currentLabel = (Label)sender;
            if (isAnchorDown)
            {
                isAnchorDown = false;
                originalWidth = currentLabel.ActualWidth;
                originalHeight = currentLabel.ActualHeight;
            }
            if (!isResizing)
            {
                isDragging = true;
                startPoint = e.GetPosition(this);
            }
            ActiveLabel(currentLabel);
            UpdateImgCut();
        }

        //移动Label
        private void Label_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && !isResizing)
            {
                Point currentPoint = e.GetPosition(this);
                double deltaX = currentPoint.X - startPoint.X;
                double deltaY = currentPoint.Y - startPoint.Y;

                double newLeft = currentLabel.Margin.Left + deltaX;
                double newTop = currentLabel.Margin.Top + deltaY;

                double labelBorderThickness = currentLabel.BorderThickness.Left; // Assumes uniform border thickness

                // Ensure the label stays within the image bounds

                newLeft = Math.Max(newLeft, border.Margin.Left + border.BorderThickness.Left - currentLabel.BorderThickness.Left);
                newLeft = Math.Min(newLeft, image.Margin.Left + image.ActualWidth - currentLabel.ActualWidth + currentLabel.BorderThickness.Left + border.Margin.Left + border.BorderThickness.Left);
                newTop = Math.Max(newTop, border.Margin.Top + border.BorderThickness.Top - currentLabel.BorderThickness.Top);
                newTop = Math.Min(newTop, image.Margin.Top + image.ActualHeight - currentLabel.ActualHeight + currentLabel.BorderThickness.Top + border.Margin.Top + border.BorderThickness.Top);

                currentLabel.Margin = new Thickness(newLeft, newTop, 0, 0);
                startPoint = currentPoint;
                //TODO 需要更新cutList数据
                //Debug.WriteLine($"\n\nLabelMove:left,top:{newLeft},{newTop}|| {currentLabel.ActualWidth / image.ActualWidth},{currentLabel.ActualHeight / image.ActualHeight}");
                // Debug.WriteLine($"{currentLabel.Margin.Left + currentLabel.ActualWidth/2+labelBorderThickness-newLeft");
                UpdateImgCut();
            }
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        private void Anchor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isAnchorDown = true;
            isResizing = true;
            isDragging = false; // Disable dragging logic
            activeAnchor = (sender as FrameworkElement)?.Tag.ToString();
            lastMousePosition = e.GetPosition(this);
            lastMousePositionSub = e.GetPosition(this);
        }

        private void Anchor_MouseMove(object sender, MouseEventArgs e)
        {
            // No need to handle MouseMove here
        }

        private void Anchor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isResizing = false;
            isDragging = false;
            activeAnchor = null;
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {

            if (!isResizing) return;

            Point newPosition = e.GetPosition(this);
            double deltaX = newPosition.X - lastMousePosition.X;
            double deltaY = newPosition.Y - lastMousePosition.Y;

            double newWidth = originalWidth;
            double newHeight = originalHeight;
            double newLeft = currentLabel.Margin.Left;
            double newTop = currentLabel.Margin.Top;

            switch (activeAnchor)
            {
                case "TopLeft":
                    newWidth = Math.Max(originalWidth - deltaX, 0);
                    newHeight = Math.Max(originalHeight - deltaY, 0);
                    newLeft = currentLabel.Margin.Left + (newPosition.X - lastMousePositionSub.X);
                    newTop = currentLabel.Margin.Top + (newPosition.Y - lastMousePositionSub.Y);
                    break;
                case "Top":
                    newHeight = Math.Max(originalHeight - deltaY, 0);
                    newTop = currentLabel.Margin.Top + (newPosition.Y - lastMousePositionSub.Y);
                    break;
                case "TopRight":
                    newWidth = Math.Max(originalWidth + deltaX, 0);
                    newHeight = Math.Max(originalHeight - deltaY, 0);
                    newTop = currentLabel.Margin.Top + (newPosition.Y - lastMousePositionSub.Y);
                    break;
                case "Left":
                    newWidth = Math.Max(originalWidth - deltaX, 0);
                    newLeft = currentLabel.Margin.Left + (newPosition.X - lastMousePositionSub.X);
                    break;
                case "Right":
                    newWidth = Math.Max(originalWidth + deltaX, 0);
                    break;
                case "BottomLeft":
                    newWidth = Math.Max(originalWidth - deltaX, 0);
                    newHeight = Math.Max(originalHeight + deltaY, 0);
                    newLeft = currentLabel.Margin.Left + (newPosition.X - lastMousePositionSub.X);
                    break;
                case "Bottom":
                    newHeight = Math.Max(originalHeight + deltaY, 0);
                    break;
                case "BottomRight":
                    newWidth = Math.Max(originalWidth + deltaX, 0);
                    newHeight = Math.Max(originalHeight + deltaY, 0);
                    break;
            }

            double labelBorderThickness = currentLabel.BorderThickness.Left; // Assumes uniform border thickness

            // Ensure the label width and height do not exceed image bounds plus border
            double maxWidth = image.ActualWidth + 2 * currentLabel.BorderThickness.Left;
            double maxHeight = image.ActualHeight + 2 * currentLabel.BorderThickness.Top;
            if (newWidth < 20) newWidth = 20;
            if (newHeight < 20) newHeight = 20;
            newWidth = Math.Min(newWidth, maxWidth);
            newHeight = Math.Min(newHeight, maxHeight);

            // Ensure the label stays within the image bounds
            newLeft = Math.Max(newLeft, image.Margin.Left + border.Margin.Left + border.BorderThickness.Left - currentLabel.BorderThickness.Left);
            newLeft = Math.Min(newLeft, image.Margin.Left + image.ActualWidth - newWidth + border.Margin.Left + border.BorderThickness.Left + currentLabel.BorderThickness.Left);
            newTop = Math.Max(newTop, image.Margin.Top + border.Margin.Top + border.BorderThickness.Top - currentLabel.BorderThickness.Top);
            newTop = Math.Min(newTop, image.Margin.Top + image.ActualHeight - newHeight + border.Margin.Top + border.BorderThickness.Top + currentLabel.BorderThickness.Left);

            // Recalculate newWidth based on the final newLeft
            if (activeAnchor == "TopLeft" || activeAnchor == "Left" || activeAnchor == "BottomLeft")
            {
                newWidth = Math.Max(currentLabel.Margin.Left + currentLabel.Width - newLeft, 20);
            }

            // Recalculate newHeight based on the final newTop
            if (activeAnchor == "TopLeft" || activeAnchor == "Top" || activeAnchor == "TopRight")
            {
                newHeight = Math.Max(currentLabel.Margin.Top + currentLabel.Height - newTop, 20);
            }

            // Ensure newLeft does not change when dragging right-side anchors
            if (activeAnchor == "TopRight" || activeAnchor == "Right" || activeAnchor == "BottomRight")
            {
                newLeft = currentLabel.Margin.Left;
                //Debug.WriteLine($"{newWidth},{image.ActualWidth}-{newLeft},{image.ActualWidth - currentLabel.Margin.Left + currentLabel.BorderThickness.Left}");
                newWidth = Math.Min(newWidth, image.ActualWidth - currentLabel.Margin.Left + currentLabel.BorderThickness.Left + border.Margin.Left + border.BorderThickness.Left);
            }

            // Ensure newTop does not change when dragging bottom-side anchors
            if (activeAnchor == "Bottom" || activeAnchor == "BottomRight" || activeAnchor == "BottomLeft")
            {
                newTop = currentLabel.Margin.Top;
                newHeight = Math.Min(newHeight, image.ActualHeight - currentLabel.Margin.Top + currentLabel.BorderThickness.Top + border.Margin.Top + border.BorderThickness.Top);
            }

            //if (newLeft < (image.Margin.Left - labelBorderThickness))
            //{
            //    newLeft = image.Margin.Left - labelBorderThickness;
            //}
            //if (newTop < (image.Margin.Top - labelBorderThickness))
            //{
            //    newTop = image.Margin.Top - labelBorderThickness;
            //}

            //Debug.WriteLine($"newLeft, newTop: {newLeft}, {newTop}");

            // Update label size and position
            currentLabel.Margin = new Thickness(newLeft, newTop, 0, 0);
            currentLabel.Width = newWidth;
            currentLabel.Height = newHeight;

            // Update the last mouse position
            lastMousePositionSub = newPosition;
            //TODO 需要更新cutList数据
            // Update the RenderTargetBitmap for the resized area
            UpdateImgCut();
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {

            isDragging = false;
            isResizing = false;
            activeAnchor = null;
        }




        private void CutWindow_Closed(object sender, EventArgs e)
        {
            cutWindow = null; // Set cutWindow to null when it is closed
            //if (cbShowCut != null) // Check if cbShowCut is not null to avoid NullReferenceException
            //    cbShowCut.IsChecked = false; // Uncheck the CheckBox when the window is closed

            isShowCutWindow = false;
        }



        private void UpdateImgCut()
        {
            try
            {
                if (currentLabel == null) return;
                if(isSizeChanging)return;

                CutLabelModel currentModel = null;

                foreach (CutLabelModel item in viewModel.CutList)
                {
                    if (item.Name == currentLabel.Name)
                    {
                        if (isResizing)
                        {
                            item.Width = currentLabel.ActualWidth;
                            item.Height = currentLabel.ActualHeight;
                            item.XWeight = (item.Width - 2 * currentLabel.BorderThickness.Left) / image.ActualWidth;
                            item.YWeight = (item.Height - 2 * currentLabel.BorderThickness.Top) / image.ActualHeight;
                        }

                        item.XCenter = (currentLabel.Margin.Left - border.Margin.Left - border.BorderThickness.Left + item.Width / 2) / image.ActualWidth;
                        item.YCenter = (currentLabel.Margin.Top - border.Margin.Top - border.BorderThickness.Top + item.Height / 2) / image.ActualHeight;

                        currentModel = item;
                        break;
                    }
                }

                OnLabelDataChanged(viewModel.CutList.ToList());
                if (isCutModel)
                {
                    if (isResizing )
                    {
                        cutPicModel.Width = currentLabel.ActualWidth;
                        cutPicModel.Height = currentLabel.ActualHeight;
                        cutPicModel.XWeight = (cutPicModel.Width - 2 * currentLabel.BorderThickness.Left) / image.ActualWidth;
                        cutPicModel.YWeight = (cutPicModel.Height - 2 * currentLabel.BorderThickness.Top) / image.ActualHeight;
                    }

                    cutPicModel.XCenter = (currentLabel.Margin.Left - border.Margin.Left - border.BorderThickness.Left + cutPicModel.Width / 2) / image.ActualWidth;
                    cutPicModel.YCenter = (currentLabel.Margin.Top - border.Margin.Top - border.BorderThickness.Top + cutPicModel.Height / 2) / image.ActualHeight;

                    currentModel = cutPicModel;
                }
                BitmapSource target= GetCutLabelBitmapSource(currentModel);
                if (target != null)
                {
                    cutImgSource = target;
                    ShowImageCutWindow(isShowCutWindow);
                }
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine($"ArgumentException: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

        public BitmapSource GetCutApplyBitmapSource()
        {
            return GetCutLabelBitmapSource(cutPicModel);
        }

        private BitmapSource GetCutLabelBitmapSource(CutLabelModel? currentModel )
        {
            var bitmapSource = image.Source as BitmapSource;
            if (bitmapSource == null) return null;

            if (currentModel == null) return null;
            int width = (int)(currentModel.XWeight * bitmapSource.PixelWidth);
            int height = (int)(currentModel.YWeight * bitmapSource.PixelHeight);
            var absoluteX = currentModel.XCenter * bitmapSource.PixelWidth - (currentModel.XWeight * bitmapSource.PixelWidth / 2);
            var absoluteY = currentModel.YCenter * bitmapSource.PixelHeight - (currentModel.YWeight * bitmapSource.PixelHeight / 2);

            //Debug.WriteLine($"{width},{height},{absoluteX},{absoluteY}");
            // Ensure rect is within the image bounds
            if (absoluteX < 0 || absoluteY < 0 || absoluteX + width > bitmapSource.PixelWidth || absoluteY + height > bitmapSource.PixelHeight)
            {
                return null;
            }

            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[height * stride];

            var rect = new Int32Rect((int)absoluteX, (int)absoluteY, width, height);
            bitmapSource.CopyPixels(rect, pixels, stride, 0);

            BitmapSource target = BitmapSource.Create(width, height, bitmapSource.DpiX, bitmapSource.DpiY, bitmapSource.Format, bitmapSource.Palette, pixels, stride);
            return target;
        }

        public void ShowImageCutWindow(bool isShow)
        {
            isShowCutWindow = isShow;
            if (isShowCutWindow == true)
            {
                // Ensure the window is created and available
                if (cutWindow == null || !cutWindow.IsLoaded)
                {
                    cutWindow = new ImageCutWindow();
                    cutWindow.Closed += CutWindow_Closed;
                }

                // If the window is initialized and not visible, show the window
                if (!cutWindow.IsVisible)
                {
                    cutWindow.Show();
                }

                // Set the image source and activate the window if cutImgSource is not null
                if (cutImgSource != null)
                {
                    cutWindow.SetImageSource(cutImgSource);
                    cutWindow.Activate();
                }
            }
            else
            {
                if (cutWindow != null)
                {
                    cutWindow.Close();
                }
            }
        }



        public void OnControlSizeChanged()
        {
            //深拷贝，避免地址引用。CutLabelModel中含有clone方法。
            oldList = viewModel.CutList.Select(item => item.Clone()).ToList();
            isSizeChanging = true;
            double imgW = image.ActualWidth;
            double imgH = image.ActualHeight;

            //Label tmpLbl;
            double scaleX = 1, scaleY = 1;

            for (int i = lblCon.Children.Count - 1; i >= 0; i--)
            {
                if (lblCon.Children[i] is Label)
                {
                    if (((Label)lblCon.Children[i]).Name != "lblCut")
                    {
                        lblCon.Children.RemoveAt(i);
                    }
                }
            }
            foreach (var item in viewModel.CutList)
            {
                Label newLabel = addNewLabel(item.Name, false);
                item.Width = item.XWeight * imgW + 2 * newLabel.BorderThickness.Left;
                item.Height = item.YWeight * imgH + 2 * newLabel.BorderThickness.Top;
                // Set the new width and height of the Label
                newLabel.Width = item.Width;
                newLabel.Height = item.Height;

                double left = imgW * item.XCenter + border.BorderThickness.Left + border.Margin.Left - item.Width / 2;
                double top = imgH * item.YCenter + border.BorderThickness.Top + border.Margin.Top - item.Height / 2;
                Thickness margin = new Thickness(left, top, 0, 0);
                newLabel.Margin = margin;
                lblCon.Children.Add(newLabel);
            }

            if (isCutModel)
            {
                cutPicModel.Width = cutPicModel.XWeight * imgW + 2 * currentLabel.BorderThickness.Left;
                cutPicModel.Height = cutPicModel.YWeight * imgH + 2 * currentLabel.BorderThickness.Top;
                // Set the new width and height of the Label
                currentLabel.Width = cutPicModel.Width;
                currentLabel.Height = cutPicModel.Height;

                double left = imgW * cutPicModel.XCenter + border.BorderThickness.Left + border.Margin.Left - cutPicModel.Width / 2;
                double top = imgH * cutPicModel.YCenter + border.BorderThickness.Top + border.Margin.Top - cutPicModel.Height / 2;
                Thickness margin = new Thickness(left, top, 0, 0);
                currentLabel.Margin = margin;
            }

            UpdateImgCut();
            isSizeChanging = false;
        }

        private Label addNewLabel(string lblName, bool isCutPic=false)
        {
            Label newLabel = new Label
            {
                Name = lblName,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Green, // Transparent,
                FontSize = 10,
                Foreground = Brushes.White,
                BorderBrush = Brushes.Green, // Change the border color as needed
                BorderThickness = new Thickness(2),
                Content = lblName,// $"lbl{RandomStringGenerator.Generate(6)}",
                Cursor = Cursors.SizeAll,
                Template = (ControlTemplate)FindResource("ResizableLabelTemplate") // Apply the same template as the existing label
            };
            if (!isCutPic)
            {
                // Create right-click menu
                ContextMenu contextMenu = new ContextMenu();
                MenuItem deleteItem = new MenuItem { Header = "删除" };
                deleteItem.Click += NewLabelRightHandler(newLabel);
                contextMenu.Items.Add(deleteItem);
                newLabel.ContextMenu = contextMenu;
            }

            // Add event handlers for mouse interaction
            newLabel.MouseDown += Label_MouseDown;
            newLabel.MouseMove += Label_MouseMove;
            newLabel.MouseUp += Label_MouseUp;
            return newLabel;
        }

        private void SaveBitmapSourceAsJpeg(BitmapSource bitmapSource)
        {
            // Create SaveFileDialog
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JPEG files (*.jpg)|*.jpg|All files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Save the image
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                // 如果在设计器模式下，跳过一些代码
                return;
            }
            OnControlSizeChanged();
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            //gdOpt.Visibility = Visibility.Visible;
            DoubleAnimation showAnimation = new DoubleAnimation
            {
                From = 0,
                To = 40, // 这里设置为目标高度，可以根据需要调整
                Duration = TimeSpan.FromSeconds(0.5)
            };
            gdOpt.BeginAnimation(Grid.HeightProperty, showAnimation);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            isDragging = false;
            isResizing = false;
            activeAnchor = null;
            //gdOpt.Visibility = Visibility.Collapsed;
            DoubleAnimation hideAnimation = new DoubleAnimation
            {
                From = 40, // 与显示动画中的目标高度一致
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            gdOpt.BeginAnimation(Grid.HeightProperty, hideAnimation);
        }

        public BitmapSource ResizeImage(BitmapSource source, int width, int height)
        {
            // 创建一个新的RenderTargetBitmap
            RenderTargetBitmap target = new RenderTargetBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Default);

            // 创建一个DrawingVisual并绘制原始图像
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawImage(source, new Rect(0, 0, width, height));
            }

            // 将DrawingVisual渲染到RenderTargetBitmap
            target.Render(visual);

            return target;
        }

        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            isCutModel = true;
            btnCut.Visibility = Visibility.Collapsed;
            btnCutApply.Visibility = Visibility.Visible;
            btnCancelCut.Visibility = Visibility.Visible;
            //Label newLabel = new Label
            //{
            //    Name = $"lblCut",
            //    HorizontalAlignment = HorizontalAlignment.Left,
            //    VerticalAlignment = VerticalAlignment.Top,
            //    Background = Brushes.Green, // Transparent,
            //    FontSize = 10,
            //    Foreground = Brushes.White,
            //    BorderBrush = Brushes.Red, // Change the border color as needed
            //    BorderThickness = new Thickness(2),
            //    Content = $"lblCut",
            //    Cursor = Cursors.SizeAll,
            //    Margin = new Thickness(100, 100, 0, 0), // Set initial margin as needed
            //    Height = 100,
            //    Width = 100,
            //    Template = (ControlTemplate)FindResource("ResizableLabelTemplate2") // Apply the same template as the existing label
            //};

            for (int i = lblCon.Children.Count - 1; i >= 0; i--)
            {
                if (lblCon.Children[i] is Label)
                {
                    if (((Label)lblCon.Children[i]).Name == "lblCut")
                    {
                        return;
                    }
                }
            }

            Label newLabel = addNewLabel("lblCut", true);
            newLabel.Margin = new Thickness(100, 100, 0, 0);
            newLabel.Width = 100;
            newLabel.Height = 100;
            newLabel.Content = "lblCut";
            newLabel.Template = (ControlTemplate)FindResource("ResizableLabelTemplate2");
            newLabel.BorderBrush = Brushes.Red;
            foreach (var kvp in lblCon.Children)
            {
                if (kvp is Label)
                {
                    if(((Label)kvp).Name == "lblCon")
                    {
                        currentLabel = (Label)kvp;
                        return;
                    }
                }
            }
            cutPicModel = new CutLabelModel();
            cutPicModel.Name = newLabel.Name;
            cutPicModel.ClassIndex = -1;
            cutPicModel.Width = newLabel.Width;
            cutPicModel.Height = newLabel.Height;
            //item.Margin = new MarginModel(newLabel.Margin.Left - lblMarginOffsetLeft + newLabel.BorderThickness.Left, newLabel.Margin.Top - lblMarginOffsetTop + newLabel.BorderThickness.Top, newLabel.Margin.Right, newLabel.Margin.Bottom);
            //dataGrid.ItemsSource = null; // 先将数据源设置为 null
            //dataGrid.ItemsSource = cutList; // 然后重新设置为更新后的数据源


            var bitmapSource = image.Source as BitmapSource;
            //if (bitmapSource == null) return;
            // Calculate scale factors
            double scaleX = bitmapSource.PixelWidth / image.ActualWidth;
            double scaleY = bitmapSource.PixelHeight / image.ActualHeight;

            cutPicModel.XCenter = (newLabel.Margin.Left - border.Margin.Left - border.BorderThickness.Left + cutPicModel.Width / 2) / image.ActualWidth;
            cutPicModel.YCenter = (newLabel.Margin.Top - border.Margin.Top - border.BorderThickness.Top + cutPicModel.Height / 2) / image.ActualHeight;
            cutPicModel.XWeight = (cutPicModel.Width - 2 * newLabel.BorderThickness.Left) / image.ActualWidth;
            cutPicModel.YWeight = (cutPicModel.Height - 2 * newLabel.BorderThickness.Top) / image.ActualHeight;

            // Add the new label to the Content control
            lblCon.Children.Add(newLabel);
            currentLabel = newLabel;
        }

        private void btnCutApply_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("裁剪后当前图片的标注数据将会丢失，是否继续？", "提示", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                BitmapSource cutSource = GetCutApplyBitmapSource();
                for (int i = lblCon.Children.Count - 1; i >= 0; i--)
                {
                    if (lblCon.Children[i] is Label)
                    {
                        lblCon.Children.RemoveAt(i);
                    }
                }
                viewModel.CutList.Clear();
                cancelCut();


                image.Source = null;
                 

                File.Delete(ImagePath);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(cutSource));
                using (FileStream fileStream = new FileStream(ImagePath, FileMode.CreateNew))
                {
                    encoder.Save(fileStream);
                }

                //BitmapImage bitmap = new BitmapImage(new Uri(ImagePath));
                //image.Source = bitmap;

                // 读取文件到内存流中
                byte[] imageBytes = File.ReadAllBytes(ImagePath);
                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 确保图片完全加载到内存
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze(); // 确保图片可以跨线程使用

                    // 将图片设置到Image控件
                    image.Source = bitmap;
                }
            }
        }

        private void btnCancelCut_Click(object sender, RoutedEventArgs e)
        {
            cancelCut();
        }

        private void cancelCut()
        {
            isCutModel = false;
            btnCut.Visibility = Visibility.Visible;
            btnCutApply.Visibility = Visibility.Collapsed;
            btnCancelCut.Visibility = Visibility.Collapsed;
            for (int i = lblCon.Children.Count - 1; i >= 0; i--)
            {
                if (lblCon.Children[i] is Label)
                {
                    if (((Label)lblCon.Children[i]).Name == "lblCut")
                    {
                        lblCon.Children.RemoveAt(i);
                    }
                }
            }
        }
    }



    public class ActiveLabelChangedEventArgs : RoutedEventArgs
    {
        public CutLabelModel CutLabel { get; }

        public ActiveLabelChangedEventArgs(RoutedEvent routedEvent, CutLabelModel cutLabel) : base(routedEvent)
        {
            CutLabel = cutLabel;
        }
    }

    public class LabelDataChangedEventArgs : RoutedEventArgs
    {
        public List<CutLabelModel> OldList { get; }
        public List<CutLabelModel> List { get; }

        public LabelDataChangedEventArgs(RoutedEvent routedEvent, List<CutLabelModel> _list, List<CutLabelModel> oldList) : base(routedEvent)
        {
            List = _list;
            OldList = oldList;
        }
    }
}
