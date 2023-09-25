using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpService
{
    public sealed class Config
    {
        public int? Port { get; private set; }
        public string Type { get; private set; }
        public string ConnectionString { get; private set; }

        private static IConfiguration configuration;
        private Config()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);               
            configuration = builder.Build();
            
            Port = configuration.GetValue<Int32>("Config:Port");
            Type = configuration.GetValue<string>("Config:Type");
            ConnectionString = configuration.GetValue<string>("Config:ConnectionString");
        }
        private static readonly Lazy<Config> lazy = new Lazy<Config>(() => new Config());
        public static Config Instance
        {

            get
            {
                return lazy.Value;
            }
        }
    }
}
