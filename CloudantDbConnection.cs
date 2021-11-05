using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MyCouch.Net;
using Newtonsoft.Json;

namespace MyCouch.CloudantIAM
{
    public class CloudantDbConnection : DbConnection
    {
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private ApikeyAuth _apikeyAuth;
        private CookieAuth _cookieAuth;

        public CloudantDbConnection(CloudantDbConnectionInfo connectionInfo) : base(connectionInfo) 
        { 
            _apikeyAuth = connectionInfo.ApikeyAuth;
            _cookieAuth = connectionInfo.CookieAuth;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequest httpRequest, CancellationToken cancellationToken = default)
        {
            var response = await base.SendAsync(httpRequest, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized && await AuthorizeAsync()) 
            {
                return await base.SendAsync(httpRequest, cancellationToken);
            }
            else
            {
                return response;
            }
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequest httpRequest, HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
        {
            var response = await base.SendAsync(httpRequest, completionOption, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized && await AuthorizeAsync())
            {
                return await base.SendAsync(httpRequest, completionOption, cancellationToken);
            }
            else 
            {
                return response;
            }
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
                        var response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode) return false;

                        var credentials = JsonConvert.DeserializeObject<CloudantCredentials>(await response.Content.ReadAsStringAsync());
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
                        var response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode) return false;

                        base.HttpClient.DefaultRequestHeaders.Remove("Cookie");
                        if (GetFirstCookie(response, out string cookie) &&
                            base.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", cookie)) return true;
                        else return false;
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