///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This is a template for an advanced plugin for Windows Server Advanced Power Management (WSAPM) based on a Windows Forms UI
//
//
// Instructions:
//
//  -   If the reference to Wsapm.Extensions.dll can't be found, WSAPM is probably not installed in C:\Program Files (x86)\Windows Server Advanced Power Management\
//      In this case, remove the reference to Wsapm.Extensions.dll and add it again with the DLL located in the install folder of WSAPM.
//
//  -   Add your plugin's information in the WsapmPlugin attribute. Most important is to replace "{YOUR-GUID-HERE}" by a real GUID.
//      To generate a new GUID in Visual Studio, use the Create GUID utility (Tools > Create GUID)
//
//  -   This project targets the .NET Framework 4 Client Profile. This is the profile which is needed by WSAPM and is installed during its setup.
//      If you know the installed framework version on the target machine, you could also target another framework version.
//
//  -   Implement your plugin's logic in the overridden methods below. See the descriptions of the single methods.
//
//  -   When finished, pack your plugin DLL, the plugin manifest and all needed DLLs/resources/ReadMe files from the project's output folder in a ZIP file.
//      Do NOT include the Wsapm.Extensions.dll nor the de\Wsapm.Extensions.resources.dll in your package (these files are included in the WSAPM installation)!
//
//  -   Now you can ship your plugin!
//      To install it, go to WSAPM Settings > Plugins > Install plugin and follow the instructions.
//
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel.Composition;
using Wsapm.Extensions;

namespace WsapmPluginAdvancedTemplateWinForms
{
    [Export(typeof(WsapmPluginBase))]
    [WsapmPlugin("WSAPM plugin advanced template (WinForms)", "v1.0.0", "{YOUR-GUID-HERE}")]
    public class WsapmPluginAdvancedTemplate : WsapmPluginAdvancedBase
    {
        // The instance of the settings control. This is used in the SettingsControl override (see below).
        private WsapmPluginAdvancedControl settingsControl;

        public WsapmPluginAdvancedTemplate()
            : base(typeof(WsapmPluginAdvancedSettings))
        {
            // This is the constrcutor of you plugin class.
            // You have to call the base class' constructor with the type of your settings class here.
            // Do NOT create an instance of your UI class here or call any UI related code here, 
            // as this will throw an InvalidOperationException when your plugin is loaded by the WSAPM service!
        }

        protected override bool Initialize()
        {
            // Method which is called at least once after the plugin was loaded.
            //
            // Use this method to exeute any one-time initialization code.
            // The return code indicates if initialization was successfull.
            //
            // When your plugin does not need specific initialization, just return true:
            return true;
        }

        protected override bool Prepare()
        {
            // Method which is always called just before the plugin policy is checked.
            //
            // Use this method to prepare your plugin for the subsequent check of the plugin policy.
            // The return code indicates if preparation was successful.
            //
            // When your plugin does not need specific preparation, just return true:
            return true;
        }

        protected override PluginCheckSuspendResult CheckPluginPolicy()
        {
            // Method which is called to check the plugin's policy.
            //
            // Check you policy and create a PluginCheckSuspendResult:
            //      The fist parameter of the contsctucor indicates if standby should be supressed.
            //      The second one gives the reason for supression of standby - should be String.Empty if the first argument is 'false'.
            //
            // You can access your plugin's settings by using property 'CurrentSettings' from the base class (you will need a cast).
            var settings = (WsapmPluginAdvancedSettings)this.CurrentSettings;
            var myString = settings.MyString;

            var checkResult = new PluginCheckSuspendResult(false, String.Empty);
            return checkResult;
        }

        protected override bool TearDown()
        {
            // Method which is called after the plugin policy was checked.
            //
            // Use this method to dispose all resources which were created during the preparation or check of the plugin policy.
            // The return code indicates if tearing down was successfull.
            //
            // When your plugin does not need specific tearing down, just return true:
            return true;
        }

        protected override object LoadDefaultSettings()
        {
            // Method which get the default settings for the plugin.
            //
            // Use this method to provide the default settings for your plugin.
            // These default settings are loaded when there are no settings available, e.g. the plugin is started for the first time.
            var settings = new WsapmPluginAdvancedSettings();
            settings.MyString = "Default";
            return settings;
        }

        public override object SettingsControl
        {
            // Method to obtain the settings control for the plugin.
            //
            // Make sure that there will always be only one instance of your settings control,
            // i.e. create a new instance if it does not exist already, and return that instance on all following situations.
            //
            // IMPORTANT: This is the only place where you should do any UI related stuff, e.g. you must not create an instance of your UI class anywhere else!
            get
            {
                if (this.settingsControl == null)
                    this.settingsControl = new WsapmPluginAdvancedControl();

                return this.settingsControl;
            }
        }
    }
}
