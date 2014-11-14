using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    public static class HttpResponseMessageExtensions
    {
        public static string GetValueForHeader(this HttpHeaders headers, string header)
        {
            IEnumerable<string> headerValues;
            if (headers.TryGetValues(header, out headerValues))
                return headerValues.FirstOrDefault();
            else
                return null;
        }

        //public static void EnsureSuccessStatusCode2(this HttpResponseMessage message)
        //{
        //    if (!message.IsSuccessStatusCode)
        //        throw new ErrorResponseException(message);
        //}
    }
}
