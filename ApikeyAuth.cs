using System;

namespace MyCouch.CloudantIAM
{
    public class ApikeyAuth
    {
        public string Apikey { get; private set; }

        public ApikeyAuth(string apikey)
        {
            Apikey = apikey;
        }
    }
}