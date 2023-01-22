using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MyCouch.Extensions;
using MyCouch.Net;
using Newtonsoft.Json;

namespace MyCouch.CloudantIAM
{
    public class CloudantDbConnection : DbConnection
    {
        private ApikeyAuth _apikeyAuth;
        private CookieAuth _cookieAuth;

        public CloudantDbConnection(CloudantDbConnectionInfo connectionInfo) : base(connectionInfo) 
        { 
            _apikeyAuth = connectionInfo.ApikeyAuth;
            _cookieAuth = connectionInfo.CookieAuth;
        }

        protected override bool ShouldFollowResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                bool authorized = AuthorizeAsync().ForAwait().GetAwaiter().GetResult();
                if (!authorized) return false;

                response.Headers.Location = response.RequestMessage.RequestUri;
                return true;
            }

            return base.ShouldFollowResponse(response);
        }

        private async Task<bool> AuthorizeAsync()
        {
            if (_apikeyAuth != null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://iam.cloud.ibm.com/identity/token");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HttpContentTypes.Json));
                    client.Timeout = TimeSpan.FromSeconds(5);

                    using (var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]{
                        new KeyValuePair<string, string>("grant_type", "urn:ibm:params:oauth:grant-type:apikey"),
                        new KeyValuePair<string, string>("apikey", _apikeyAuth.Apikey)
                    }))
                    {
                        var request = new HttpRequestMessage { Method = HttpMethod.Post, Content = content };
                        var response = await client.SendAsync(request).ForAwait();
                        if (!response.IsSuccessStatusCode) return false;

                        var credentials = JsonConvert.DeserializeObject<CloudantCredentials>(await response.Content.ReadAsStringAsync().ForAwait());
                        base.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(credentials.TokenType, credentials.AccessToken);
                        return true;
                    }
                }
            }

            if (_cookieAuth != null)
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = ReplacePathFrom(base.HttpClient.BaseAddress, "_session");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HttpContentTypes.Json));
                    client.Timeout = TimeSpan.FromSeconds(5);

                    using (var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]{
                        new KeyValuePair<string, string>("name", _cookieAuth.Name),
                        new KeyValuePair<string, string>("password", _cookieAuth.Password)
                    }))
                    {
                        var request = new HttpRequestMessage { Method = HttpMethod.Post, Content = content };
                        var response = await client.SendAsync(request).ForAwait();
                        if (!response.IsSuccessStatusCode) return false;

                        lock (base.HttpClient)
                        {
                            base.HttpClient.DefaultRequestHeaders.Remove("Cookie");
                            if (GetFirstCookie(response, out string cookie) &&
                                base.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookie)) return true;
                            else return false;
                        }
                    }
                }
            }

            return false;
        }

        private static Uri ReplacePathFrom(Uri address, string newPath)
        {
            return new Uri(address.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped) + "/" + newPath.Trim('/'));
        }

        private static bool GetFirstCookie(HttpResponseMessage message, out string cookie)
        {
            if (!message.Headers.TryGetValues("Set-Cookie", out var setCookie)) cookie = null;
            else cookie = setCookie.FirstOrDefault()?.Split(';').FirstOrDefault();
            return cookie != null;
        }
    }

    internal class CloudantCredentials 
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}