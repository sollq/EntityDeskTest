namespace EntityDesk.Core.Models;

public class Counterparty
{
    public virtual int Id { get; set; }
    public virtual required string Name { get; set; }
    public virtual required string INN { get; set; }
    public virtual required Employee Curator { get; set; }
}