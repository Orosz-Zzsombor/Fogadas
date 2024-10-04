using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogadasMokuskodas
{
    public class Bet
    {
        public DateTime BetDate { get; set; }
        public decimal Amount { get; set; }
        public decimal Odds { get; set; }
        public string Status { get; set; }
    }
}

