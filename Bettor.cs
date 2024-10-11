using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogadasMokuskodas
{
    public class Bettor
    {
        public int BettorsID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public decimal Balance { get; set; }
        public DateTime JoinDate { get; set; }
        public string Role { get; set; }  
        public bool IsActive { get; set; }
        public static class SessionData
        {
            public static Bettor CurrentBettor { get; set; }
        }
    }
   
}
