namespace RealDeal.Classes;

public class Match : ModelBase
{
    public int Id { get; set; }
    public string Equipe1 { get; set; }
    public string Equipe2 { get; set; }
    public DateTime DateMatch { get; set; }

}
