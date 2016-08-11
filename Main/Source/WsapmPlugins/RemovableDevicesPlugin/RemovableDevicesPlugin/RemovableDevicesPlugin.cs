using System;
using System.ComponentModel.Composition;
using Wsapm.Extensions;

namespace RemovableDevicesPlugin
{
    /// <summary>
    ///  WSAPM plugin class for supressing standby if any removable devices are present.
    /// </summary>
    [Export(typeof(WsapmPluginBase))]
    [WsapmPlugin("Removable Devices", "v1.1.0" ,"{AC53783B-7EE6-4D95-B848-4576328E32D9}")]
    public class RemovableDevicesPlugin : WsapmPluginBase
    {
        protected override bool Initialize()
        {
            return true;
        }

        protected override bool Prepare()
        {
            return true;
        }

        protected override PluginCheckSuspendResult CheckPluginPolicy()
        {
            var removableDevicesPresent = RemovableDeviceCheck.IsRemovableDeviceConnected();

            // Supress standby if any removable devices are present.
            var checkResult = new PluginCheckSuspendResult(removableDevicesPresent, Resources.RemovableDevicesPlugin.RemovableDevicesPlugin_ReasonSuppressStandby);
            return checkResult;
        }       

        protected override bool TearDown()
        {
            return true;
        }
    }
}
