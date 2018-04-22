using System.Collections.Generic;

namespace ProxyCrawler.Entity
{
    public class ProxyIpEntity
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string Location { get; set; }

        public string Channel { get; set; }
    }

    internal class ProxyEntityEqualityComparer : IEqualityComparer<ProxyIpEntity>
    {
        public bool Equals(ProxyIpEntity x, ProxyIpEntity y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }
            return x.Ip == y.Ip && x.Port == y.Port;
        }

        public int GetHashCode(ProxyIpEntity obj)
        {
            return $"{obj.Ip}:{obj.Port}".GetHashCode();
        }
    }
}