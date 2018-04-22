using ProxyCrawler.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeihanLi.Common.Helpers;

namespace ProxyCrawler.ProxyProviders
{
    /// <summary>
    /// http://www.mayidaili.com/free/anonymous/%E9%AB%98%E5%8C%BF/2
    /// </summary>
    internal class MayiProxyProvider : BaseProxyProvider
    {
        public MayiProxyProvider() : base(LogHelper.GetLogHelper<MayiProxyProvider>())
        {
        }

        public override string ProxyProviderName => "Mayi";

        protected override string PageUrlFormat => "http://www.mayidaili.com/free/anonymous/%E9%AB%98%E5%8C%BF/{0}";

        protected override Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex)
        {
            // TODO:识别网页上的图片端口号
            return Task.FromResult(Enumerable.Empty<ProxyIpEntity>());
        }
    }
}