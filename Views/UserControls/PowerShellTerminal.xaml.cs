using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// PowerShellTerminal.xaml 的交互逻辑
    /// </summary>
    public partial class PowerShellTerminal : UserControl
    {
        private Process powerShellProcess;
        private StringBuilder outputBuilder;
        private int promptLength;
        private string prompt = "";

        public PowerShellTerminal()
        {
            InitializeComponent();
            StartPowerShellProcess();
            terminalTextBox.Text = prompt;
            terminalTextBox.CaretIndex = terminalTextBox.Text.Length;
            promptLength = terminalTextBox.Text.Length;
        }

        private void StartPowerShellProcess()
        {
            outputBuilder = new StringBuilder();

            powerShellProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            powerShellProcess.OutputDataReceived += (sender, args) => AppendOutput(args.Data);
            powerShellProcess.ErrorDataReceived += (sender, args) => AppendOutput(args.Data);

            powerShellProcess.Start();
            powerShellProcess.BeginOutputReadLine();
            powerShellProcess.BeginErrorReadLine();
        }

        private void AppendOutput(string data)
        {
            if (data != null)
            {
                Dispatcher.Invoke(() =>
                {
                    outputBuilder.AppendLine(data);
                    terminalTextBox.Text += data + Environment.NewLine + prompt;
                    terminalTextBox.CaretIndex = terminalTextBox.Text.Length;
                    terminalTextBox.ScrollToEnd();
                    promptLength = terminalTextBox.Text.Length;
                });
            }
        }

        private async void TerminalTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string input = terminalTextBox.Text.Substring(promptLength).Trim();
                terminalTextBox.Text += Environment.NewLine;

                await Task.Run(() =>
                {
                    powerShellProcess.StandardInput.WriteLine(input);
                    powerShellProcess.StandardInput.Flush();
                });

                e.Handled = true;
                promptLength = terminalTextBox.Text.Length;
            }
            else if (e.Key == Key.Back)
            {
                if (terminalTextBox.CaretIndex <= prompt.Length)
                {
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Left || e.Key == Key.Home)
            {
                if (terminalTextBox.CaretIndex <= prompt.Length)
                {
                    e.Handled = true;
                }
            }
        }


        private void TerminalTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            terminalTextBox.ScrollToEnd();
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (powerShellProcess != null && !powerShellProcess.HasExited)
            {
                powerShellProcess.Kill();
                powerShellProcess.Dispose();
            }
        }
    }
}
