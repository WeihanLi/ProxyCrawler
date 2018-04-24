using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ProxyCrawler.Entity;
using Quartz;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Log;
using WeihanLi.Extensions;

namespace ProxyCrawler.Job
{
    public class SyncProxyJob : BaseJob
    {
        private readonly IReadOnlyCollection<IProxyProvider> _proxyProviders;

        public SyncProxyJob() : this(DependencyResolver.Current.GetServices<IProxyProvider>().ToArray())
        {
        }

        public SyncProxyJob(IReadOnlyCollection<IProxyProvider> proxyProviders) : base(LogHelper.GetLogHelper<SyncProxyJob>()) => _proxyProviders = proxyProviders;

        protected override async Task ExecuteAsync(IJobExecutionContext context)
        {
            var ips = (await Task.WhenAll(_proxyProviders.Select(_ => _.SyncProxyIp()))).SelectMany(_ => _).Distinct(new ProxyEntityEqualityComparer()).ToArray();
            if (ips.Length > 0)
            {
                //验证代理可用性
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

        private static int SyncToDb(IReadOnlyCollection<ProxyIpEntity> proxyIpEntities)
        {
            if (proxyIpEntities.Count <= 0)
            {
                return 0;
            }
            var dataTable = proxyIpEntities.ToDataTable();
            using (var conn = new SqlConnection(ConfigurationHelper.ConnectionString("Proxy")))
            {
                conn.Execute("TRUNCATE TABLE [dbo].[tabProxyIp]");
                return conn.BulkCopy(dataTable, "tabProxyIp");
            }
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
            }
        }
    }
}
