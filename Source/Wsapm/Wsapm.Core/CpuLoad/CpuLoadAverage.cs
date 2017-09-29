using System;
using System.Collections.Generic;
using System.Threading;

namespace Wsapm.Core
{
    /// <summary>
    /// Class to determine the average CPU load.
    /// </summary>
    public sealed class CpuLoadAverage : CpuLoadBase
    {
        /// <summary>
        /// Initializes a new instance of CpuLoadAverage.
        /// </summary>
        public CpuLoadAverage() : base()
        {
        }

        /// <summary>
        /// Gets the average CPU load for the given number of probes and the time between the probes.
        /// </summary>
        /// <param name="numberOfProbes">The number of probes which should be taken.</param>
        /// <param name="breakBetweenProbes"></param>
        /// <returns>The average CPU load in percent.</returns>
        public float GetAverageCpuLoad(int numberOfProbes, TimeSpan breakBetweenProbes)
        {
            try
            {
                IList<float> buffer = new List<float>();

                for (int i = 0; i < numberOfProbes; i++)
                {
                    buffer.Add(this.performanceCounterCpu.NextValue());
                    Thread.Sleep(breakBetweenProbes);
                }

                float tmp = 0;

                for (int i = 0; i < buffer.Count; i++)
                {
                    tmp += buffer[i];
                }

                tmp = tmp / buffer.Count;
                return tmp;
            }
            catch (Exception ex)
            {                
                throw new WsapmException(Wsapm.Core.Resources.Wsapm_Core.CpuLoadAverage_RetrieveLoadError, ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
