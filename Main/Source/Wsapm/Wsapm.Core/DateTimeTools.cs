using System;

namespace Wsapm.Core
{
    /// <summary>
    /// Class for DateTime tools.
    /// </summary>
    public static class DateTimeTools
    {
        /// <summary>
        /// Determines if the DateTime specified is already passed by, i.e. the DateTime lies in the past.
        /// </summary>
        /// <param name="dateTime">The DateTime to inspect.</param>
        /// <returns>True if the specified DateTime is already passed by.</returns>
        /// <remarks>When the DateTime specified is exactly the current DateTime, the method will return false.</remarks>
        public static bool DateTimeIsPassedBy(DateTime dateTime)
        {
            return DateTime.Now > dateTime;
        }
    }
}
