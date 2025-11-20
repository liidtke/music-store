module Store.Infrastructure.Store.ProductTypeRepository

open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open Store.Domain.Store.Interfaces
open Store.Domain.Store.Catalog
open Store.Infrastructure.Store.Models
open Store.Infrastructure.Context

type ProductTypeRepository(ctx: IMongoContext, logger: ILogger<ProductTypeRepository>) =
  let name = "product_types"
  let collection = ctx.Collection<ProductTypeData>(name)

  interface IProductTypeRepository with
    member this.Add(productType) =
      mapInsert
        logger
        ProductTypeMapping.toProductTypeData
        ProductTypeMapping.toProductType
        "Error adding product type"
        collection.InsertOneAsync
        productType

    member this.Update(productType) =
      let filter =
        Builders<ProductTypeData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(productType.id))

      let f a b = collection.ReplaceOneAsync(a, b)
      mapUpdate logger ProductTypeMapping.toProductTypeData "Error updating product type" f filter productType

    member this.Find(productTypeId) =
      let query id =
        let filter = Builders<ProductTypeData>.Filter.Eq((fun p -> p.id), id)

        let effect () =
          collection.Find(filter).FirstOrDefaultAsync()

        mapFind logger ProductTypeMapping.toProductType "Error finding product type" effect

      query |> parseExec productTypeId

    member this.All() =
      let effect () =
        collection.Find(Builders<ProductTypeData>.Filter.Eq((fun p -> p.isActive), true)).ToListAsync()

      mapFetch logger ProductTypeMapping.toProductType "Error listing all professionals" effect
