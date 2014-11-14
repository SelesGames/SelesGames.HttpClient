using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Net.Http.Compression.Settings
{
    public class CompressionHandlerCollection : List<CompressionHandler>
    {
        public IEnumerable<string> GetSupportedEncodings()
        {
            return this.SelectMany(o => o.SupportedEncodings);
        }

        public CompressionHandler Find(string encoding)
        {
            return this
                .FirstOrDefault(o => o.SupportedEncodings
                    .Any(e => e.Equals(encoding, StringComparison.OrdinalIgnoreCase)));
        }

        public IEnumerable<CompressionHandler> Find(IEnumerable<string> encodings)
        {
            List<CompressionHandler> matches = new List<CompressionHandler>();

            foreach (var encoding in encodings)
            {
                foreach (var handler in this)
                {
                    if (handler.SupportedEncodings
                        .Any(o => o.Equals(encoding, StringComparison.OrdinalIgnoreCase)))
                    {
                        matches.Add(handler);
                    }
                }
            }

            return matches.Distinct();
        }
    }
}
