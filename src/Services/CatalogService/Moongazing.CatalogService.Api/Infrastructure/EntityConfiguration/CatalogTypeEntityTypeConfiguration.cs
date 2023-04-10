using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moongazing.CatalogService.Api.Core.Domain.Entities;
using Moongazing.CatalogService.Api.Infrastructure.Context;

namespace Moongazing.CatalogService.Api.Infrastructure.EntityConfiguration
{
    public class CatalogTypeEntityTypeConfiguration : IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable("CatalogType", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseHiLo("catalog_type_hilo")
                .IsRequired();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
