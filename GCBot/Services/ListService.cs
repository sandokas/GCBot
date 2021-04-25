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
            this.Regiments = config.GetSection("RegimentRoles").Get<List<Regiment>>();
            this.Insults = config.GetSection("Insults").Get<List<string>>();
            this.Praises = config.GetSection("Praises").Get<List<string>>();
            this.BotChannel = config.GetSection("BotChannel").Get<string>();
            this.RegimentalAdminRole = config.GetSection("RegimentalAdminRole").Get<string>();
            this.Choices = config.GetSection("Choices").Get<List<string>>();
            this.AutoRoles = config.GetSection("AutoRoles").Get<List<string>>();
            this.AutoRoles = config.GetSection("AutoRoles").Get<List<string>>();

        }
        public IList<Regiment> Regiments { get; } = new List<Regiment>();
        public IList<string> Insults { get; } = new List<string>();
        public IList<string> Praises { get; } = new List<string>();
        public string BotChannel { get; } = "";
        public string RegimentalAdminRole { get; } = "";
        public IList<string> AutoRoles { get; } = new List<string>();
        public IList<string> Choices { get; } = new List<string>();
        public IList<string> Chosen { get; set; }= new List<string>();
        public IList<string> StrategyRoles { get; } = new List<string>();
    }
}
