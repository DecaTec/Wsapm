namespace Wsapm.Core
{
    /// <summary>
    /// Class for conversion.
    /// </summary>
    public static class WsapmConvert
    {
        #region Size

        /// <summary>
        /// Converts size from byte to KB.
        /// </summary>
        /// <param name="sizeInByte">The size in byte.</param>
        /// <returns>The size in KB.</returns>
        public static uint ConvertByteToKB(uint sizeInByte)
        {
            return sizeInByte / 1000;
        }

        /// <summary>
        /// Converts size from byte to KB.
        /// </summary>
        /// <param name="sizeInByte">The size in byte.</param>
        /// <returns>The size in KB.</returns>
        public static float ConvertByteToGB(ulong sizeInByte)
        {
            return  (float)(sizeInByte / 1024f / 1024f / 1024f);
        }

        /// <summary>
        /// Converts size from byte to KB.
        /// </summary>
        /// <param name="sizeInByte">The size in byte.</param>
        /// <returns>The size in KB.</returns>
        public static float ConvertByteToKB(float sizeInByte)
        {
            return sizeInByte / 1000;
        }

        /// <summary>
        /// Converts size from KB to byte.
        /// </summary>
        /// <param name="sizeInKB">The size in KB.</param>
        /// <returns>The size in byte.</returns>
        public static uint ConvertKBToByte(uint sizeInKB)
        {
            return sizeInKB * 1000;
        }

        /// <summary>
        /// Converts size from KB to byte.
        /// </summary>
        /// <param name="sizeInKB">The size in KB.</param>
        /// <returns>The size in byte.</returns>
        public static float ConvertKBToByte(float sizeInKB)
        {
            return sizeInKB * 1000;
        }

        /// <summary>
        /// Converts size from byte to KBit.
        /// </summary>
        /// <param name="sizeInByte">The size in byte.</param>
        /// <returns>The size in KBit.</returns>
        public static uint ConvertByteToKBit(uint sizeInByte)
        {
            return sizeInByte * 8 / 1000;
        }

        /// <summary>
        /// Converts size from byte to KBit.
        /// </summary>
        /// <param name="sizeInByte">The size in byte.</param>
        /// <returns>The size in KBit.</returns>
        public static float ConvertByteToKBit(float sizeInByte)
        {
            return sizeInByte * 8 / 1000;
        }

        /// <summary>
        /// Converts size from KBit to byte.
        /// </summary>
        /// <param name="sizeInKB">The size in KBit.</param>
        /// <returns>The size in byte.</returns>
        public static float ConvertKBitToByte(float sizeInKBit)
        {
            return sizeInKBit * 1000 / 8;
        }

        #endregion Size
    }
}
