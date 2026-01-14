module Store.Infrastructure.Serializers

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
        | None   -> context.Writer.WriteNull()


    override _.Deserialize(context, args) =
        match context.Reader.GetCurrentBsonType() with
        | BsonType.Null ->
            context.Reader.ReadNull()
            None

        | BsonType.String ->
            Some (context.Reader.ReadString())

        | t ->
            // You can make this stricter or more permissive
            // For robustness we accept string or null only
            sprintf "Expected string or null for string option, got %A" t
            |> BsonSerializationException
            |> raise


// ────────────────────────────────────────────────────────────────
// Registration (do this early – ideally at app startup)

