using Microsoft.EntityFrameworkCore;

namespace EntityDesk.Migrations
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var dbFactory = new EntityDeskDbContextFactory();
            using var db = dbFactory.CreateDbContext(args);
            db.Database.Migrate();
            SeedData.EnsureSeeded(db);
            Console.WriteLine("База данных создана и заполнена начальными данными.");
        }
    }
}
