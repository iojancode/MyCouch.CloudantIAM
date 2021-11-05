using System;

namespace MyCouch.CloudantIAM
{
    public class CookieAuth
    {
        public string Name { get; private set; }
        public string Password { get; private set; }

        public CookieAuth(string name, string password)
        {
            Name = name;
            Password = password;
        }
    }
}