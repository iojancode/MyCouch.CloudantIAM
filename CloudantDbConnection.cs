using System;
using System.Collections.Generic;
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
        private CloudantApikey _apiKeyAuth;

        public CloudantDbConnection(CloudantDbConnectionInfo connectionInfo) : base(connectionInfo) 
        { 
            _apiKeyAuth = connectionInfo.ApikeyAuth;
        }

        public override async Task<HttpResponseMessage> SendAsync(HttpRequest httpRequest, CancellationToken cancellationToken = default)
        {
            var response = await base.SendAsync(httpRequest, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized && await RetrieveTokenAsync()) 
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
            if (response.StatusCode == HttpStatusCode.Unauthorized && await RetrieveTokenAsync())
            {
                return await base.SendAsync(httpRequest, completionOption, cancellationToken);
            }
            else 
            {
                return response;
            }
        }

        private async Task<bool> RetrieveTokenAsync()
        {
            if (_apiKeyAuth == null) return false;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://iam.cloud.ibm.com/identity/token");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HttpContentTypes.Json));
                client.Timeout = TimeSpan.FromSeconds(5);

                using (var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]{
                    new KeyValuePair<string, string>("grant_type", "urn:ibm:params:oauth:grant-type:apikey"),
                    new KeyValuePair<string, string>("apikey", _apiKeyAuth.Apikey)
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
    }

    internal class CloudantCredentials 
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}