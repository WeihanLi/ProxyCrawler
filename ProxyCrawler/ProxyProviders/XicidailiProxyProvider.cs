using ProxyCrawler.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;

namespace ProxyCrawler.ProxyProviders
{
    /// <summary>
    /// http://www.xicidaili.com/nn/{pageIndex}
    /// </summary>
    internal class XicidailiProxyProvider : BaseProxyProvider
    {
        public override string ProxyProviderName => "Xicidaili";
        protected override string PageUrlFormat => "http://www.xicidaili.com/nn/{0}";

        public XicidailiProxyProvider() : base(LogHelper.GetLogHelper<XicidailiProxyProvider>())
        {
        }

        protected override Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex)
        {
            // TODO:Xicidaili
            return Task.FromResult(Enumerable.Empty<ProxyIpEntity>());
        }
    }
}