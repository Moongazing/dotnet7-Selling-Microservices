using Microsoft.Data.SqlClient;
using Moongazing.CatalogService.Api.Core.Domain.Entities;
using Polly;
using System.Globalization;
using System.IO.Compression;
using System.Linq;

namespace Moongazing.CatalogService.Api.Infrastructure.Context
{
    public class CatalogContextSeed
    {
        public async Task SeedAsync(CatalogContext context, IWebHostEnvironment env, ILogger<CatalogContextSeed> logger)
        {

            var policy = Policy.Handle<SqlException>()
                 .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                onRetry: (exception, timespan, rerty, ctx) =>
                {
                    //log
                });
            var setupDirPath = Path.Combine(env.ContentRootPath, "Infrastructure", "Setup", "SeedFiles");
            var picturePath = "Pics";

            await policy.ExecuteAsync(() => ProcessSeeding(context, setupDirPath, picturePath, logger);
        }
        private async Task ProcessSeeding(CatalogContext context, string setupDirPath, string picturePath, ILogger logger)
        {
            if (!context.CatalogBrands.Any())
            {
                await context.CatalogBrands.AddRangeAsync(GetCatalogBrandsFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }
            if (!context.CatalogTypes.Any())
            {
                await context.CatalogTypes.AddRangeAsync(GetCatalogTypesFromFile(setupDirPath));
                await context.SaveChangesAsync();
            }
            if (!context.CatalogItems.Any())
            {
                await context.CatalogItems.AddRangeAsync(GetCatalogItemsFromFile(setupDirPath, context));
                await context.SaveChangesAsync();
                GetCatalogItemsPictures(setupDirPath, picturePath);
            }
        }
        private IEnumerable<CatalogBrand> GetCatalogBrandsFromFile(string contentPath)
        {
            IEnumerable<CatalogBrand> GetPreconfiguredCatalogBrands()
            {
                return new List<CatalogBrand>
                {
                     new CatalogBrand() { Brand = "Azure" },
                     new CatalogBrand() { Brand = ".NET" },
                     new CatalogBrand() { Brand = "SQL Server" },
                };
            }
            string fileName = Path.Combine(contentPath, "BrandsTextFile.txt");
            if (!File.Exists(fileName))
            {
                return GetPreconfiguredCatalogBrands();
            }
            var fileContent = File.ReadAllLines(fileName);
            var list = fileContent.Select(i => new CatalogType()
            {
                Type = i.Trim('"')
            }).Where(i => i != null);

            return list ?? GetPreconfiguredCatalogBrands();
        }
        private IEnumerable<CatalogType> GetCatalogTypesFromFile(string contentPath)
        {
            IEnumerable<CatalogType> GetPreconfiguredCatalogTypes()
            {
                return new List<CatalogType>
                {
                     new CatalogType() { Type = "Mug" },
                     new CatalogType() { Type = "T-Shirt" },
                     new CatalogType() { Type = "Sheet" },
                     new CatalogType() { Type = "USB Memory Stick" }

                };
            }
            string fileName = Path.Combine(contentPath, "CatalogTypes.txt");
            if (!File.Exists(fileName))
            {
                return GetPreconfiguredCatalogTypes();
            }
            var fileContent = File.ReadAllLines(fileName);
            var list = fileContent.Select(i => new CatalogType()
            {
                Type = i.Trim('"')
            }).Where(i => i != null);

            return list ?? GetPreconfiguredCatalogTypes();
        }
        private IEnumerable<CatalogItem> GetCatalogItemsFromFile(string contentPath, CatalogContext context)
        {
            IEnumerable<CatalogItem> GetPreconfiguredItems()
            {
                return new List<CatalogItem>()
                {
                    new CatalogItem{ CatalogTypeId = 2, CatalogBrandId=2, AvailableStock= 100, Description=".NET Bot Black Hoddie, and more",Name="T-Shirt",OnReorder=false,PictureFileName= "19.5.1.png"}

                };
            }


            string fileName = Path.Combine(contentPath, "CatalogItems.txt");
            if (!File.Exists(fileName))
            {
                return GetPreconfiguredItems();
            }
            var catalogTypeIdLookup = context.CatalogTypes.ToDictionary(x => x.Type, x => x.Id);
            var catalogBrandIdLookup = context.CatalogBrands.ToDictionary(x => x.Brand, x => x.Id);

            var fileContent = File.ReadAllLines(fileName)
                .Skip(1)
                .Select(i => i.Split(","))
                .Select(i => new CatalogItem()
                {
                    CatalogTypeId = catalogTypeIdLookup[i[0]],
                    CatalogBrandId = catalogTypeIdLookup[i[1]],
                    Description = i[2].Trim('"').Trim(),
                    Name = i[3].Trim('"').Trim(),
                    Price = Decimal.Parse(i[4].Trim('"'), System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
                    PictureFileName = i[5].Trim('"').Trim(),
                    AvailableStock = string.IsNullOrEmpty(i[6]) ? 0 : int.Parse(i[6]),
                    OnReorder = Convert.ToBoolean(i[7])
                });
            return fileContent;
        }
        private void GetCatalogItemsPictures(string contentPath, string picturePath)
        {
            picturePath ??= "pics";
            if (picturePath != null)
            {
                DirectoryInfo directory = new DirectoryInfo(picturePath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
                string zipFileCatalogItemPictures = Path.Combine(contentPath, "CatalogItems.zip");
                ZipFile.ExtractToDirectory(zipFileCatalogItemPictures, picturePath);
            }
        }
    }
}
