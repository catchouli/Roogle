using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Roogle.RoogleSpider.Db
{
  /// <summary>
  /// The design time db context for using the dotnet ef tool
  /// </summary>
  public class RoogleSpiderDbContextFactory : IDesignTimeDbContextFactory<RoogleSpiderDbContext>
  {
    /// <summary>
    /// Create the db context
    /// </summary>
    /// <param name="args">The args</param>
    /// <returns>The created context</returns>
    public RoogleSpiderDbContext CreateDbContext(string[] args)
    {
      string devConnString = "server=localhost;database=roogle;user=root;password=password";
      var optionsBuilder = new DbContextOptionsBuilder<RoogleSpiderDbContext>();
      optionsBuilder.UseMySql(devConnString, ServerVersion.AutoDetect(devConnString));

      return new RoogleSpiderDbContext(optionsBuilder.Options);
    }
  }
}