using System;
using CrystalQuartz.Application;
using CrystalQuartz.Owin;
using Microsoft.Owin.Hosting;
using Quartz;
using Quartz.Impl;
using Topshelf;

namespace ProxyCrawler
{
    public class QuartzService : ServiceControl
    {
        private readonly IScheduler _scheduler = StdSchedulerFactory.GetDefaultScheduler();

        private IDisposable _webApp;

        public bool Start(HostControl hostControl)
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }
            // https://stackoverflow.com/questions/27168432/the-server-factory-could-not-be-located-for-the-given-input-microsoft-owin-host
            //
            _webApp = WebApp.Start("http://127.0.0.1:8200", app =>
            {
                app.UseCrystalQuartz(_scheduler, new CrystalQuartzOptions
                {
                    Path = "/quartz"
                });
            });
            _scheduler.Start();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            if (_webApp != null)
            {
                _webApp.Dispose();
                _webApp = null;
            }
            if (!_scheduler.IsShutdown)
            {
                _scheduler.Shutdown(false);
            }
            return true;
        }
    }
}
