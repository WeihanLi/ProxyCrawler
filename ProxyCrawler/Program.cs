using System;
using Autofac;
using ProxyCrawler.ProxyProviders;
using Topshelf;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Log;
using WeihanLi.Redis;

namespace ProxyCrawler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Init();
            try
            {
                HostFactory.Run(host =>
                {
                    host.RunAsLocalSystem();
                    host.StartAutomatically();

                    host.SetServiceName("ProxyCrawler");
                    host.SetDisplayName("ProxyCrawler");
                    host.SetDescription("代理爬虫");

                    host.DependsOn("Redis");

                    host.Service<QuartzService>(service =>
                    {
                        service.ConstructUsing(() => new QuartzService());
                        service.WhenStarted(x => x.Start());
                        service.WhenStopped(x => x.Stop());
                    });
                });
            }
            catch (Exception e)
            {
                LogHelper.GetLogHelper<Program>().Error(e);
            }
        }

        private static void Init()
        {
            //Log
            LogHelper.LogInit();

            // DI
            var builder = new ContainerBuilder();

            // TODO:Baibian Ip，Ip解码
            // builder.RegisterType<BaibianIpProxyProvider>().As<IProxyProvider>();
            builder.RegisterType<CoderBusyProxyProvider>().As<IProxyProvider>();
            builder.RegisterType<KuaidailiProxyProvider>().As<IProxyProvider>();
            builder.RegisterType<MayiProxyProvider>().As<IProxyProvider>();
            builder.RegisterType<SixIpProxyProvider>().As<IProxyProvider>();
            builder.RegisterType<XicidailiProxyProvider>().As<IProxyProvider>();
            builder.RegisterType<YundailiProxyProvider>().As<IProxyProvider>();

            var container = builder.Build();
            DependencyResolver.SetDependencyResolver(t => container.Resolve(t));

            // Redis
            RedisManager.AddRedisConfig(config =>
            {
                config.CachePrefix = "ProxyCrawler";
                config.DefaultDatabase = 2;
            });
        }
    }
}
