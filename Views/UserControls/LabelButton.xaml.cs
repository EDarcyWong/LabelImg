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

namespace LabelImg.Views.UserControls
{
    /// <summary>
    /// LabelButton.xaml 的交互逻辑
    /// </summary>
    public partial class LabelButton : UserControl
    {
        public LabelButton()
        {
            InitializeComponent();
            lblLeft.Visibility = Visibility.Collapsed;
            lblTop.Visibility = Visibility.Collapsed;
            lblRight.Visibility = Visibility.Collapsed;
            lblBottom.Visibility = Visibility.Visible;
            ((RotateTransform)lblText.LayoutTransform).Angle = 0;
        }

        // 定义 Alignment 依赖属性
        public static readonly DependencyProperty AlignmentProperty =
            DependencyProperty.Register(
                "Alignment",
                typeof(Alignment),
                typeof(LabelButton),
                new PropertyMetadata(Alignment.Bottom, OnAlignmentChanged));

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(LabelButton), new PropertyMetadata(null, OnContentChanged));

        // Define the Click event
        public event RoutedEventHandler Click;

        // Raise the Click event when the Label is clicked
        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }

        public new object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as LabelButton;
            if (control != null && control.lblText != null)
            {
                control.lblText.Content = e.NewValue;
            }
        }
        public Alignment Alignment
        {
            get { return (Alignment)GetValue(AlignmentProperty); }
            set { SetValue(AlignmentProperty, value); }
        }

        private static void OnAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 在此处理属性变化的逻辑，例如更新控件布局等
            var control = d as LabelButton;
            if (control != null)
            {
                control.UpdateAlignment();
            }
        }

        private void UpdateAlignment()
        {
            // 根据 Alignment 属性更新控件的布局或外观
            switch (Alignment)
            {
                case Alignment.Left:
                    // 更新控件布局为 Left
                    lblLeft.Visibility = Visibility.Visible;
                    lblTop.Visibility = Visibility.Collapsed;
                    lblRight.Visibility = Visibility.Collapsed;
                    lblBottom.Visibility = Visibility.Collapsed;
                    ((RotateTransform)lblText.LayoutTransform).Angle = 90; 
                    break;
                case Alignment.Top:
                    // 更新控件布局为 Top
                    lblLeft.Visibility = Visibility.Collapsed;
                    lblTop.Visibility = Visibility.Visible;
                    lblRight.Visibility = Visibility.Collapsed;
                    lblBottom.Visibility = Visibility.Collapsed;
                    ((RotateTransform)lblText.LayoutTransform).Angle = 0;
                    break;
                case Alignment.Right:
                    // 更新控件布局为 Right
                    lblLeft.Visibility = Visibility.Collapsed;
                    lblTop.Visibility = Visibility.Collapsed;
                    lblRight.Visibility = Visibility.Visible;
                    lblBottom.Visibility = Visibility.Collapsed;
                    ((RotateTransform)lblText.LayoutTransform).Angle = 90;
                    break;
                case Alignment.Bottom:
                    // 更新控件布局为 Bottom
                    lblLeft.Visibility = Visibility.Collapsed;
                    lblTop.Visibility = Visibility.Collapsed;
                    lblRight.Visibility = Visibility.Collapsed;
                    lblBottom.Visibility = Visibility.Visible;
                    ((RotateTransform)lblText.LayoutTransform).Angle = 0;
                    break;
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            int n = 0;
            // 自适应内容宽度
            switch (Alignment)
            {
                case Alignment.Left:n = 1;
                    break;
                case Alignment.Top:n = 0;
                    break;
                case Alignment.Right:n = 1;
                    break;
                case Alignment.Bottom:n= 0;
                    break;

            }
            if (lblText != null)
            {
                if (n == 0)
                {
                    lblText.Width = Double.NaN; // 让Label自适应内容宽度
                    lblText.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    Width = lblText.DesiredSize.Width ;
                }
                else
                {
                    lblText.Height = Double.NaN;
                    lblText.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    Height = lblText.DesiredSize.Height;
                    Width = 30;
                }
                    lblText.HorizontalAlignment = HorizontalAlignment.Center;
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            lblText.Foreground = new SolidColorBrush(Colors.Black);
            lblLeft.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7160E8"));
            lblTop.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7160E8"));
            lblRight.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7160E8"));
            lblBottom.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7160E8"));
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            lblText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF676767"));
            lblLeft.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B6B"));
            lblTop.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B6B"));
            lblRight.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B6B"));
            lblBottom.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B6B"));
        }
    }


    public enum Alignment
    {
        Left,
        Top,
        Right,
        Bottom
    }

}
