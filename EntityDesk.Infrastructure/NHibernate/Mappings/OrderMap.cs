using EntityDesk.Core.Models;
using FluentNHibernate.Mapping;

namespace EntityDesk.Infrastructure.NHibernate.Mappings
{
    public class OrderMap : ClassMap<Order>
    {
        public OrderMap()
        {
            Table("`Order`");
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Date).Not.Nullable().Column("Date");
            Map(x => x.Amount).Not.Nullable().Column("Amount");
            References(x => x.Employee).Column("EmployeeId").Not.Nullable();
            References(x => x.Counterparty).Column("CounterpartyId").Not.Nullable();
        }
    }
} 