using System;
using TextCopy;

namespace Gwen.Platform
{
    /// <summary>
    /// Platform-agnostic utility functions.
    /// </summary>
    public static class Neutral
    {
        private static readonly DateTime m_FirstTime = DateTime.Now;

        /// <summary>
        /// Gets text from clipboard.
        /// </summary>
        /// <returns>Clipboard text.</returns>
        public static string GetClipboardText()
        {
            return ClipboardService.GetText();
        }

        /// <summary>
        /// Sets the clipboard text.
        /// </summary>
        /// <param name="text">Text to set.</param>
        /// <returns>True if succeeded.</returns>
        public static bool SetClipboardText(string text)
        {
            ClipboardService.SetText(text);
            return true;
        }

        /// <summary>
        /// Gets elapsed time since this class was initalized.
        /// </summary>
        /// <returns>Time interval in seconds.</returns>
        public static float GetTimeInSeconds()
        {
            //[halfofastaple] Note:
            //  After 3.8 months, the difference in value will be greater than a second,
            //  which isn't a problem for most people (who will run this that long?), but
            //  if it is, we can convert this (and all timestamps that rely on this) to a double, 
            //  which will grow stale (time difference > 1s) after ~3,168,888 years 
            //  (that's gotta be good enough, right?)
            //P.S. someone fix those numbers if I'm wrong.
            return (float)((DateTime.Now - m_FirstTime).TotalSeconds);
        }
    }
}
