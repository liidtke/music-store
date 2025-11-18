module Store.Infrastructure.Store.ProfessionalRepository

open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open Store.Domain.Store.Interfaces
open Store.Infrastructure.Context
open Store.Infrastructure.Store.Models

type ProfessionalRepository(ctx: IMongoContext, logger: ILogger<ProfessionalRepository>) =
  let collectionName = "professionals"
  let collection = ctx.Collection<ProfessionalData>(collectionName)

  interface IProfessionalRepository with

    member this.Add(professional) =
      mapInsert
        logger
        ProfessionalMapping.toProfessionalData
        ProfessionalMapping.toProfessional
        "Error adding professional"
        collection.InsertOneAsync
        professional

    member this.Update(professional) =
      let filter =
        Builders<ProfessionalData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(professional.id))

      let replaceAsync filter data =
        collection.ReplaceOneAsync(filter, data)

      mapUpdate
        logger
        ProfessionalMapping.toProfessionalData
        "Error updating professional"
        replaceAsync
        filter
        professional

    member this.Find(id: string) =
      let filter =
        Builders<ProfessionalData>.Filter.Eq((fun p -> p.id), BsonObjectId.Create(id))

      let effect () =
        collection.Find(filter).FirstOrDefaultAsync()

      mapFind logger ProfessionalMapping.toProfessional "Error finding professional" effect

    member this.All() =
      let effect () =
        collection.Find(Builders<ProfessionalData>.Filter.Eq((fun p -> p.isActive), true)).ToListAsync()

      mapFetch logger ProfessionalMapping.toProfessional "Error listing all professionals" effect
