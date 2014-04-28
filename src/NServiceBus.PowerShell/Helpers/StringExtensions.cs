using System;

namespace NServiceBus.PowerShell.Helpers
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Mimics the IsNullOrWhiteSpace method 
        /// </summary>
        public static bool IsNullOrWhiteSpace(String value)
        {
            if (value == null) return true;

            for (var i = 0; i < value.Length; i++)
            {
                if (!Char.IsWhiteSpace(value[i])) 
                    return false; 
            }
            return true;
        }
    }
}
