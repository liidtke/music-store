 module Store.Infrastructure.AccountsPayable.AccountsPayableData
//
// open System.Linq
// open Store.Domain.Health.AccountsPayableService
// open Store.Domain.Health.AccountsPayable
// open Store.Infrastructure.AccountsPayable.DataModels
// open Store.Infrastructure.Context
// open MongoDB.Bson
// open MongoDB.Driver
// open MongoDB.Driver.Linq
//
// type PaymentRepository(mongoContext: IMongoContext) =
//   let collectionName = "hs_payments"
//
//   interface IPaymentRepository with
//     member _.Add(payment: Payment) =
//       async {
//
//         try
//           let collection = mongoContext.Collection<PaymentData> collectionName
//           do! collection.InsertOneAsync(payment |> mapFromPayment) |> Async.AwaitTask
//           return succeed payment
//         with ex ->
//           return fromException ex
//       }
//
//     member _.Update(payment: Payment) =
//       async {
//         try
//           let collection = mongoContext.Collection<PaymentData> collectionName
//           let paymentId = ObjectId(payment.id)
//
//           let filter =
//             MongoDB.Driver.Builders<PaymentData>.Filter.Eq((fun p -> p.id), paymentId)
//
//           let! result = collection.ReplaceOneAsync(filter, payment |> mapFromPayment) |> Async.AwaitTask
//
//           if result.ModifiedCount > 0L then
//             return succeed payment
//           else
//             return internalError "No document was updated"
//         with ex ->
//           return fromException ex
//       }
//
//     member this.Query(input: PaymentQueryInput) =
//       let collection = mongoContext.Collection<PaymentData> collectionName
//
//       let baseQuery =
//         collection.AsQueryable().Where(fun p -> p.organization = input.organizationId)
//
//       let query =
//         let q = baseQuery
//
//         let q =
//           match input.paymentStatus with
//           | Some status -> q.Where(fun p -> (p.status |> mapToPaymentStatus) = status)
//           | None -> q
//
//         let q =
//           match input.issuerId with
//           | Some issuerId -> q.Where(fun p -> p.documents.Any(fun d -> d.issuer.id = issuerId))
//           | None -> q
//
//         let q =
//           match input.beneficiaryId with
//           | Some beneficiaryId -> q.Where(fun p -> p.items.Any(fun i -> i.beneficiary.id = beneficiaryId))
//           | None -> q
//
//         let q =
//           match input.number with
//           | Some num -> q.Where(fun p -> p.documents.Any(fun d -> d.number = num))
//           | None -> q
//
//         let q =
//           match input.description with
//           | Some desc -> q.Where(fun p -> p.documents.Any(fun d -> d.description = input.description))
//           | None -> q
//
//         let q =
//           match input.startIssuedDate with
//           | Some date -> q.Where(fun p -> p.occurred >= date)
//           | None -> q
//
//         let q =
//           match input.startDueDate with
//           | Some date -> q.Where(fun p -> p.documents.Any(fun d -> d.dueDate >= date))
//           | None -> q
//
//         let q =
//           match input.documentCategoryId with
//           | Some catId -> q.Where(fun p -> p.documents.Any(fun d -> d.category.id = catId))
//           | None -> q
//
//         q
//       //end query
//
//       async {
//         try
//           let! rawData = query.ToListAsync() |> Async.AwaitTask
//           let payments = rawData |> Seq.map mapToPayment |> Seq.toList
//           return payments |> succeed
//         with ex ->
//           return fromException ex
//       }
//
//     member this.Find(input) =
//       let collection = mongoContext.Collection<PaymentData> collectionName
//
//       async {
//         let value = ObjectId.Parse(input)
//
//         try
//           let! result = collection.AsQueryable().FirstAsync(fun x -> x.id = value) |> Async.AwaitTask
//           return result |> mapToPayment |> succeed
//         with ex ->
//           return fromException ex
//       }
