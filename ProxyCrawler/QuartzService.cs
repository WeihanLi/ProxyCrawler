using System;
using CrystalQuartz.Application;
using CrystalQuartz.Owin;
using Microsoft.Owin.Hosting;
using Quartz;
using Quartz.Impl;
using WeihanLi.Common.Helpers;
using WeihanLi.Common.Log;

namespace ProxyCrawler
{
    public class QuartzService
    {
        private readonly IScheduler _scheduler = StdSchedulerFactory.GetDefaultScheduler();
        private static readonly ILogHelper Logger = LogHelper.GetLogHelper<QuartzService>();

        private IDisposable _webApp;

        public bool Start()
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }

            _scheduler.Start();

            try
            {
                // https://stackoverflow.com/questions/27168432/the-server-factory-could-not-be-located-for-the-given-input-microsoft-owin-host
                //
                _webApp = WebApp.Start("http://*:8200", app =>
                {
                    app.UseCrystalQuartz(_scheduler, new CrystalQuartzOptions
                    {
                        Path = "/quartz"
                    });
                });
                Logger.Info("webapp started");
            }
            catch (Exception ex)
            {
                Logger.Error("服务启动 webApp 失败", ex);
            }

            return true;
        }

        public bool Stop()
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }
            _scheduler.Shutdown(false);
            return true;
        }
    }
}
