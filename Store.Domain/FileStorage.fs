module Store.Domain.FileStorage

open System

// type StorageLimit =
//   { subscription: Subscription
//     fileSize: int //max individual file size
//     storageSize: int } //total combined size per month
//
// module StorageLimit =
//   let basic =
//     { subscription = Subscription.Basic
//       fileSize = 2024
//       storageSize = 100000 }
//
//   let advanced =
//     { subscription = Subscription.Advanced
//       fileSize = 5400
//       storageSize = 300000 }
//
// type StorageStatus =
//   { id: string
//     period: DateTime //month
//     qty: int
//     size: int
//     organizationId: int }

type FileStorageConfiguration =
  { endpoint: string
    accessKey: string
    keySecret: string
    bucket: string
    storageClass: string }

type FileMeta =
  { id: string
    identifier: string //source id
    objectId: string //id in object storage
    name: string
    extension: string
    mimeType: string
    occurred: DateTime
    createdBy: int
    size: int //kb
    organizationId: int option }
