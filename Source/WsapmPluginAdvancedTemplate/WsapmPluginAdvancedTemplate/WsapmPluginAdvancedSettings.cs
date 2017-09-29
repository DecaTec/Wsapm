using System;

namespace WsapmPluginAdvancedTemplate
{
    // Add all of your settings in this class.
    // Just make sure that it is public, serializable and serialization works correctly.
    [Serializable]
    public class WsapmPluginAdvancedSettings
    {
        public string MyString
        {
            get;
            set;
        }
    }
}
