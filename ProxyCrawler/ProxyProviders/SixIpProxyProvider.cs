using ProxyCrawler.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;

namespace ProxyCrawler.ProxyProviders
{
    internal class SixIpProxyProvider : BaseProxyProvider
    {
        public SixIpProxyProvider() : base(LogHelper.GetLogHelper<SixIpProxyProvider>())
        {
        }

        public override string ProxyProviderName => "66Ip";
        protected override string PageUrlFormat => "";

        protected override Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex)
        {
            // TODO:66ip
            return Task.FromResult(Enumerable.Empty<ProxyIpEntity>());
        }
    }
}