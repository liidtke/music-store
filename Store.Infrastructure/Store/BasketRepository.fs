module Store.Infrastructure.Store.BasketRepository

open System.Linq
open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver.Linq
open Store.Domain.Store.Interfaces
open Store.Infrastructure.Context
open Store.Infrastructure.Store.Models

type BasketRepository(ctx: IMongoContext, logger: ILogger<BasketRepository>) =
  let collectionName = "baskets"
  let collection = ctx.Collection<BasketData>(collectionName)

  interface IBasketRepository with
    member this.Add(product) =
      
      let basket = if product.id = "" then {product with id = BsonObjectId.Empty.ToString()} else product

      let filter =
        Builders<BasketData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(basket.id))

      try
        collection.DeleteOne(filter) |> ignore
      with err ->
        logger.LogWarning("error deleting basket")

      mapInsert
        logger
        BasketMapping.toBasketData
        BasketMapping.toBasket
        "Error adding customer info"
        collection.InsertOneAsync
        basket

    member this.Update(product) =
      let filter =
        Builders<BasketData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(product.id))

      let replaceAsync filter data =
        collection.ReplaceOneAsync(filter, data)

      mapUpdate logger BasketMapping.toBasketData "Error updating customer info" replaceAsync filter product

    member this.Find(id: string) =
      if id = "" then validationAsync "Basket id cannot be empty" else
        let filter =
          Builders<BasketData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(id))

        let effect () =
          collection.Find(filter).FirstOrDefaultAsync()

        mapFind logger BasketMapping.toBasket "Error finding customer info" effect
