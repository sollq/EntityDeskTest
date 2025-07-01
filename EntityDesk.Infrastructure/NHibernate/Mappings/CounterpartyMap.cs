using EntityDesk.Core.Models;
using FluentNHibernate.Mapping;

namespace EntityDesk.Infrastructure.NHibernate.Mappings
{
    public class CounterpartyMap : ClassMap<Counterparty>
    {
        public CounterpartyMap()
        {
            Table("Counterparties");
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.Name).Not.Nullable().Column("Name");
            Map(x => x.INN).Not.Nullable().Column("INN");
            References(x => x.Curator).Column("CuratorId").Not.Nullable();
        }
    }
} 