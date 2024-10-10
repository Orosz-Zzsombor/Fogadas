public class Bet
{
    public int BetsID { get; set; }
    public DateTime BetDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Odds { get; set; }
    public int Status { get; set; }
    public int EventID { get; set; } 
    public string EventName { get; set; }  

    public string StatusString
    {
        get
        {
            return Status == 1 ? "Active" : "Inactive";
        }
    }
    public int BettorsID { get; set; }
}
