module Store.Infrastructure.Serializers

open System
open MongoDB.Bson
open MongoDB.Bson.Serialization
open MongoDB.Bson.Serialization.Serializers


/// Serializes string option → string | null
/// Deserializes string | null → string option
type StringOptionSerializer() =
  inherit SerializerBase<string option>()

  override _.Serialize(context, args, value) =
    match value with
    | Some s -> context.Writer.WriteString(s)
    | None -> context.Writer.WriteNull()


  override _.Deserialize(context, args) =
    match context.Reader.GetCurrentBsonType() with
    | BsonType.Null ->
      context.Reader.ReadNull()
      None

    | BsonType.String -> Some(context.Reader.ReadString())

    | t ->
      // You can make this stricter or more permissive
      // For robustness we accept string or null only
      sprintf "Expected string or null for string option, got %A" t
      |> BsonSerializationException
      |> raise



/// Generic serializer for F# 'T list → BSON Array
/// Avoids many compatibility / reflection issues
type FSharpListSerializer<'T>() =
  inherit SerializerBase<'T list>()

  // We delegate item serialization to the driver's own serializer for 'T
  let itemSerializer = lazy BsonSerializer.LookupSerializer<'T>()

  override _.Serialize(context, args, value) =
    context.Writer.WriteStartArray()

    for item in value do
      itemSerializer.Value.Serialize(context, item)

    context.Writer.WriteEndArray()


  override _.Deserialize(context, args) =
    if context.Reader.CurrentBsonType <> BsonType.Array then
      []
    else
      context.Reader.ReadStartArray()

      let mutable acc = []

      while context.Reader.ReadBsonType() <> BsonType.EndOfDocument do
        let item = itemSerializer.Value.Deserialize(context)
        acc <- item :: acc

      context.Reader.ReadEndArray()

      List.rev acc // important — we built in reverse
open MongoDB.Bson.Serialization.Conventions


type FSharpListConvention() =
    interface IMemberMapConvention with
        member _.Name = "F# list → array serializer"

        member _.Apply(map) =
            if map.MemberType.IsGenericType &&
               map.MemberType.GetGenericTypeDefinition() = typedefof<list<_>> then

                let itemType = map.MemberType.GetGenericArguments().[0]
                let serializerType = typedefof<FSharpListSerializer<_>>.MakeGenericType(itemType)
                let serializer = Activator.CreateInstance(serializerType) :?> IBsonSerializer

                map.SetSerializer(serializer) |> ignore

