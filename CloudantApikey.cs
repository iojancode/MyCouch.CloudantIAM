using System;

namespace MyCouch.CloudantIAM
{
    public class CloudantApikey
    {
        public string Apikey { get; private set; }

        public CloudantApikey(string apikey)
        {
            Apikey = apikey;
        }
    }
}