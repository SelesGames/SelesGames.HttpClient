using System.Net.Http.Formatting;
using System.Linq;

namespace SelesGames.HttpClient
{
    public class StandardMediaTypeFormatters : MediaTypeFormatterCollection
    {
        public StandardMediaTypeFormatters()
        {
            if (!this.Any())
                return;

            var jsonFormatter = (JsonMediaTypeFormatter)this[0];
            jsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            jsonFormatter.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
        }
    }
}
