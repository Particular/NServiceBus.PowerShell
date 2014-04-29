namespace NServiceBus.PowerShell.Helpers
{
    using System.IO;

    internal static class StreamsExtensions
    {
        const int bufferSize = 81920;

        //Replacement for .Net 4 CopyTo - This doesn't exist in .Net 2 
        public static void CopyTo(this Stream source, Stream destination)
        {
            var buffer = new byte[bufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
                destination.Write(buffer, 0, read);
        }
    }
}
