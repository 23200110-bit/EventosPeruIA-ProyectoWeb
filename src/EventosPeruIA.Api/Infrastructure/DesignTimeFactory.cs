using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using EventosPeruIA.Domain.Infrastructure.Data;
namespace EventosPeruIA.Api.Infrastructure;
public sealed class DesignTimeFactory:IDesignTimeDbContextFactory<EventosPeruDbContext>
{
 public EventosPeruDbContext CreateDbContext(string[] args)=>new(new DbContextOptionsBuilder<EventosPeruDbContext>().UseSqlServer(Environment.GetEnvironmentVariable("ConnectionStrings__EventosPeruIA")??"Server=(localdb)\\MSSQLLocalDB;Database=EventosPeruIA;Integrated Security=True;TrustServerCertificate=True").Options);
}
