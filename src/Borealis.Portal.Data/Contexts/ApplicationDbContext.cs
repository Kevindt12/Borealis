using Borealis.Domain.Effects;
using Borealis.Domain.Ledstrips;
using Borealis.Domain.Runtime;
using Borealis.Portal.Data.Converters;
using Borealis.Portal.Domain.Configuration;
using Borealis.Portal.Domain.Devices.Models;

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
    /// The Device aggregates
    /// </summary>
    public DbSet<Device> Devices { get; set; }

    /// <summary>
    /// The ledstrips that we know of.
    /// </summary>
    public DbSet<Ledstrip> Ledstrips { get; set; }


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
            effect.HasMany(p => p.JavascriptModules);

            effect.Property(p => p.Frequency).HasConversion<FrequencyTypeConverter>();

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
            device.Property(p => p.ConfigurationConcurrencyToken).IsRequired();

            device.HasMany(p => p.Ports).WithOne(x => x.Device);
        });

        modelBuilder.Entity<DevicePort>(configuration =>
        {
            configuration.ToTable("DeviceConnections");
            configuration.HasKey(p => p.Id);

            configuration.HasOne(p => p.Ledstrip);
        });

        // Ledstrips
        modelBuilder.Entity<Ledstrip>(ledstrip =>
        {
            ledstrip.ToTable("Ledstrips");
            ledstrip.HasKey(p => p.Id);
        });

        modelBuilder.Entity<JavascriptModule>(module =>
        {
            module.ToTable("JavascriptModules");
            module.HasKey(p => p.Id);
        });

        base.OnModelCreating(modelBuilder);
    }
}