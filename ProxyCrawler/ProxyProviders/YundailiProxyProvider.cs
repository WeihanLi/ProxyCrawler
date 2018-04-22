using ProxyCrawler.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Log;
using WeihanLi.Extensions;

namespace ProxyCrawler.ProxyProviders
{
    /// <summary>
    ///
    /// http://www.ip3366.net/free/?page={pageIndex}
    /// </summary>
    internal class YundailiProxyProvider : BaseProxyProvider
    {
        public override string ProxyProviderName => "Yundaili";
        protected override string PageUrlFormat => "http://www.ip3366.net/free/?page={0}";

        protected override async Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex)
        {
            try
            {
                var result = await Client.GetStringAsync(PageUrlFormat.FormatWith(pageIndex));
                var doc = await Parser.ParseAsync(result);
                return doc.QuerySelectorAll("#list>table>tbody>tr").Select(tr => new ProxyIpEntity
                {
                    Ip = tr.QuerySelectorAll("td")[0].TextContent.Trim(),
                    Port = tr.QuerySelectorAll("td")[1].TextContent.Trim().To<int>(),
                    Channel = ProxyProviderName,
                    Location = tr.QuerySelectorAll("td")[4].TextContent.Trim()
                });
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return Enumerable.Empty<ProxyIpEntity>();
            }
        }

        public YundailiProxyProvider() : base(LogHelper.GetLogHelper<YundailiProxyProvider>())
        {
        }
    }
}