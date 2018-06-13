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
using WeihanLi.Extensions;
using WeihanLi.Redis;

namespace ProxyCrawler.Job
{
    public class SyncProxyJob : BaseQuartzJob
    {
        private readonly IReadOnlyCollection<IProxyProvider> _proxyProviders;

        public SyncProxyJob() : this(DependencyResolver.Current.ResolveServices<IProxyProvider>().ToArray())
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
                var result = SaveProxy(
                    await ValidateProxyAsync(ips.Distinct(new ProxyEntityEqualityComparer()).ToArray())
                    );
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

        private int SaveProxy(ProxyIpEntity[] proxyIpEntities)
        {
            if (proxyIpEntities.Length == 0)
            {
                Logger.Info("没有可用的代理");
                return 0;
            }
            Logger.Info($"可用代理IP数量：{proxyIpEntities.Length}");
            var commonClient = RedisManager.GetCommonRedisClient(RedisDataType.List);
            if (commonClient.KeyExists("proxyList"))
            {
                commonClient.KeyDelete("proxyList");
            }
            var proxyList = RedisManager.GetListClient<ProxyIpEntity>("proxyList");
            proxyList.Push(proxyIpEntities);
            return 1;
        }

        private async Task<ProxyIpEntity[]> ValidateProxyAsync(IReadOnlyCollection<ProxyIpEntity> proxyList)
        {
#if DEBUG
            await Task.Run(() => proxyList.ForEach(p => p.IsValid = true));
#else

            await Task.WhenAll(proxyList.Select(ValidateProxyAsync));
#endif
            return proxyList.Where(_ => _.IsValid).ToArray();
        }

        private async Task ValidateProxyAsync(ProxyIpEntity proxyEntity)
        {
            var client = new HttpRequestClient("https://baidu.com");
            client.AddProxy(new WebProxy(proxyEntity.Ip, proxyEntity.Port));
            HttpWebResponse response = null;
            try
            {
                response = await RetryHelper.TryInvokeAsync(() => client.ExecuteForResponseAsync(), res => res.StatusCode == HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.Warn($"验证代理【{proxyEntity.Ip}:{proxyEntity.Port}】发生异常", ex);
            }
            if (response?.StatusCode != null)
            {
                proxyEntity.IsValid = true;
                Logger.Info($"验证代理【{proxyEntity.Ip}:{proxyEntity.Port}】,response StatusCode:{response.StatusCode}");
            }
        }
    }
}
