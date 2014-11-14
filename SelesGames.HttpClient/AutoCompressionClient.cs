using Common.Net.Http.Compression;

namespace SelesGames.HttpClient
{
    public class AutoCompressionClient : System.Net.Http.HttpClient
    {
        public AutoCompressionClient()
            : base(new HttpClientCompressionHandler())
        { }
    }
}
