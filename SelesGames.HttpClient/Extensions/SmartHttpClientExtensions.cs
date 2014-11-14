using System;
using System.Linq;
using System.Net.Http.Formatting;

namespace SelesGames.HttpClient
{
    public static class SmartHttpClientExtensions
    {
        //public static void AddFormatter(this SmartHttpClient client, MediaTypeFormatter formatter)
        //{
        //    if (formatter == null) throw new ArgumentNullException("formatter");

        //    if (!client.Formatters.Any(o => o.Equals(formatter)))
        //        client.Formatters.Add(formatter);
        //}

        //public static void RemoveFormatters(this SmartHttpClient client, Func<MediaTypeFormatter, bool> condition)
        //{
        //    if (condition == null) throw new ArgumentNullException("condition");

        //    var matchingFormatters = client.Formatters.Where(condition).ToList();

        //    foreach (var formatter in matchingFormatters)
        //        client.Formatters.Remove(formatter);
        //}
    }
}
