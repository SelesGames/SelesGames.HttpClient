using System;

namespace Common.Net.Http.Compression
{
    static class StringExtensions
    {
        public static bool IsGzipOrDeflate(this string encodingType)
        {
            return
                encodingType.Equals("gzip", StringComparison.OrdinalIgnoreCase) ||
                encodingType.Equals("deflate", StringComparison.OrdinalIgnoreCase);
        }
    }
}
