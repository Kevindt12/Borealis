using Borealis.Domain.Animations;
using Borealis.Domain.Effects;
using Borealis.Portal.Data.Converters;
using Borealis.Portal.Domain.Configuration;
using Borealis.Portal.Domain.Devices;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;



namespace Borealis.Portal.Data.Contexts;


public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// The effects aggregates
    /// </summary>
    public DbSet<Effect> Effects { get; set; }

    /// <summary>
    /// The animations aggregates
    /// </summary>
    public DbSet<Animation> Animations { get; set; }

    /// <summary>
    /// The Device aggregates
    /// </summary>
    public DbSet<Device> Devices { get; set; }


    public ApplicationDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        Database.EnsureCreated();
    }


    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(BuildConnectionString());

        base.OnConfiguring(optionsBuilder);
    }


    private string BuildConnectionString()
    {
        // Build the connection string.
        SqliteConnectionStringBuilder connectionStringBuilder = new SqliteConnectionStringBuilder();
        connectionStringBuilder.DataSource = _configuration[ConfigurationKeys.DatabaseSourceLocation];
        connectionStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
        connectionStringBuilder.Cache = SqliteCacheMode.Shared;

        return connectionStringBuilder.ToString();
    }


    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Effect
        modelBuilder.Entity<Effect>(effect =>
        {
            effect.ToTable("Effects");
            effect.HasKey(p => p.Id);

            // Effect Parameters
            effect.OwnsMany(p => p.EffectParameters,
                            ef =>
                            {
                                ef.ToTable("EffectParameters");
                                ef.WithOwner().HasForeignKey("EffectId");
                                ef.HasKey(p => p.Id);
                                ef.Property(p => p.Value).HasConversion<EffectParameterValueConverter>();
                            });
        });

        // Device.
        modelBuilder.Entity<Device>(device =>
        {
            device.ToTable("Devices");
            device.HasKey(p => p.Id);

            device.Property(p => p.EndPoint).HasConversion<IPEndPointTypeConverter>();

            device.Ignore(p => p.Configuration);
        });

        // Animation
        modelBuilder.Entity<Animation>(animation =>
        {
            animation.ToTable("Animations");
            animation.HasKey(p => p.Id);

            // Effect
            animation.OwnsOne(p => p.Effect,
                              ae =>
                              {
                                  ae.ToTable("AnimationEffects");
                                  ae.HasKey(p => p.Id);

                                  // Effect Parameters
                                  ae.OwnsMany(p => p.EffectParameters,
                                              ef =>
                                              {
                                                  ef.ToTable("AnimationEffectParameters");
                                                  ef.WithOwner().HasForeignKey("EffectId");
                                                  ef.HasKey(p => p.Id);
                                                  ef.Property(p => p.Value).HasConversion<EffectParameterValueConverter>();
                                              });
                              });
        });

        base.OnModelCreating(modelBuilder);
    }
}