using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wsapm.Wpf.Controls.Extensions;

namespace Wsapm.Wpf.Controls.StatusPanel
{
    /// <summary>
    /// Interaction logic for StatusPanel.xaml
    /// </summary>
    public partial class StatusPanel : UserControl
    {
        private StatusPanelStatus status;
        private StatusPanelStyle statusPanelStyle;

        public StatusPanel()
        {
            DataContext = this;

            InitializeComponent();

            this.DarkColor = Colors.DarkGray;
            this.LightColor = Colors.LightGray;
            this.Status = StatusPanelStatus.Undefined;
            this.StatusPanelStyle = StatusPanelStyle.Windows;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets the status of the StatusPanel.
        /// </summary>
        public StatusPanelStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
                SetColor();
                SetStatus();
            }
        }

        /// <summary>
        /// gets or sets the style of the StatusPanel.
        /// </summary>
        public StatusPanelStyle StatusPanelStyle
        {
            get
            {
                return this.statusPanelStyle;
            }
            set
            {
                this.statusPanelStyle = value;
                SetColor();
                SetStatusImage();
            }
        }

        /// <summary>
        /// Gets or sets a custom status image.
        /// </summary>
        public ImageSource StatusImage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a custom dark color.
        /// </summary>
        public Color DarkColor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a custom light color.
        /// </summary>
        public Color LightColor
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Dependency properties

