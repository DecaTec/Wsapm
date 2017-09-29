
namespace Wsapm.Core
{
    /// <summary>
    /// Enum defining the level of details in the log.
    /// </summary>
    public enum LogMode
    {
        /// <summary>
        /// None, no information will be logged.
        /// </summary>
        None,
        /// <summary>
        /// OnlyErrors, only logs errors and no other details.
        /// </summary>
        OnlyErrors,
        /// <summary>
        /// Normal details, only normal details get logged.
        /// </summary>
        Normal,
        /// <summary>
        /// Verbose, all information gets logged.
        /// </summary>
        Verbose
    }
}
