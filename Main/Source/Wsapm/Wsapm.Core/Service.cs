using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class representing a service.
    /// </summary>
    [Serializable]
    public sealed class Service
    {
        /// <summary>
        /// Initializes a new instance of Service.
        /// </summary>
        public Service()
        {

        }

         /// <summary>
         /// Initializes a new instance of Service.
         /// </summary>
         /// <param name="serviceName">The service' name (name or display name).</param>
         public Service(string serviceName)
         {
             this.ServiceName = serviceName;
         }

         /// <summary>
         /// Gets or sets the service name.
         /// </summary>
         public string ServiceName
         {
             get;
             set;
         }

         /// <summary>
         /// Returns the string representation of the Service.
         /// </summary>
         /// <returns>The string representation of the Service.</returns>
         public override string ToString()
         {
             return this.ServiceName;
         }

         #region Equals

         public override bool Equals(object obj)
         {
             if (obj == null || GetType() != obj.GetType())
                 return false;

             Service other = obj as Service;
             return this.Equals(other);
         }

         public bool Equals(Service other)
         {
             if (other == null)
                 return false;

             return this.GetHashCode() == other.GetHashCode();
         }

         public static bool operator ==(Service a, Service b)
         {
             if (System.Object.ReferenceEquals(a, b))
                 return true;

             if (((object)a == null) || ((object)b == null))
                 return false;

             return a.GetHashCode() == b.GetHashCode();
         }

         public static bool operator !=(Service a, Service b)
         {
             return !(a == b);
         }

         public override int GetHashCode()
         {
             if(this.ServiceName == null)
                 return 0;

             return this.ServiceName.GetHashCode();
         }

         #endregion Equals

         /// <summary>
         /// Copies the object.
         /// </summary>
         /// <returns></returns>
         public Service Copy()
         {
             Service service = new Service(this.ServiceName);
             return service;
         }
    }
}
