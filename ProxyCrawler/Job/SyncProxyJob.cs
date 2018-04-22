using ProxyCrawler.Entity;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Log;
using WeihanLi.Extensions;

namespace ProxyCrawler.Job
{
    public class SyncProxyJob : IJob
    {
        private static readonly ILogHelper Logger = LogHelper.GetLogHelper<SyncProxyJob>();

        private readonly IReadOnlyCollection<IProxyProvider> _proxyProviders;

        public SyncProxyJob() : this(DependencyResolver.Current.GetServices<IProxyProvider>().ToArray())
        {
        }

        public SyncProxyJob(IReadOnlyCollection<IProxyProvider> proxyProviders) => _proxyProviders = proxyProviders;

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var ips = (await Task.WhenAll(_proxyProviders.Select(_ => _.SyncProxyIp()))).SelectMany(_ => _).Distinct(new ProxyEntityEqualityComparer()).ToArray();
                if (ips.Length > 0)
                {
                    ips = ValidateProxy(ips).ToArray();
                    var result = SyncToDb(ips);
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
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private static int SyncToDb(IReadOnlyCollection<ProxyIpEntity> proxyIpEntities)
        {
            var dataTable = proxyIpEntities.ToDataTable();
            using (var conn = new SqlConnection(ConfigurationHelper.ConnectionString("Proxy")))
            {
                return conn.BulkCopy(dataTable, "tabProxyIp");
            }
        }

        private static IEnumerable<ProxyIpEntity> ValidateProxy(IEnumerable<ProxyIpEntity> proxyList)
        {
            var client = new HttpRequestClient("https://weihanli.xyz");
            foreach (var entity in proxyList)
            {
                client.AddProxy(new WebProxy(entity.Ip, entity.Port));
                var response = RetryHelper.TryInvoke(() => client.ExecuteForResponse(), res => res.StatusCode == HttpStatusCode.OK);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    yield return entity;
                }
            }
        }
    }
}