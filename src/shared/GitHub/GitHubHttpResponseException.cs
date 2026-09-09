using System.Net;
using System.Net.Http;

namespace GitHub
{
    internal class GitHubHttpResponseException : HttpRequestException
    {
        public GitHubHttpResponseException(HttpStatusCode statusCode)
#if NET8_0_OR_GREATER
            : base($"The GitHub API returned HTTP {(int) statusCode} ({statusCode}).", null, statusCode)
#else
            : base($"The GitHub API returned HTTP {(int) statusCode} ({statusCode}).")
#endif
        {
#if !NET8_0_OR_GREATER
            StatusCode = statusCode;
#endif
        }

#if !NET8_0_OR_GREATER
        public HttpStatusCode StatusCode { get; }
#endif
    }
}
