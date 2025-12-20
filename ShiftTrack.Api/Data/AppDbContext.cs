using Microsoft.EntityFrameworkCore;
using ShiftTrack.Api.Models;

namespace ShiftTrack.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ShiftDefinition> ShiftDefinitions => Set<ShiftDefinition>();
}
