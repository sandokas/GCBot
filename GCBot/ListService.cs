using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GCBot
{
    public class ListService
    {
        public ListService(IConfiguration config)
        {
            var dics = config.GetSection("RegimentRoles").Get<Dictionary<string, string>>();
            foreach (var dic in dics)
            {
                Regiments.Add(new Regiment() { ShortName = dic.Key, LongName = dic.Value });
            }
            this.Insults = config.GetSection("Insults").Get<List<string>>();
            this.Praises = config.GetSection("Praises").Get<List<string>>();
            this.BotChannel = config.GetSection("BotChannel").Get<string>();
            this.RegimentalAdminRole = config.GetSection("RegimentalAdminRole").Get<string>();
        }
        public IList<Regiment> Regiments { get; } = new List<Regiment>();
        public IList<string> Insults { get; } = new List<string>();
        public IList<string> Praises { get; } = new List<string>();
        public string BotChannel { get; } = "";
        public string RegimentalAdminRole { get; } = "";
    }
}
