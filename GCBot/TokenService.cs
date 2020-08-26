using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GCBot
{
    public class TokenService
    {
        public TokenService(IConfiguration config)
        {
            //var config = services.GetRequiredService<IConfiguration>();
            var dics = config.GetSection("TokenRoles").Get<Dictionary<string, string>>();
            foreach (var dic in dics)
            {
                Tokens.Add(new Token() { ShortName = dic.Key, LongName = dic.Value });
            }

        }
        public IList<Token> Tokens { get; } = new List<Token>();
    }
}
