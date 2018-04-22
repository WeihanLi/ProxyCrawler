using ProxyCrawler.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace ProxyCrawler.ProxyProviders
{
    /// <summary>
    /// https://proxy.coderbusy.com/classical/anonymous-type/highanonymous.aspx?page=2
    /// </summary>
    internal class CoderBusyProxyProvider : BaseProxyProvider
    {
        public CoderBusyProxyProvider() : base(LogHelper.GetLogHelper<CoderBusyProxyProvider>())
        {
        }

        public override string ProxyProviderName => "CoderBusy";

        protected override int TotalPage => 3;

        protected override string PageUrlFormat =>
            "https://proxy.coderbusy.com/classical/anonymous-type/highanonymous.aspx?page={0}";

        protected override async Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex)
        {
            var doc = await Parser.ParseAsync(await Client.GetStringAsync(PageUrlFormat.FormatWith(pageIndex)));
            return doc.QuerySelectorAll("table>tbody>tr").Select(tr =>
            {
                var tds = tr.QuerySelectorAll("td");
                if (tds[2].TextContent.To<double>() < 40)
                {
                    return null;
                }
                return new ProxyIpEntity
                {
                    Ip = tds[0].TextContent.Trim(),
                    Port = tds[2].TextContent.Trim().To<int>(),
                    Channel = ProxyProviderName,
                    Location = tds[3].TextContent
                };
            }).Where(_ => null != _);
        }
    }
}