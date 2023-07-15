using Microsoft.AspNetCore.Http;
using System.Text;

namespace MyNetCore.Business
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// 获得当前完整url
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetAbsoluteUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

        public static string GetDomainUri(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .ToString();
        }
    }
}
