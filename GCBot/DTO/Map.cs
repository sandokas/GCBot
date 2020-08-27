using System;
using System.Collections.Generic;
using System.Text;

namespace GCBot.DTO
{
    public class Map
    {
        public Map(string code, Theater theater, string name, Faction attacker)
        {
            this.Code = code;
            this.Theater = theater;
            this.Name = name;
            this.Attacker = attacker;

        }
        public string Code { get; }
        public Theater Theater {get; }
        public string Name { get; }
        public Faction Attacker { get; }
    }
}