        /// <summary>
        /// The dependency propertiy 'Content'.
        /// </summary>
        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(StatusPanel), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Content of the control.
        /// </summary>
        public new object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// The dependency property 'Caption'.
        /// </summary>
        public static DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(StatusPanel));

        /// <summary>
        /// Gets or sets the Caption.
        /// </summary>
        public string Caption
        {
            get;
            set;
        }

        #endregion Dependency properties

        #region private methods

        private void SetStatus()
        {
            SetColor();
            SetStatusImage();
        }

        private void SetColor()
        {
            LinearGradientBrush linearGradientBrush = new LinearGradientBrush();
            linearGradientBrush.StartPoint = new System.Windows.Point(1.0, 0.0);

            switch (this.status)
            {
                case StatusPanelStatus.Undefined:
                    // Grey
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.DarkColor = Colors.Gray;
                            this.LightColor = Colors.DarkGray;
                            break;
                        case StatusPanelStyle.Modern:
                            this.DarkColor = Color.FromRgb(255, 169, 169);
                            this.LightColor = this.DarkColor;
                            break;
                        default:
                            this.DarkColor = Colors.Gray;
                            this.LightColor = Colors.DarkGray;
                            break;
                    }
                    break;
                case StatusPanelStatus.OK:
                    // Green
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.DarkColor = Color.FromRgb(22, 118, 20);
                            this.LightColor = Color.FromRgb(65, 178, 69);
                            break;
                        case StatusPanelStyle.Modern:
                            this.DarkColor = Color.FromRgb(96, 169, 23);
                            this.LightColor = this.DarkColor;
                            break;
                        default:
                            this.DarkColor = Color.FromRgb(22, 118, 20);
                            this.LightColor = Color.FromRgb(65, 178, 69);
                            break;
                    }
                    break;
                case StatusPanelStatus.Warning:
                    // Yellow
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.DarkColor = Color.FromRgb(242, 177, 0);
                            this.LightColor = Color.FromRgb(254, 205, 72);
                            break;
                        case StatusPanelStyle.Modern:
                            this.DarkColor = Color.FromRgb(227, 200, 0);
                            this.LightColor = this.DarkColor;
                            break;
                        default:
                            this.DarkColor = Color.FromRgb(242, 177, 0);
                            this.LightColor = Color.FromRgb(254, 205, 72);
                            break;
                    }
                    break;
                case StatusPanelStatus.Error:
                    // Red
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.DarkColor = Color.FromRgb(172, 1, 2);
                            this.LightColor = Color.FromRgb(221, 1, 2);
                            break;
                        case StatusPanelStyle.Modern:
                            this.DarkColor = Color.FromRgb(229, 20, 0);
                            this.LightColor = this.DarkColor;
                            break;
                        default:
                            this.DarkColor = Color.FromRgb(172, 1, 2);
                            this.LightColor = Color.FromRgb(221, 1, 2);
                            break;
                    }
                    break;
                case StatusPanelStatus.Working:
                    // Blue
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.DarkColor = Colors.DarkBlue;
                            this.LightColor = Colors.Blue;
                            break;
                        case StatusPanelStyle.Modern:
                            this.DarkColor = Color.FromRgb(0, 80, 239);
                            this.LightColor = this.DarkColor;
                            break;
                        default:
                            this.DarkColor = Colors.DarkBlue;
                            this.LightColor = Colors.Blue; ;
                            break;
                    }
                    break;
                case StatusPanelStatus.Custom:
                    break;
                default:
                    // Grey.
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.DarkColor = Colors.DarkGray;
                            this.LightColor = Colors.LightGray;
                            break;
                        case StatusPanelStyle.Modern:
                            this.DarkColor = Color.FromRgb(255, 169, 169);
                            this.LightColor = this.DarkColor;
                            break;
                        default:
                            this.DarkColor = Colors.DarkGray;
                            this.LightColor = Colors.LightGray;
                            break;
                    }
                    break;
            }

            linearGradientBrush.GradientStops.Add(new GradientStop(this.DarkColor, 0.0));
            linearGradientBrush.GradientStops.Add(new GradientStop(this.LightColor, 1.0));
            this.rectangleColorStrip.Fill = linearGradientBrush;
        }

        private void SetStatusImage()
        {
            switch (this.status)
            {
                case StatusPanelStatus.Undefined:
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Windows.ToImageSource();
                            break;
                        case StatusPanelStyle.Modern:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Modern.ToImageSource(ImageFormat.Png);
                            break;
                        default:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Modern.ToImageSource(ImageFormat.Png);
                            break;
                    }
                    break;
                case StatusPanelStatus.OK:
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_OK_Windows.ToImageSource();
                            break;
                        case StatusPanelStyle.Modern:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_OK_Modern.ToImageSource(ImageFormat.Png);
                            break;
                        default:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_OK_Modern.ToImageSource(ImageFormat.Png);
                            break;
                    }
                    break;
                case StatusPanelStatus.Warning:
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Warning_Windows.ToImageSource();
                            break;
                        case StatusPanelStyle.Modern:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Warning_Modern.ToImageSource(ImageFormat.Png);
                            break;
                        default:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Warning_Modern.ToImageSource(ImageFormat.Png);
                            break;
                    }
                    break;
                case StatusPanelStatus.Error:
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Error_Windows.ToImageSource();
                            break;
                        case StatusPanelStyle.Modern:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Error_Modern.ToImageSource(ImageFormat.Png);
                            break;
                        default:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Error_Modern.ToImageSource(ImageFormat.Png);
                            break;
                    }
                    break;
                case StatusPanelStatus.Working:
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Working_Windows.ToImageSource();
                            break;
                        case StatusPanelStyle.Modern:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Working_Modern.ToImageSource(ImageFormat.Png);
                            break;
                        default:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Working_Modern.ToImageSource(ImageFormat.Png);
                            break;
                    }
                    break;
                case StatusPanelStatus.Custom:
                    if (this.StatusImage != null)
                        this.imageStatus.Source = this.StatusImage;
                    else
                    {
                        switch (this.StatusPanelStyle)
                        {
                            case StatusPanelStyle.Windows:
                                this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Windows.ToImageSource();
                                break;
                            case StatusPanelStyle.Modern:
                                this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Modern.ToImageSource(ImageFormat.Png);
                                break;
                            default:
                                this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Modern.ToImageSource(ImageFormat.Png);

                                break;
                        }
                    }
                    break;
                default:
                    switch (this.StatusPanelStyle)
                    {
                        case StatusPanelStyle.Windows:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Windows.ToImageSource();
                            break;
                        case StatusPanelStyle.Modern:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Modern.ToImageSource(ImageFormat.Png);
                            break;
                        default:
                            this.imageStatus.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Undefined_Modern.ToImageSource(ImageFormat.Png);
                            break;
                    }
                    break;
            }
        }

        #endregion private methods
    }

    /// <summary>
    /// Enum defining the status of the StatusPanel.
    /// </summary>
    public enum StatusPanelStatus
    {
        /// <summary>
        /// Undefined.
        /// </summary>
        Undefined,
        /// <summary>
        /// OK.
        /// </summary>
        OK,
        /// <summary>
        /// Warning.
        /// </summary>
        Warning,
        /// <summary>
        /// Error.
        /// </summary>
        Error,
        /// <summary>
        /// Working.
        /// </summary>
        Working,
        /// <summary>
        /// Custom status.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Enum defining the styles of the StatusPanl.
    /// </summary>
    public enum StatusPanelStyle
    {
        /// <summary>
        /// Windows style.
        /// </summary>
        Windows,
        /// <summary>
        /// Modern style.
        /// </summary>
        Modern
    }
}
