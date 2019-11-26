using Microsoft.AspNetCore.ResponseCompression;
using System.IO;

namespace AR.Telegraph.Helpers
{
    public class CustomCompressionProvider : ICompressionProvider
    {
        public string EncodingName => "telegraphCompression";
        public bool SupportsFlush => true;

        public Stream CreateStream(Stream outputStream)
        {
            // Create a custom compression stream wrapper here
            return outputStream;
        }
    }
}
