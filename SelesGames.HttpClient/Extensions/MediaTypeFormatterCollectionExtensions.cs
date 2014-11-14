using System;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace SelesGames.HttpClient
{
    public static class MediaTypeFormatterCollectionExtensions
    {
        public static MediaTypeFormatter FindWriteFormatter<T>(this MediaTypeFormatterCollection formatters, MediaTypeHeaderValue mediaType)
        {
            if (formatters == null) throw new ArgumentNullException("formatters");
            if (mediaType == null) throw new ArgumentNullException("mediaType");

            var type = typeof(T);
            var formatter = formatters.FindWriter(type, mediaType);

            if (formatter == null)
                throw new Exception(string.Format("unable to find a valid MediaTypeFormatter that matches {0}", mediaType));

            return formatter;
        }
    }
}