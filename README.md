# EntityFramework.Conventions
Conventions for EnitityFramework 6.x

``` csharp
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

public class BlogContext : DbContext  
{  
    public DbSet<Post> Posts { get; set; }  
    public DbSet<Comment> Comments { get; set; }  
    
    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Conventions.Add<SnakeCaseStoreModelConvention>();
        modelBuilder.Conventions.Add<LowerCaseTableNameConvention>();
        modelBuilder.Conventions.Add<SnakeCaseColumnNameConvention>();
    }
}
``` 