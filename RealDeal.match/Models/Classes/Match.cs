namespace RealDeal.Models;

public class Match : ModelBase
{
    public string Equipe1 { get; set; }
    public string Equipe2 { get; set; }
    public DateTime DateMatch { get; set; }

}
