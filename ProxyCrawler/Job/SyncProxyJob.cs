using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ProxyCrawler.Entity;
using Quartz;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Log;
using WeihanLi.Redis;

namespace ProxyCrawler.Job
{
    public class SyncProxyJob : BaseQuartzJob
    {
        private readonly IReadOnlyCollection<IProxyProvider> _proxyProviders;

        public SyncProxyJob() : this(DependencyResolver.Current.GetServices<IProxyProvider>().ToArray())
        {
        }

        public SyncProxyJob(IReadOnlyCollection<IProxyProvider> proxyProviders) : base(LogHelper.GetLogHelper<SyncProxyJob>()) => _proxyProviders = proxyProviders;

        protected override async Task ExecuteAsync(IJobExecutionContext context)
        {
            var ips = (await Task.WhenAll(_proxyProviders.Select(_ => _.SyncProxyIp()))).SelectMany(_ => _).ToList();
            ips.AddRange(RedisManager.GetListClient<ProxyIpEntity>("proxyList").ListRange() ?? Enumerable.Empty<ProxyIpEntity>());
            if (ips.Count > 0)
            {
                //验证代理可用性
                var result = SaveProxy(ValidateProxy(ips.Distinct(new ProxyEntityEqualityComparer())).ToArray());
                if (result > 0)
                {
                    Logger.Info("代理同步成功");
                }
                else
                {
                    Logger.Warn("代理同步失败");
                }
            }
        }

        private static int SaveProxy(ProxyIpEntity[] proxyIpEntities)
        {
            if (proxyIpEntities.Length > 0)
            {
                return 0;
            }
            var proxyList = RedisManager.GetListClient<ProxyIpEntity>("proxyList");
            proxyList.Push(proxyIpEntities);
            return 1;
        }

        private IEnumerable<ProxyIpEntity> ValidateProxy(IEnumerable<ProxyIpEntity> proxyList)
        {
            foreach (var entity in proxyList)
            {
                var client = new HttpRequestClient("https://weihanli.xyz");
                client.AddProxy(new WebProxy(entity.Ip, entity.Port));
                HttpWebResponse response = null;
                try
                {
                    response = RetryHelper.TryInvoke(() => client.ExecuteForResponse(), res => res.StatusCode == HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    Logger.Info("异常", ex);
                }
                if (response?.StatusCode == HttpStatusCode.OK)
                {
                    yield return entity;
                }
                else
                {
                    Logger.Info($"代理【{entity.Ip}:{entity.Port}】不可用");
                }
            }
        }
    }
}
