using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Wsapm.Wpf.Controls;
using Wsapm.Wpf.Controls.Extensions;

namespace Wsapm.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionWindow.xaml
    /// </summary>
    public partial class UnhandledExceptionWindow : Window
    {
        private Exception currentException;
        private ProgramInformation programInformation;

        private readonly bool showDetailsFromBeginning;
        private readonly Size expandedSize = new Size(550, 330);
        private readonly Size collapsedSize = new Size(550, 170);

        private const string Separator = "--------------------";
        private const string InnerExceptionTitle = "Inner exception:";

        private UnhandledExceptionWindow()
        {
            InitializeComponent();
        }

        public UnhandledExceptionWindow(Exception exception, ProgramInformation programInformation, string title, ImageSource windowIcon)
            : this(exception, programInformation, title, windowIcon, null, false)
        {
        }

        public UnhandledExceptionWindow(Exception exception, ProgramInformation programInformation, string title, ImageSource windowIcon, ImageSource errorIcon)
            : this(exception, programInformation, title, windowIcon, errorIcon, false)
        {

        }

        public UnhandledExceptionWindow(Exception exception, ProgramInformation programInformation, string title, ImageSource windowIcon, ImageSource errorIcon, bool showDetails)
            : this()
        {
            this.Title = title;

            if (windowIcon != null)
                this.Icon = windowIcon;
            else
                this.Icon = Wsapm.Wpf.Controls.Properties.Resources.Icon_Error_Windows.ToImageSource();

            if (errorIcon != null)
                this.errorIcon.Source = errorIcon;
            else
                this.errorIcon.Source = Wsapm.Wpf.Controls.Properties.Resources.Icon_Error_Windows.ToImageSource();

            this.currentException = exception;
            this.programInformation = programInformation;
            this.showDetailsFromBeginning = showDetails;
            LoadExceptionInfo();

            if (this.showDetailsFromBeginning)
                ExpandDialog();
            else
                CollapseDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Workaround:
            // This has to be done when window is loaded AND again when window's content is rendered.
            if (this.showDetailsFromBeginning)
                ExpandDialog();
            else
                CollapseDialog();
            //SetWindowSize(collapsedSize);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // See comment on Window_Loaded.
            if (this.showDetailsFromBeginning)
                ExpandDialog();
            else
                CollapseDialog();
            //SetWindowSize(collapsedSize);
        }

        private void LoadExceptionInfo()
        {
            if (this.currentException == null)
                LoadExceptionInfo(Wsapm.Wpf.Controls.Resources.Wsapm_Wpf_Controls.UnhandledExceptionWindow_ErrorTitle, string.Empty, string.Empty);
            else
                LoadExceptionInfo(Wsapm.Wpf.Controls.Resources.Wsapm_Wpf_Controls.UnhandledExceptionWindow_ErrorTitle, this.currentException.Message, this.currentException.StackTrace);
        }

        private void LoadExceptionInfo(string caption, string exceptionMessage, string stackTrace)
        {
            this.textBlockCaption.Text = caption;
            this.textBlockMessage.Text = exceptionMessage;
            this.textBlockStackTrace.Text = stackTrace;
        }

        private void SetWindowSize(Size size)
        {
            // Take the preferred size (argument) and add the height of the message text block (in case it's multiline).
            var height = size.Height;
            height += this.textBlockMessage.ActualHeight;

            this.MinWidth = size.Width;
            this.MinHeight = height;
            this.Width = size.Width;
            this.Height = height;
        }

        private void CollapseDialog()
        {
            this.rowDefinitionExtendedInfo.Height = new GridLength(0);
            SetWindowSize(collapsedSize);
            this.buttonDetails.IsChecked = false;
        }

        private void ExpandDialog()
        {
            SetWindowSize(expandedSize);
            this.rowDefinitionExtendedInfo.Height = new GridLength(1, GridUnitType.Star);
            this.buttonDetails.IsChecked = true;
        }

        private void buttonDetails_Click(object sender, RoutedEventArgs e)
        {
            if (this.buttonDetails.IsChecked.Value)
                ExpandDialog();
            else
                CollapseDialog();
        }

        private void buttonSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.OverwritePrompt = true;
            sfd.Filter = Wsapm.Wpf.Controls.Resources.Wsapm_Wpf_Controls.UnhandledExceptionWindow_ErrorExportFilter;
            sfd.FileName = "Report.txt";
            var result = sfd.ShowDialog();

            if (result == true)
                SaveErrorFile(sfd.FileName);
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Saving error file

        private void SaveErrorFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);

                Stream fStream = null;

                try
                {
                    fStream = File.Create(fileName);

                    using (var streamWriter = new StreamWriter(fStream))
                    {
                        fStream = null;
                        streamWriter.Write(BuildErrorFileContent(this.currentException));
                    }
                }
                finally
                {
                    if (fStream != null)
                        fStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Wsapm.Wpf.Controls.Resources.Wsapm_Wpf_Controls.UnhandledExceptionWindow_ErrorExportingErrorFile, fileName, Environment.NewLine, ex.Message), Wsapm.Wpf.Controls.Resources.Wsapm_Wpf_Controls.UnhandledExceptionWindow_ErrorMessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string BuildErrorFileContent(Exception exception)
        {
            if (exception == null)
                return Wsapm.Wpf.Controls.Resources.Wsapm_Wpf_Controls.UnhandledExceptionWindow_ErrorTitle;

            var stringBuilder = new StringBuilder();

            if (this.programInformation != null)
            {
                stringBuilder.Append(this.programInformation.ProgramName);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(this.programInformation.ProgramVersion);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(this.programInformation.ProgramModule);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Separator);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.Append(BuildErrorString(exception));

            return stringBuilder.ToString();
        }

        private string BuildErrorString(Exception exception)
        {
            if (exception == null)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(exception.Message);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(exception.GetType().ToString());
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append(exception.StackTrace);

            if (exception.InnerException != null)
            {
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Separator);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(InnerExceptionTitle);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append(BuildErrorString(exception.InnerException));
            }

            return stringBuilder.ToString();
        }

        #endregion Saving error file
    }
}
