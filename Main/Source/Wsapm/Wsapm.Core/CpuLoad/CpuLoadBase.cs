using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Wsapm.Core
{
    /// <summary>
    /// Base class for all CPU load related stuff.
    /// </summary>
    public abstract class CpuLoadBase : IDisposable
    {
        protected readonly PerformanceCounter performanceCounterCpu;

        protected const string CategoryName = "Processor";
        protected const string PerformanceCounterName = "% Processor Time";
        protected const string InstanceName = "_Total";

        protected CpuLoadBase()
        {
            this.performanceCounterCpu = new PerformanceCounter(CategoryName, PerformanceCounterName, InstanceName);
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
                if (this.performanceCounterCpu != null)
                    this.performanceCounterCpu.Dispose();
            }
        }

        #endregion IDisposable members
    }
}
