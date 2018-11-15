namespace Kenc.ACMELib
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Kenc.ACMELib.ACMEEntities;
    using Kenc.ACMELib.ACMEResponses;
    using Kenc.ACMELib.Exceptions;
    using Kenc.ACMELib.JWS;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ACMERestClient : IRestClient
    {
        private const string ApplicationJsonMime = "application/json";
        private const string ApplicationJoseAndJson = "application/jose+json";
        private const string ApplicationProblemJsonMime = "application/problem+json";
        private const string ApplicationPemCertChainMime = "application/pem-certificate-chain";


        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly Jws jws;
        private readonly string UserAgent;
        private string nonce = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMERestClient"/> class.
        /// </summary>
        /// <param name="jws">Jws key to use.</param>
        /// <param name="logger">Optional logger.</param>
        public ACMERestClient(Jws jws)
        {
            var client = typeof(ACMEClient);
            var runtimeVersion = client.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

            this.jws = jws;
            UserAgent = $"{client.FullName}/{runtimeVersion.Version.ToString()} ({RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture})";
        }

        /// <summary>
        /// Sends a GET request to the targeted endpoint.
        /// </summary>
        /// <typeparam name="TResult">The class to serialize the result into.</typeparam>
        /// <param name="uri">Uri of the endpoint.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>a <typeparamref name="TResult"/> class with the result and a string containing the result.</returns>
        public async Task<(TResult Result, string Response)> GetAsync<TResult>(Uri uri, CancellationToken token) where TResult : class
        {
            return await SendAsync<TResult>(HttpMethod.Get, uri, null, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a HEAD request to the targeted endpoint.
        /// </summary>
        /// <typeparam name="TResult">The class to serialize the result into.</typeparam>
        /// <param name="uri">Uri of the endpoint.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>a <typeparamref name="TResult"/> class with the result and a string containing the result.</returns>
        public async Task<(TResult result, string response)> HeadAsync<TResult>(Uri uri, CancellationToken token) where TResult : class
        {
            return await SendAsync<TResult>(HttpMethod.Head, uri, null, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a POST request to the targeted endpoint.
        /// </summary>
        /// <typeparam name="TResult">The class to serialize the result into.</typeparam>
        /// <param name="uri">Uri of the endpoint.</param>
        /// <param name="message">Message body to send as part of the request.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>a <typeparamref name="TResult"/> class with the result and a string containing the result.</returns>
        public async Task<(TResult Result, string Response)> PostAsync<TResult>(Uri uri, object message, CancellationToken token) where TResult : class
        {
            return await SendAsync<TResult>(HttpMethod.Post, uri, message, token).ConfigureAwait(false);
        }

        private async Task<(TResult Result, string Response)> SendAsync<TResult>(HttpMethod method, Uri uri, object message, CancellationToken token) where TResult : class
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method.ToString();

            if (message != null)
            {
                if (string.IsNullOrEmpty(nonce))
                {
                    throw new NoNonceException();
                }

                var encodedMessage = jws.Encode(message, new JwsHeader(nonce, uri));
                var json = JsonConvert.SerializeObject(encodedMessage, JsonSettings);

                var stream = await request.GetRequestStreamAsync();
                var encoded = UTF8Encoding.UTF8.GetBytes(json);
                stream.Write(encoded, 0, encoded.Length);
                stream.Close();
                request.ContentLength = encoded.Length;
            }

            request.Headers[HttpRequestHeader.UserAgent] = UserAgent;
            request.Headers[HttpRequestHeader.ContentType] = ApplicationJoseAndJson;
            WebResponse response = null;
            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch (WebException exception)
            {
                response = exception.Response;
            }

            if (response.Headers.AllKeys.Contains("Replay-Nonce"))
            {
                nonce = response.Headers.GetValues("Replay-Nonce").First();
            }

            string responseBody = string.Empty;
            if (response.ContentLength > 0)
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        responseBody = reader.ReadToEnd();
                    }
                }
            }

            var httpResponse = (HttpWebResponse)response;
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                if (response.Headers[HttpRequestHeader.ContentType] == ApplicationProblemJsonMime)
                {
                    var problem = JsonConvert.DeserializeObject<Problem>(responseBody);
                    problem.RawJson = responseBody;
                    ExceptionHelper.ThrowException(problem);
                }
            }

            TResult responseContent = null;
            if (!string.IsNullOrEmpty(responseBody))
            {
                if (typeof(TResult) == typeof(string) && response.ContentType == ApplicationPemCertChainMime)
                {
                    return ((TResult)(object)responseBody, null);
                }

                responseContent = JObject.Parse(responseBody).ToObject<TResult>();
                if (response.Headers.AllKeys.Contains("Location") && responseContent is ILocationResponse locationResponse)
                {
                    locationResponse.Location = new Uri(response.Headers["Location"]);
                }
            }

            if (response.Headers.AllKeys.Contains("Location") && (responseContent == null || !(responseContent is ILocationResponse)))
            {
                Debug.WriteLine($"Response had location: {response.Headers["Location"]} but interface not applied to {responseContent?.GetType()}");
            }

            if (responseContent != null && responseContent is Account account)
            {
                jws.SetKeyId(account);
            }

            return (responseContent, responseBody);
        }
    }
}