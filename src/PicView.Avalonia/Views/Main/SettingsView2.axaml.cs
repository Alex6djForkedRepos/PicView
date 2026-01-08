using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace PicView.Avalonia.Views.Main;

public partial class SettingsView2 : UserControl
{ 
    public SettingsView2()
    {
        InitializeComponent();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            //FileAssociationsTabItem.IsEnabled = false;
        }
    }
}