using System;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a network interface to monitor (for settings).
    /// </summary>
    [Serializable]
    public sealed class NetworkInterfaceToMonitor
    {
        /// <summary>
        /// Gets or sets the network interface to use for network load check.
        /// </summary>
        public string NetworkInterface
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the total network load should be checked.
        /// </summary>
        public bool EnableCheckNetworkLoadTotal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total network load (upload + download) in KBit/s.
        /// </summary>
        public float NetworkLoadTotal
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a formatted string ('xx KBit/s') for the total network load.
        /// </summary>
        [XmlIgnore]
        public string NetworkLoadTotalFormatted
        {
            get
            {
                if (this.EnableCheckNetworkLoadTotal)
                    return string.Format("{0} KBit/s", this.NetworkLoadTotal);
                else
                    return "-";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the download network load should be checked.
        /// </summary>
        public bool EnableCheckNetworkLoadDownload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total network load (download) in KBit/s.
        /// </summary>
        public float NetworkLoadDownload
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a formatted string ('xx KBit/s') for the download network load.
        /// </summary>
        [XmlIgnore]
        public string NetworkLoadDownloadFormatted
        {
            get
            {
                if (this.EnableCheckNetworkLoadDownload)
                    return string.Format("{0} KBit/s", this.NetworkLoadDownload);
                else
                    return "-";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if the upload network load should be checked.
        /// </summary>
        public bool EnableCheckNetworkLoadUpload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total network load (upload) in KBit/s.
        /// </summary>
        public float NetworkLoadUpload
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a formatted string ('xx KBit/s') for the upload network load.
        /// </summary>
        [XmlIgnore]
        public string NetworkLoadUploadFormatted
        {
            get
            {
                if (this.EnableCheckNetworkLoadUpload)
                    return string.Format("{0} KBit/s", this.NetworkLoadUpload);
                else
                    return "-";
            }
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            NetworkInterfaceToMonitor other = obj as NetworkInterfaceToMonitor;
            return this.Equals(other);
        }

        public bool Equals(NetworkInterfaceToMonitor other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(NetworkInterfaceToMonitor a, NetworkInterfaceToMonitor b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(NetworkInterfaceToMonitor a, NetworkInterfaceToMonitor b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return this.EnableCheckNetworkLoadDownload.GetHashCode() ^ this.EnableCheckNetworkLoadUpload.GetHashCode() ^ this.EnableCheckNetworkLoadTotal.GetHashCode()
                ^ this.NetworkInterface.GetHashCode() ^ this.NetworkLoadDownload.GetHashCode() ^ this.NetworkLoadUpload.GetHashCode() ^ this.NetworkLoadTotal.GetHashCode();
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public NetworkInterfaceToMonitor Copy()
        {
            NetworkInterfaceToMonitor nic = new NetworkInterfaceToMonitor();
            nic.EnableCheckNetworkLoadDownload = this.EnableCheckNetworkLoadDownload;
            nic.EnableCheckNetworkLoadUpload = this.EnableCheckNetworkLoadUpload;
            nic.EnableCheckNetworkLoadTotal = this.EnableCheckNetworkLoadTotal;
            nic.NetworkInterface = this.NetworkInterface;
            nic.NetworkLoadDownload = this.NetworkLoadDownload;
            nic.NetworkLoadUpload = this.NetworkLoadUpload;
            nic.NetworkLoadTotal = this.NetworkLoadTotal;
            return nic;
        }
    }
}
