using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moongazing.CatalogService.Api.Core.Domain.Entities;
using Moongazing.CatalogService.Api.Infrastructure.Context;

namespace Moongazing.CatalogService.Api.Infrastructure.EntityConfiguration
{
    public class CatalogBrandEntityTypeConfiguration : IEntityTypeConfiguration<CatalogBrand>
    {
        public void Configure(EntityTypeBuilder<CatalogBrand> builder)
        {
            builder.ToTable("CatalogBrand", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .UseHiLo("catalog_brand_hilo")
                .IsRequired();

            builder.Property(x => x.Brand)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
