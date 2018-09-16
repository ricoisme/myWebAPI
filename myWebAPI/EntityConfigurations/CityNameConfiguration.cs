using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using myWebAPI.Controllers;

namespace myWebAPI.EntityConfigurations
{
    public class CityNameConfiguration : IEntityTypeConfiguration<CityName>
    {
        public void Configure(EntityTypeBuilder<CityName> builder)
        {
            builder.ToTable("CityName")
                .HasKey(a => a.Id)
                .ForSqlServerIsClustered();

            builder.Property(a => a.Id)
                .HasColumnType("int")
                .IsRequired();

            builder.Property(a => a.CName)
                .HasColumnType("varchar(30)")
                .IsRequired(false);
        }
    }
}
