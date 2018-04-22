using AngleSharp.Parser.Html;
using ProxyCrawler.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeihanLi.Common.Log;

namespace ProxyCrawler.ProxyProviders
{
    internal abstract class BaseProxyProvider : IProxyProvider, IDisposable
    {
        public abstract string ProxyProviderName { get; }

        protected abstract string PageUrlFormat { get; }

        protected virtual int TotalPage { get; } = 1;

        protected readonly HttpClient Client = new HttpClient();

        //创建一个（可重用）解析器前端
        protected static readonly HtmlParser Parser = new HtmlParser();

        protected readonly ILogHelper Logger;

        protected BaseProxyProvider(ILogHelper logger) => Logger = logger;

        protected abstract Task<IEnumerable<ProxyIpEntity>> SyncProxyInternal(int pageIndex);

        public async Task<IReadOnlyCollection<ProxyIpEntity>> SyncProxyIp()
        {
            var stopwatch = Stopwatch.StartNew();
            var proxyes = (await Task.WhenAll(Enumerable.Range(1, TotalPage).Select(SyncProxyInternal))).SelectMany(e => e).ToArray();
            stopwatch.Stop();
            if (proxyes.Length > 0)
            {
                Logger.Info($"{ProxyProviderName} 同步代理完成，同步成功{proxyes.Length}个代理");
            }
            return proxyes;
        }

        #region IDisposable Support

        private bool _disposed; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)。
                }

                // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                Client?.Dispose();
                // 将大型字段设置为 null。

                _disposed = true;
            }
        }

        // 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~BaseProxyProvider() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}