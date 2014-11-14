using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SelesGames.HttpClient
{
    public static class UrlHelper
    {
        public static async Task<string> GetFinalRedirectLocation(string url, TimeSpan timeout, int cycleLimit = 5)
        {
            int cycleCount = 0;

            string previousRedirect = url;
            var currentRedirect = await GetRedirectOrOriginalUri(url, timeout);

            while (currentRedirect != previousRedirect)
            {
                // if a cycle is detected, return the original url
                if (cycleCount++ > cycleLimit)
                    return url;

                previousRedirect = currentRedirect;
                currentRedirect = await GetRedirectOrOriginalUri(currentRedirect, timeout);
            }

            return currentRedirect;
        }

        static async Task<string> GetRedirectOrOriginalUri(string url, TimeSpan timeout)
        {
            var client = new System.Net.Http.HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            client.Timeout = timeout;

            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

            var statusCode = response.StatusCode;

            if (
                   statusCode == HttpStatusCode.MultipleChoices     // 300
                || statusCode == HttpStatusCode.Ambiguous           // 300
                || statusCode == HttpStatusCode.MovedPermanently    // 301
                || statusCode == HttpStatusCode.Moved               // 301
                || statusCode == HttpStatusCode.Found               // 302
                || statusCode == HttpStatusCode.Redirect            // 302
                || statusCode == HttpStatusCode.SeeOther            // 303
                || statusCode == HttpStatusCode.RedirectMethod      // 303
                || statusCode == HttpStatusCode.TemporaryRedirect   // 307
                || statusCode == HttpStatusCode.RedirectKeepVerb    // 307
                || (int)statusCode == 308)                          // Permanent Redirect 308, part of experimental RFC proposal
            {
                var movedTo = GetLocationOrNull(response);
                if (!string.IsNullOrWhiteSpace(movedTo))
                    return movedTo;
            }

            return url;
        }

        static string GetLocationOrNull(HttpResponseMessage response)
        {
            if (response == null || EnumerableEx.IsNullOrEmpty(response.Headers) || response.Headers.Location == null)
                return null;

            return response.Headers.Location.OriginalString;
        }
    }
}
