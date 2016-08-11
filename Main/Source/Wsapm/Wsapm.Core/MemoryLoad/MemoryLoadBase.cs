using Microsoft.VisualBasic.Devices;
using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Base class for all memory load related stuff.
    /// </summary>
    public abstract class MemoryLoadBase : IDisposable
    {
        protected ComputerInfo computerInfo;
        private ulong lastTotalPhysicalMemory;
        private ulong lastAvailablePhysicalMemory;

        protected MemoryLoadBase()
        {
            this.computerInfo = new ComputerInfo();
        }

        public ulong TotalPhysicalMemory
        {
            get
            {
                try
                {
                    this.lastTotalPhysicalMemory = this.computerInfo.TotalPhysicalMemory;
                }
                catch (Exception)
                {
                }

                return this.lastTotalPhysicalMemory;
            }
        }

        public ulong CurrentPhysicalMemory
        {
            get
            {
                try
                {
                    this.lastAvailablePhysicalMemory = this.computerInfo.AvailablePhysicalMemory;
                }
                catch (Exception)
                {
                }

                return this.TotalPhysicalMemory - this.lastAvailablePhysicalMemory;
            }
        }

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.computerInfo != null)
                    this.computerInfo = null;
            }
        }

        #endregion IDisposable members
    }
}
