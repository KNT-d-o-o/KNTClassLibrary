using KNTInstaller.Model;
using KNTInstaller.Repository;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace KNTInstaller;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    InstallRepository _installerRepository = new InstallRepository();
    
    public MainWindow()
    {
        InitializeComponent();

        richTextBoxLog.Document.Blocks.Clear(); // TODO, clear empty spaces 

        SetInstalledUi();
    }

    void SetInstalledUi()
    {    
        if (_installerRepository.IsIisInstalled())
            checkBox_Common_IIS.Foreground = new SolidColorBrush(Colors.Red);
        if (_installerRepository.IsDotnetHostingInstalled())
            checkBox_Common_DotNetHosting.Foreground = new SolidColorBrush(Colors.Red);
        if (_installerRepository.IsEdgeInstalled())
            checkBox_Common_Edge.Foreground = new SolidColorBrush(Colors.Red);

        if (_installerRepository.IsSMMInstalled())
            checkBox_SMM.Foreground = new SolidColorBrush(Colors.Red);        
        if(_installerRepository.IsSMMFirewallOpen())
            checkBox_SMM_OpenFirewallPort.Foreground = new SolidColorBrush(Colors.Red);
        if (_installerRepository.IsSMMIisApp())
            checkBox_SMM_IISCreateApp.Foreground = new SolidColorBrush(Colors.Red);

        if(_installerRepository.IsServiceIo())
            checkBox_Services_IO.Foreground = new SolidColorBrush(Colors.Red);
    }

    void FakeBind(Installer installer) // TODO binding
    {
        installer.Common_IIS = checkBox_Common_IIS.IsChecked ?? false;
        installer.Common_DotNetHosting = checkBox_Common_DotNetHosting.IsChecked ?? false;
        installer.Common_Edge = checkBox_Common_Edge.IsChecked ?? false;

        installer.SMM = checkBox_SMM.IsChecked ?? false;
        installer.SMM_IISCreateApp = checkBox_SMM_IISCreateApp.IsChecked ?? false;
        installer.SMM_OpenFirewallPort = checkBox_SMM_OpenFirewallPort.IsChecked ?? false;

        installer.Services_IO = checkBox_Services_IO.IsChecked ?? false;
    }


    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        var installer = new Installer();

        FakeBind(installer);

#if DEBUG
        //installer.SMM_OpenFirewallPort = true;
        //installer.Services_IO = true;
#endif
        WriteLog("-- Install Start --");

        // NOTE: make sure to install in correct order
        if (installer.Common_IIS)
            await Run(_installerRepository.IIS);
        if (installer.Common_DotNetHosting)
            await Run(_installerRepository.DotNetHosting);
        if (installer.Common_Edge)
            await Run(_installerRepository.Edge);

        if (installer.SMM)
        {
            if (_installerRepository.IsSMMIisApp())
            {
                await Run(_installerRepository.SMM_IIS_Stop);
                await Task.Delay(5000); // test, iis might need some time to stop
            }

            await Run(_installerRepository.SMM);

            if (_installerRepository.IsSMMIisApp())
                await Run(_installerRepository.SMM_IIS_Start);
        }
            
        if (installer.SMM_OpenFirewallPort)
            await Run(_installerRepository.SMM_OpenFirewallPort);
        if (installer.SMM_IISCreateApp)
            await Run(_installerRepository.SMM_IISCreateApp);
        if (installer.Services_IO)
            await Run(_installerRepository.Services_IO);

        WriteLog("-- Install Finished --");

        SetInstalledUi();
    }

    async Task Run(Func<string> func)
    {
        try
        {
            var text = await Task.Run<string>(func);
            WriteLog(text);
        }
        catch (Exception ex)
        {
            WriteLog($"Run error ex: {ex.Message}, ex: {ex.StackTrace}");
        }
    }

    void WriteLog(string text)
    {
        text = $"{DateTime.Now.ToString("HH:mm:ss")} {text}";

        var paragraph = new Paragraph(new Run(text))
        {
            Margin = new Thickness(0),
            Foreground = Brushes.Black
        };
        
        richTextBoxLog.Document.Blocks.Add(paragraph);
    }



}