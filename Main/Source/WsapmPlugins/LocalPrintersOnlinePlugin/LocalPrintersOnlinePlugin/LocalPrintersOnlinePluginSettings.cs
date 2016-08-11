using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalPrintersOnlinePlugin
{
    [Serializable]
    public class LocalPrintersOnlinePluginSettings
    {
        public List<string> OnlinePrinters
        {
            get;
            set;
        }
    }
}
