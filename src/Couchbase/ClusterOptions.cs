using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Couchbase
{
    public sealed class ClusterOptions
    {
        private ConcurrentBag<Uri> _servers = new ConcurrentBag<Uri>();
        private ConcurrentBag<string> _buckets = new ConcurrentBag<string>();
        internal ConnectionString ConnectionString { get; set; }
        public string connectionString { get; set; }

        public static bool UseInterNetworkV6Addresses { get; set; }

        public ClusterOptions WithConnectionString(string connectionString)
        {
            ConnectionString = ConnectionString.Parse(connectionString);
            var uriBuilders = ConnectionString.Hosts.Select(x => new UriBuilder
            {
                Host = x,
                Port = KvPort
            }.Uri).ToArray();
            WithServers(uriBuilders);
            return this;
        }

        public ClusterOptions WithServers(params string[] servers)
        {
            if (!servers?.Any() ?? true)
            {
                throw new ArgumentException($"{nameof(servers)} cannot be null or empty.");
            }

            //for now just copy over - later ensure only new nodes are added
            _servers = new ConcurrentBag<Uri>(servers.Select(x => new Uri(x)));
            return this;
        }

        internal ClusterOptions WithServers(IEnumerable<Uri> servers)
        {
            if (!servers?.Any() ?? true)
            {
                throw new ArgumentException($"{nameof(servers)} cannot be null or empty.");
            }

            //for now just copy over - later ensure only new nodes are added
            _servers = new ConcurrentBag<Uri>(servers.ToList());
            return this;
        }

        public ClusterOptions WithBucket(params string[] bucketNames)
        {
            if (!bucketNames?.Any() ?? true)
            {
                throw new ArgumentException($"{nameof(bucketNames)} cannot be null or empty.");
            }

            //just the name of the bucket for now - later make and actual cluster
            _buckets = new ConcurrentBag<string>(bucketNames.ToList());
            return this;
        }

        public ClusterOptions WithCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException($"{nameof(username)} cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"{nameof(password)} cannot be null or empty.");
            }

            UserName = username;
            Password = password;
            return this;
        }

        public ClusterOptions WithLogging(ILoggerProvider provider = null)
        {
            //configure a null logger as the default
            if (provider == null)
            {
                provider = NullLoggerProvider.Instance;
            }

            LogManager.LoggerFactory.AddProvider(provider);
            return this;
        }

        public IEnumerable<Uri> Servers => _servers;
        public IEnumerable<string> Buckets => _buckets;
        public string UserName { get; set; }
        public string Password { get; set; }
        public TimeSpan ConnectTimeout { get; set; }
        public TimeSpan KvTimeout { get; set; }
        public TimeSpan ViewTimeout { get; set; }
        public TimeSpan QueryTimeout { get; set; }
        public TimeSpan AnalyticsTimeout { get; set; }
        public TimeSpan SearchTimeout { get; set; }
        public TimeSpan ManagementTimeout { get; set; }
        public TimeSpan ConfigPollInterval { get; set; } = TimeSpan.FromSeconds(2.5);
        public TimeSpan TcpKeepAliveTime { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan TcpKeepAliveInterval { get; set; } = TimeSpan.FromSeconds(1);
        public bool UseSsl { get; set; }
        public bool EnableTracing { get; set; }
        public bool EnableMutationTokens { get; set; }
        public int MgmtPort { get; set; } = 8091;
        public bool Expect100Continue { get; set; }
        public bool EnableCertificateAuthentication { get; set; }
        public bool EnableCertificateRevocation { get; set; }
        public bool IgnoreRemoteCertificateNameMismatch { get; set; }
        public int MaxQueryConnectionsPerServer { get; set; } = 10;
        public bool OrphanedResponseLoggingEnabled { get; set; }
        public bool EnableConfigPolling { get; set; } = true;
        public bool EnableTcpKeepAlives { get; set; } = true;
        public bool EnableIPV6Addressing { get; set; }
        public int KvPort { get; set; } = 11210;
        public bool EnableDnsSrvResolution { get; set; } = true;
        public IDnsResolver DnsResolver { get; set; } = new DnsClientDnsResolver();

        internal bool IsValidDnsSrv()
        {
            if (!EnableDnsSrvResolution || DnsResolver == null)
            {
                return false;
            }

            if (ConnectionString.Scheme != Scheme.Couchbase && ConnectionString.Scheme != Scheme.Couchbases)
            {
                return false;
            }

            if (ConnectionString.Hosts.Count > 1)
            {
                return false;
            }

            return ConnectionString.Hosts.Single().IndexOf(":") == -1;
        }
    }

    public static class NetworkTypes
    {
        public const string Auto = "auto";
        public const string Default = "default";
        public const string External = "external";
    }
}
