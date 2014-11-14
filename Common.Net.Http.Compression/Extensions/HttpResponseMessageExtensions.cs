using System.Net.Http;

namespace Common.Net.Http.Compression
{
    internal static class HttpResponseMessageExtensions
    {
        public static bool HasEncodeableContent(this HttpResponseMessage response)
        {
            return
                response.IsSuccessStatusCode &&
                response.Content != null &&
                response.RequestMessage != null &&
                response.RequestMessage.Headers != null;
        }
    }
}
