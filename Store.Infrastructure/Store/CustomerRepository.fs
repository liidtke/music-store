module Store.Infrastructure.Store.CustomerRepository

open System.Linq
open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver.Linq
open Store.Domain.Store.Interfaces
open Store.Infrastructure.Context
open Store.Infrastructure.Store.Models

type CustomerRepository(ctx: IMongoContext, logger: ILogger<CustomerRepository>) =
  let collectionName = "customers"
  let collection = ctx.Collection<CustomerData>(collectionName)

  interface ICustomerRepository with
    member this.Add(product) =
      mapInsert
        logger
        CustomerMapping.toCustomerData
        CustomerMapping.toCustomer
        "Error adding customer info"
        collection.InsertOneAsync
        product

    member this.Update(product) =
      let filter =
        Builders<CustomerData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(product.id))

      let replaceAsync filter data =
        collection.ReplaceOneAsync(filter, data)

      mapUpdate logger CustomerMapping.toCustomerData "Error updating customer info" replaceAsync filter product

    member this.Find(id: string) =
      let filter =
        Builders<CustomerData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(id))

      let effect () =
        collection.Find(filter).FirstOrDefaultAsync()

      mapFind logger CustomerMapping.toCustomer "Error finding customer info" effect
