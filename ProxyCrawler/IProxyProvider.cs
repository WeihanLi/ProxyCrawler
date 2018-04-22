using ProxyCrawler.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProxyCrawler
{
    public interface IProxyProvider
    {
        string ProxyProviderName { get; }

        Task<IReadOnlyCollection<ProxyIpEntity>> SyncProxyIp();
    }
}