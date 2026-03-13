module Store.Infrastructure.Store.ProductRepository

open System.Linq
open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver.Linq
open Store.Domain.Store.Interfaces
open Store.Infrastructure.Context
open Store.Infrastructure.Store.Models

type ProductRepository(ctx: IMongoContext, logger: ILogger<ProductRepository>) =
  let collectionName = "products"
  let collection = ctx.Collection<ProductData>(collectionName)

  interface IProductRepository with
    member this.Add(product) =
      mapInsert
        logger
        ProductMapping.toProductData
        ProductMapping.toProduct
        "Error adding product"
        collection.InsertOneAsync
        product

    member this.Update(product) =
      let filter =
        Builders<ProductData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(product.id))

      let replaceAsync filter data =
        collection.ReplaceOneAsync(filter, data)

      mapUpdate logger ProductMapping.toProductData "Error updating product" replaceAsync filter product

    member this.Find(id: string) =
      let filter =
        Builders<ProductData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(id))

      let effect () =
        collection.Find(filter).FirstOrDefaultAsync()

      mapFind logger ProductMapping.toProduct "Error finding product" effect

    member this.All() =
      let effect () =
        collection.Find(Builders<ProductData>.Filter.Eq((fun p -> p.isActive), true)).ToListAsync()

      mapFetch logger ProductMapping.toProduct "Error listing all products" effect

    member this.Fetch(crit) =
      let query = collection.AsQueryable().Where(fun p -> p.isActive)

      let query =
        if crit.search.IsSome then
          query.Where(fun p -> p.name.ToLower().Contains(crit.search.Value.ToLower()))
        else
          query

      let query =
        if crit.productTypeId.IsSome then
          query.Where(fun p -> p.productType.id = BsonObjectId.Create(crit.productTypeId.Value))
        else
          query

      let query =
        if crit.professionalId.IsSome then
          query.Where(fun p -> p.professionalId = BsonObjectId.Create(crit.professionalId.Value))
        else
          query

      let effect () = query.Take(crit.limit).ToListAsync()
      mapFetch logger ProductMapping.toProduct "Error listing products" effect
