using System;
using System.Linq;
using MyCouch.Net;

namespace MyCouch.CloudantIAM
{
    public class CloudantDbConnectionInfo : DbConnectionInfo
    {
        public CloudantApikey ApikeyAuth { get; set; }

        public CloudantDbConnectionInfo(string address, string dbName) : this(new Uri(address), dbName) { }

        public CloudantDbConnectionInfo(Uri address, string dbName) : base(RemoveUserInfoFrom(address), dbName) 
        {
            if (!string.IsNullOrWhiteSpace(address.UserInfo))
            {
                var userInfoParts = ExtractUserInfoPartsFrom(address);
                if (userInfoParts.Length == 2) BasicAuth = new BasicAuthString(userInfoParts[0], userInfoParts[1]);
                else ApikeyAuth = new CloudantApikey(userInfoParts[0]);
            }
        }

        private static Uri RemoveUserInfoFrom(Uri address)
        {
            return new Uri(address.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.UserInfo, UriFormat.UriEscaped));
        }

        private static string[] ExtractUserInfoPartsFrom(Uri address)
        {
            return address.UserInfo
                .Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.UnescapeDataString)
                .ToArray();
        }
    }
}