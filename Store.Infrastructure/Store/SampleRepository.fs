module Store.Infrastructure.Store.SampleRepository

open Microsoft.Extensions.Logging
open MongoDB.Bson
open MongoDB.Driver
open Store.Domain.Store.Catalog
open Store.Domain.Store.Interfaces
open Store.Infrastructure.Context
open Store.Infrastructure.Store.Models


type SampleRepository(ctx: IMongoContext, logger: ILogger<SampleRepository>) =
  let collectionName = "samples"
  let collection = ctx.Collection<SampleData>(collectionName)

  // Supondo uma coleção separada para os arquivos binários para não pesar na busca de listagem
  let fileCollectionName = "sample_files"
  let fileCollection = ctx.Collection<SampleFileData>(fileCollectionName)

  interface ISampleRepository with
    member this.Add(sample) =
      mapInsert
        logger
        SampleMapping.toSampleData
        SampleMapping.toSample
        "Error adding sample"
        collection.InsertOneAsync
        sample

    member this.Update(sample) =
      let filter =
        Builders<SampleData>.Filter.Eq((fun s -> s.id), BsonObjectId.Create(sample.id))

      let replaceAsync filter data =
        collection.ReplaceOneAsync(filter, data)

      mapUpdate logger SampleMapping.toSampleData "Error updating sample" replaceAsync filter sample

    member this.Find(id: string) =
      let filter =
        Builders<SampleData>.Filter.Eq((fun s -> s.id), BsonObjectId.Create(id))

      let effect () =
        collection.Find(filter).FirstOrDefaultAsync()

      mapFind logger SampleMapping.toSample "Error finding sample" effect

    member this.Fetch(input: SampleSearchCriteria) =
      let filter =
        if input.productId.IsSome then
          Builders<SampleData>.Filter.Eq((fun s -> s.productId), BsonObjectId.Create(input.productId.Value))
        elif input.professionalId.IsSome then
          Builders<SampleData>.Filter.Eq((fun s -> s.professionalId), BsonObjectId.Create(input.professionalId.Value))
        else
          raise (System.Exception("Invalid search criteria"))

      let effect () = collection.Find(filter).ToListAsync()

      mapFetch logger SampleMapping.toSample "Error listing samples" effect

    member this.FetchFile(id: string) =
      let filter =
        Builders<SampleFileData>.Filter.Eq((fun f -> f.id), BsonObjectId.Create(id))

      let effect () =
        fileCollection.Find(filter).FirstOrDefaultAsync()

      mapFind logger SampleMapping.toSampleFile "Error finding sample file" effect

    member this.Delete(id) =
      let filterFile =
        Builders<SampleFileData>.Filter.Eq((fun f -> f.id), BsonObjectId.Create(id))

      let filter =
        Builders<SampleData>.Filter.Eq((fun f -> f.id), BsonObjectId.Create(id))

      async {
        try
          let! _ = fileCollection.DeleteOneAsync(filterFile) |> Async.AwaitTask
          let! _ = collection.DeleteOneAsync(filter) |> Async.AwaitTask
          return succeed ()
        with ex ->
          logger.LogError(ex, "Error deleting sample") |> ignore
          return fail "Error deleting"
      }

    member this.SaveAll(sample, file) =
      async {
        let! saveOutput =
          mapInsert
            logger
            SampleMapping.toSampleData
            SampleMapping.toSample
            "Error adding sample"
            collection.InsertOneAsync
            sample

        let! saveFileOutput =
          saveOutput
          ?>< (fun s ->
            { file with id = s.id }
            |> mapInsert
              logger
              SampleMapping.toSampleFileData
              SampleMapping.toSampleFile
              "Error adding sample file"
              fileCollection.InsertOneAsync)

        return saveOutput

      }
