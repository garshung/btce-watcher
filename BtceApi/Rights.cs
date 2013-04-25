using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BtcE
{
    /// <summary>
    /// Permissions
    /// </summary>
    public class Rights
    {
        /// <summary>
        /// Permission to get information from BTC-e
        /// </summary>
        public bool Info { get; private set; }
        /// <summary>
        /// Permission to trade on BTC-e
        /// </summary>
        public bool Trade { get; private set; }

        public static Rights ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;

            var r = new Rights()
            {
                Info = o.Value<int>("info") == 1,
                Trade = o.Value<int>("trade") == 1
            };

            return r;
        }
    }
}
