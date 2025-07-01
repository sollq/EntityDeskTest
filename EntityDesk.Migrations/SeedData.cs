using EntityDesk.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityDesk.Migrations
{
    public static class SeedData
    {
        public static void EnsureSeeded(EntityDeskDbContext db)
        {
            if (!db.Employees.Any())
            {
                var manager = new Employee { FullName = "Иванов Иван Иванович", Position = Position.Manager, BirthDate = new DateTime(1980, 1, 1) };
                var worker = new Employee { FullName = "Петров Петр Петрович", Position = Position.Worker, BirthDate = new DateTime(1990, 5, 10) };
                db.Employees.AddRange(manager, worker);
                db.SaveChanges();

                var counterparty = new Counterparty { Name = "ООО Ромашка", INN = "1234567890", Curator = manager };
                db.Counterparties.Add(counterparty);
                db.SaveChanges();

                var order = new Order { Date = DateTime.Today, Amount = 10000, Employee = worker, Counterparty = counterparty };
                db.Orders.Add(order);
                db.SaveChanges();
            }
        }
    }
} 