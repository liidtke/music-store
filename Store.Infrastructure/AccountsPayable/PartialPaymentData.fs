 module Store.Infrastructure.AccountsPayable.PartialPaymentData
//
// open System
// open System.Linq
// open Store.Domain.Health.PartialPaymentService
// open Store.Infrastructure.Context
// open MongoDB.Bson
// open MongoDB.Bson.Serialization.Attributes
// open MongoDB.Driver
// open MongoDB.Driver.Linq
//
// [<RequireQualifiedAccess>]
// module Models =
//   open Store.Infrastructure.AccountsPayable.DataModels
//   type PartialPersonData = { id: int; name: string }
//
//   type PartialInstallmentData =
//     { id: Guid
//       value: decimal option
//       target: Guid option
//       dueDate: DateTime option
//       installmentType: InstallmentTypeData }
//
//   type PartialDocumentData =
//     { id: Guid
//       issuer: PartialPersonData option
//       value: decimal option
//       number: string option
//       category: DocumentCategoryData option
//       installments: PartialInstallmentData list
//       description: string option
//       dueDate: DateTime option
//       issuedDate: DateTime option }
//
//   type PartialPaymentItemData =
//     { id: Guid
//       value: decimal option
//       installments: PartialInstallmentData list
//       paymentMethod: PaymentMethodData option
//       beneficiary: PartialPersonData option
//       paymentDate: DateTime option }
//
//   type PartialPaymentData =
//     { [<BsonId>]
//       id: ObjectId
//       isCreated: bool
//       identifier: Guid
//       createdBy: int
//       occurred: DateTime
//       organization: int
//       documents: PartialDocumentData list
//       items: PartialPaymentItemData list
//       totalValue: decimal option }
//
//   // Mapping functions from domain to data
//   let mapFromPartialPerson (pp: PartialPerson) : PartialPersonData = { id = pp.id; name = pp.name }
//
//   let mapToPartialPerson (ppd: PartialPersonData) : PartialPerson = { id = ppd.id; name = ppd.name }
//
//   let private mapFromPartialInstallment (pi: PartialInstallment) : PartialInstallmentData =
//     { id = pi.id
//       value = pi.value
//       target = pi.target
//       dueDate = pi.dueDate
//       installmentType = mapFromInstallmentType pi.installmentType }
//
//   let private mapToPartialInstallment (pid: PartialInstallmentData) : PartialInstallment =
//     { id = pid.id
//       value = pid.value
//       target = pid.target
//       dueDate = pid.dueDate
//       installmentType = mapToInstallmentType pid.installmentType }
//
//   let private mapFromPartialDocument (pd: PartialDocument) : PartialDocumentData =
//     { id = pd.id
//       issuer = pd.issuer |> Option.map mapFromPartialPerson
//       value = pd.value
//       number = pd.number
//       category = pd.category |> Option.map mapFromDocumentCategory
//       installments = List.map mapFromPartialInstallment pd.installments
//       description = pd.description
//       dueDate = pd.dueDate
//       issuedDate = pd.issuedDate }
//
//   let private mapToPartialDocument (pdd: PartialDocumentData) : PartialDocument =
//     { id = pdd.id
//       issuer = pdd.issuer |> Option.map mapToPartialPerson
//       value = pdd.value
//       number = pdd.number
//       category = pdd.category |> Option.map mapToDocumentCategory
//       installments = List.map mapToPartialInstallment pdd.installments
//       description = pdd.description
//       dueDate = pdd.dueDate
//       issuedDate = pdd.issuedDate }
//
//   let private mapFromPartialPaymentItem (ppi: PartialPaymentItem) : PartialPaymentItemData =
//     { id = ppi.id
//       value = ppi.value
//       installments = List.map mapFromPartialInstallment ppi.installments
//       paymentMethod = ppi.paymentMethod |> Option.map mapFromPaymentMethod
//       beneficiary = ppi.beneficiary |> Option.map mapFromPartialPerson
//       paymentDate = ppi.paymentDate }
//
//   let private mapToPartialPaymentItem (ppid: PartialPaymentItemData) : PartialPaymentItem =
//     { id = ppid.id
//       value = ppid.value
//       installments = List.map mapToPartialInstallment ppid.installments
//       paymentMethod = ppid.paymentMethod |> Option.map mapToPaymentMethod
//       beneficiary = ppid.beneficiary |> Option.map mapToPartialPerson
//       paymentDate = ppid.paymentDate }
//
//   let mapFromPartialPayment (pp: PartialPayment) : PartialPaymentData =
//     { id = ObjectId(pp.id)
//       identifier = pp.identifier
//       isCreated = pp.isCreated
//       createdBy = pp.createdBy
//       occurred = pp.occurred
//       organization = pp.organizationId
//       documents = List.map mapFromPartialDocument pp.documents
//       items = List.map mapFromPartialPaymentItem pp.items
//       totalValue = pp.totalValue }
//
//   let mapToPartialPayment (ppd: PartialPaymentData) : PartialPayment =
//     { id = ppd.id.ToString()
//       identifier = ppd.identifier
//       createdBy = ppd.createdBy
//       isCreated = ppd.isCreated
//       occurred = ppd.occurred
//       organizationId = ppd.organization
//       documents = List.map mapToPartialDocument ppd.documents
//       items = List.map mapToPartialPaymentItem ppd.items
//       totalValue = ppd.totalValue }
//
//
//
//
// type PaymentRepository(mongoContext: IMongoContext) =
//   let collectionName = "hs_payments"
//
//   interface IPartialPaymentRepository with
//     member _.Add(payment: PartialPayment) =
//       async {
//
//         try
//           let collection = mongoContext.Collection<Models.PartialPaymentData> collectionName
//
//           do!
//             collection.InsertOneAsync(payment |> Models.mapFromPartialPayment)
//             |> Async.AwaitTask
//
//           return succeed payment
//         with ex ->
//           return fromException ex
//       }
//
//     member _.Update(payment: PartialPayment) =
//       async {
//         try
//           let collection = mongoContext.Collection<Models.PartialPaymentData> collectionName
//           let paymentId = ObjectId(payment.id)
//
//           let filter =
//             MongoDB.Driver.Builders<Models.PartialPaymentData>.Filter
//               .Eq((fun p -> p.id), paymentId)
//
//           let! result =
//             collection.ReplaceOneAsync(filter, payment |> Models.mapFromPartialPayment)
//             |> Async.AwaitTask
//
//           if result.ModifiedCount > 0L then
//             return succeed payment
//           else
//             return internalError "No document was updated"
//         with ex ->
//           return fromException ex
//       }
//
//     member this.Get(identifier) =
//       let collection = mongoContext.Collection<Models.PartialPaymentData> collectionName
//
//       let baseQuery = collection.AsQueryable().Where(fun p -> p.identifier = identifier)
//
//       async {
//         try
//           let! rawData = baseQuery.ToListAsync() |> Async.AwaitTask
//           let payments = rawData |> Seq.map Models.mapToPartialPayment |> Seq.toList
//
//           if payments.Length = 0 then
//             return notFound "Partial not found"
//           else
//             return payments[0] |> succeed
//         with ex ->
//           return fromException ex
//       }
//
//     member this.Find(var0) = failwith "todo"
