using System;
using System.Collections.Generic;
using System.Threading;

namespace Wsapm.Core
{
    /// <summary>
    /// Class to determine the average memory load.
    /// </summary>
    public sealed class MemoryLoadAverage : MemoryLoadBase
    {
        /// <summary>
        /// Initializes a new instance of MemoryLoadAverage.
        /// </summary>
        public MemoryLoadAverage() : base()
        {
        }

        /// <summary>
        /// Gets the average memory load for the given number of probes and the time between the probes.
        /// </summary>
        /// <param name="numberOfProbes">The number of probes which should be taken.</param>
        /// <param name="breakBetweenProbes"></param>
        /// <returns>The average memory load in percent.</returns>
        public float GetAverageMemoryLoad(int numberOfProbes, TimeSpan breakBetweenProbes)
        {
            ulong lastAvailablePhysicalMemory = 0;

            try
            {
                IList<float> buffer = new List<float>();

                for (int i = 0; i < numberOfProbes; i++)
                {
                    try
                    {
                        lastAvailablePhysicalMemory = this.computerInfo.AvailablePhysicalMemory;
                    }
                    catch (Exception)
                    {
                    }

                    buffer.Add((((float)TotalPhysicalMemory - (float)lastAvailablePhysicalMemory) / (float)TotalPhysicalMemory) * 100f);
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
                throw new WsapmException(Wsapm.Core.Resources.Wsapm_Core.MemoryLoadAverage_RetrieveLoadError, ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
