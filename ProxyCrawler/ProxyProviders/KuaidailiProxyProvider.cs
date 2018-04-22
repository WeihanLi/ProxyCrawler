using ProxyCrawler.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Log;
using WeihanLi.Extensions;

namespace ProxyCrawler.ProxyProviders
{
    /// <summary>
    ///
    /// https://www.kuaidaili.com/free/inha/{pageIndex}/
    /// </summary>
    internal class KuaidailiProxyProvider : BaseProxyProvider
    {
        public override string ProxyProviderName => "Kuaidaili";
        protected override string PageUrlFormat => "https://www.kuaidaili.com/free/inha/{0}/";

        protected override int TotalPage => 3;

        private readonly HttpClient _client = new HttpClient();

        public KuaidailiProxyProvider() : base(LogHelper.GetLogHelper<KuaidailiProxyProvider>())
        {
        }

        protected override async Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex)
        {
            try
            {
                var result = await _client.GetStringAsync(PageUrlFormat.FormatWith(pageIndex));

                var doc = await Parser.ParseAsync(result);
                return doc.QuerySelectorAll("#list>table>tbody>tr").Select(_ => new ProxyIpEntity
                {
                    Ip = _.QuerySelectorAll("td")[0].TextContent,
                    Port = _.QuerySelectorAll("td")[1].TextContent.To<int>(),
                    Location = _.QuerySelectorAll("td")[4].TextContent,
                    Channel = ProxyProviderName
                }).ToArray();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Enumerable.Empty<ProxyIpEntity>();
            }
        }
    }
}