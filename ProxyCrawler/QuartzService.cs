using Quartz;
using Quartz.Impl;
using Topshelf;
using WeihanLi.Common;

namespace ProxyCrawler
{
    public class QuartzService : ServiceControl
    {
        private IScheduler _scheduler;

        public bool Start(HostControl hostControl)
        {
            _scheduler = DependencyResolver.Current.GetService<StdSchedulerFactory>().GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();
            _scheduler.Start().ConfigureAwait(false);
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            if (!_scheduler.IsShutdown)
            {
                _scheduler.Shutdown(false).ConfigureAwait(false);
            }

            return true;
        }
    }
}