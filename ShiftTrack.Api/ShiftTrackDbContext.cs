using Microsoft.EntityFrameworkCore;
using ShiftTrack.Api.Models;

namespace ShiftTrack.Api
{
    public class ShiftTrackDbContext : DbContext
    {
        public ShiftTrackDbContext(DbContextOptions<ShiftTrackDbContext> options) : base(options)
        {
        }

        public DbSet<ShiftDefinition> ShiftDefinitions => Set<ShiftDefinition>();
    }
}
