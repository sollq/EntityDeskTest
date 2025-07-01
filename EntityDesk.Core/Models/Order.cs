using System;

namespace EntityDesk.Core.Models
{
    public class Order
    {
        public virtual int Id { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual required Employee Employee { get; set; }
        public virtual required Counterparty Counterparty { get; set; }
    }
} 