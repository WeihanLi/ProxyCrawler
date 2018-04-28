using System;
using System.Threading.Tasks;
using Quartz;
using WeihanLi.Common.Log;

namespace ProxyCrawler.Job
{
    public abstract class BaseQuartzJob : IJob
    {
        protected readonly ILogHelper Logger;

        protected BaseQuartzJob(ILogHelper logger) => Logger = logger;

        public virtual void Execute(IJobExecutionContext context)
        {
            try
            {
                Logger.Info("Job 开始执行");
                Task.WaitAll(ExecuteAsync(context));
            }
            catch (Exception ex)
            {
                Logger.Error("Job 执行出错", ex);
            }
            finally
            {
                Logger.Info("Job 执行结束");
            }
        }

        protected abstract Task ExecuteAsync(IJobExecutionContext context);
    }
}
