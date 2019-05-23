using System.Net;
using HtmlAgilityPack;

namespace TixatiLib
{
    public static class HtmlAgilityPackExtensions
    {
        public static string DecodedInnerText(this HtmlNode node)
            => WebUtility.HtmlDecode(node?.InnerText);
    }
}
