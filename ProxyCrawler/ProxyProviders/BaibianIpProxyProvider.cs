using ProxyCrawler.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace ProxyCrawler.ProxyProviders
{
    /// <summary>
    /// https://www.baibianip.com/home/free.html
    /// </summary>
    internal class BaibianIpProxyProvider : BaseProxyProvider
    {
        public BaibianIpProxyProvider() : base(LogHelper.GetLogHelper<BaibianIpProxyProvider>())
        {
        }

        public override string ProxyProviderName => "BaibianIp";

        protected override string PageUrlFormat => "https://www.baibianip.com/home/free.html";

        protected override async Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex)
        {
            var doc = await Parser.ParseAsync(await Client.GetStringAsync(PageUrlFormat.FormatWith(pageIndex)));
            return doc.QuerySelectorAll("table>tbody>tr").Select(tr =>
            {
                var tds = tr.QuerySelectorAll("td");
                if (!tds[4].TextContent.Trim().EqualsIgnoreCase("优质高匿"))
                {
                    return null;
                }

                return new ProxyIpEntity
                {
                    Ip = tds[0].TextContent.Trim(),
                    Port = tds[1].TextContent.Trim().To<int>(),
                    Channel = ProxyProviderName,
                    Location = tds[2].TextContent + tds[3].TextContent
                };
            }).Where(_ => _ != null);
        }
    }
}