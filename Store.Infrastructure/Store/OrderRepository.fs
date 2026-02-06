module Store.Infrastructure.Store.OrderRepository



open System.Linq
open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver.Linq
open Store.Domain.Store.Interfaces
open Store.Infrastructure.Context
open Store.Infrastructure.Store.Models

type OrderRepository(ctx: IMongoContext, logger: ILogger<OrderRepository>) =
  let collectionName = "orders"
  let collection = ctx.Collection<OrderData>(collectionName)
  let filterBuilder = Builders<OrderData>.Filter

  interface IOrderRepository with
    member this.Add(order) =
      let basket =
        if order.id = "" then
          { order with
              id = BsonObjectId.Empty.ToString() }
        else
          order

      let filter = filterBuilder.Eq((fun p -> p.id), BsonObjectId.Create(basket.id))

      try
        collection.DeleteOne(filter) |> ignore
      with err ->
        logger.LogWarning("error deleting order")

      mapInsert
        logger
        OrderMapping.toOrderData
        OrderMapping.toOrder
        "Error adding order info"
        collection.InsertOneAsync
        order

    member this.Find(id) =
      if id = "" then
        validationAsync "Basket id cannot be empty"
      else
        let filter = filterBuilder.Eq((fun p -> p.id), BsonObjectId.Create(id))

        let effect () =
          collection.Find(filter).FirstOrDefaultAsync()

        mapFind logger OrderMapping.toOrder "Error finding customer info" effect

    member this.Update(order) =
      let filter = filterBuilder.Eq((fun p -> p.id), BsonObjectId.Create(order.id))

      let replaceAsync filter data =
        collection.ReplaceOneAsync(filter, data)

      mapUpdate logger OrderMapping.toOrderData "Error updating customer info" replaceAsync filter order

    member this.Fetch(crit) =
      let query = collection.AsQueryable()

      
      let query =
        if crit.status.IsSome then
          query.Where(fun p -> p.status = crit.status.Value)
        else
          query

      let query =
        if crit.userId.IsSome then
          query.Where(fun p -> p.customer.userId = crit.userId.Value)
        else
          query

      let query =
        if crit.startDate.IsSome then
          query.Where(fun p -> p.occurred >= crit.startDate.Value)
        else
          query

      let query =
        if crit.paymentStatus.IsSome then
          query.Where(fun p -> p.paymentStatus = crit.paymentStatus.Value)
        else
          query

      let query =
        if crit.order.IsSome then
          query.Where(fun p -> p.id = BsonObjectId.Create(crit.order.Value))
        else
          query

      let effect () =
        query.OrderByDescending(fun x -> x.occurred).Take(crit.limit).ToListAsync()

      mapFetch logger OrderMapping.toOrder "Error fetching orders" effect
