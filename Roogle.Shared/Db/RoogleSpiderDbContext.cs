using Microsoft.EntityFrameworkCore;

namespace Roogle.RoogleSpider.Db
{
  /// <summary>
  /// The database context
  /// </summary>
  public class RoogleSpiderDbContext : DbContext
  {
    /// <summary>
    /// Construct the database context with the given options
    /// </summary>
    /// <param name="options"></param>
    public RoogleSpiderDbContext(DbContextOptions<RoogleSpiderDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Pages that have been seen
    /// </summary>
    public DbSet<Page> Pages { get; set; }

    /// <summary>
    /// Links that have been seen
    /// </summary>
    public DbSet<Link> Links { get; set; }

    /// <summary>
    /// The search index
    /// </summary>
    public DbSet<SearchIndexEntry> SearchIndex { get; set; }

  }
}