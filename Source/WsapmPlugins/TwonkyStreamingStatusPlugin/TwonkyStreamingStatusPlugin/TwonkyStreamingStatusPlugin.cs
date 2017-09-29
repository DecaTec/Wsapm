using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using Wsapm.Extensions;

namespace TwonkyStreamingStatusPlugin
{
    [Export(typeof(WsapmPluginBase))]
    [WsapmPlugin("Twonky Streaming Status", "v1.0.0", "{3F5C5F7A-12D7-4257-BC94-1B359F9B3C72}")]
    public class TwonkyStreamingStatusPlugin : WsapmPluginAdvancedBase
    {
        private TwonkyStreamingStatusPluginSettingsControl settingsControl;

        public TwonkyStreamingStatusPlugin() : base(typeof(TwonkyStreamingStatusPluginSettings))
        {

        }

        protected override object LoadDefaultSettings()
        {
            return new TwonkyStreamingStatusPluginSettings() {  TwonkyUrl = TwonkyStreamingStatusPluginConstants.DefaultTwonkyUrl };
        }

        public override object SettingsControl
        {
            get
            {
                if (this.settingsControl == null)
                    this.settingsControl = new TwonkyStreamingStatusPluginSettingsControl();

                return this.settingsControl;
            }
        }

        protected override PluginCheckSuspendResult CheckPluginPolicy()
        {
            var pluginSettings = this.CurrentSettings as TwonkyStreamingStatusPluginSettings;

            if (pluginSettings == null || string.IsNullOrEmpty(pluginSettings.TwonkyUrl))
                return new PluginCheckSuspendResult(false, string.Empty);

            string requestUrl = pluginSettings.TwonkyUrl;
            
            if(!requestUrl.EndsWith(@"/"))
                requestUrl += @"/";

            requestUrl += TwonkyStreamingStatusPluginConstants.StreamingStatusUrl;

            try
            {
                var webRequest = WebRequest.Create(new Uri(requestUrl));
                // Don't use cache.
                webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                var response = webRequest.GetResponse();
                string content = string.Empty;

                using (var stream = new StreamReader(response.GetResponseStream()))
                {
                    content = stream.ReadToEnd();
                }

                if (!string.IsNullOrEmpty(content))
                {
                    if (content.ToUpper() == TwonkyStreamingStatusPluginConstants.StreamingSatusResponseStreaming) // "SA:0" > no streaming; "SA:1" > streaming active
                        return new PluginCheckSuspendResult(true, Resources.TwonkyStreamingStatusPlugin.ServerIsStreaming);
                }
            }
            catch (Exception)
            {
            }

            return new PluginCheckSuspendResult(false, string.Empty);
        }

        protected override bool Initialize()
        {
            return true;
        }

        protected override bool Prepare()
        {
            return true;
        }

        protected override bool TearDown()
        {
            return true;
        }
    }
}
