using EntityDesk.Core.Models;
using FluentNHibernate.Mapping;

namespace EntityDesk.Infrastructure.NHibernate.Mappings
{
    public class EmployeeMap : ClassMap<Employee>
    {
        public EmployeeMap()
        {
            Table("Employees");
            Id(x => x.Id).GeneratedBy.Identity().Column("Id");
            Map(x => x.FullName).Not.Nullable().Column("FullName");
            Map(x => x.Position).CustomType<int>().Not.Nullable().Column("Position");
            Map(x => x.BirthDate).Not.Nullable().Column("BirthDate");
        }
    }
} 