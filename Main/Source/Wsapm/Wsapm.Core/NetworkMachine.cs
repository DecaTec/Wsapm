using System;
using System.Net;
using System.Xml.Serialization;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a machine in a network.
    /// </summary>
    [Serializable]
    public sealed class NetworkMachine
    {
        /// <summary>
        /// Initializes a new instance of NetworkMachine.
        /// </summary>
        public NetworkMachine()
        {
        }

        /// <summary>
        /// Initializes a new instance of NetworkMachine.
        /// </summary>
        /// <param name="name">The name of the network machine.</param>
        /// <param name="ipAddress">The IP address of the network machine.</param>
        public NetworkMachine(string name, IPAddress ipAddress)
        {
            this.Name = name;
            this.IPAddress = ipAddress;
        }

        /// <summary>
        /// Initializes a new instance of NetworkMachine.
        /// </summary>
        /// <param name="name">The name of the network machine.</param>
        /// <param name="ipAddressStr">The IP address of the network machine.</param>
        public NetworkMachine(string name, string ipAddressStr)
        {
            this.Name = name;

            if (NetworkTools.IsStringIPAddress(ipAddressStr))
                this.IPAddress = IPAddress.Parse(ipAddressStr);
            else
                throw new FormatException("The IP address specified has the wrong format.");
        }

        /// <summary>
        /// Gets or sets the name of the network machine.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IP address.
        /// </summary>
        [XmlIgnore]
        public IPAddress IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IP address as string.
        /// </summary>
        [XmlElement("IPAddress")]
        public string IPAddressStr
        {
            get
            {
                if (this.IPAddress != null)
                    return this.IPAddress.ToString();
                else
                    return null;
            }
            set
            {
                try
                {
                    this.IPAddress = IPAddress.Parse(value);
                }
                catch(FormatException)
                {
                    this.IPAddress = null;
                }
            }
        }

        /// <summary>
        /// Returns the string representation of the NetworkMachine.
        /// </summary>
        /// <returns>The string representation of the NetworkMachine.</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                if (this.IPAddress == null)
                    return this.Name;
                else
                    return this.Name + " (" + this.IPAddress + ")";
            }
            else
            {
                if (this.IPAddress != null)
                    return this.IPAddress.ToString();
                else
                    return string.Empty;
            }
        }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            NetworkMachine other = obj as NetworkMachine;
            return this.Equals(other);
        }

        public bool Equals(NetworkMachine other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

        public static bool operator ==(NetworkMachine a, NetworkMachine b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))
                return false;

            return a.GetHashCode() == b.GetHashCode();
        }

        public static bool operator !=(NetworkMachine a, NetworkMachine b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            if (this.Name == null && this.IPAddress == null)
                return 0;
            else if (this.Name == null)
                return this.IPAddress.GetHashCode();
            else if (this.IPAddress == null)
                return this.Name.ToLower().GetHashCode();
            else
                return this.Name.ToLower().GetHashCode() ^ this.IPAddress.GetHashCode();
        }

        #endregion Equals

        /// <summary>
        /// Copies the object.
        /// </summary>
        /// <returns></returns>
        public NetworkMachine Copy()
        {
            NetworkMachine nm = new NetworkMachine();
            nm.Name = this.Name;
            nm.IPAddress = this.IPAddress;
            return nm;
        }
    }
}
