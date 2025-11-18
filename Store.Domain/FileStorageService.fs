module Store.Domain.FileStorageService

open Store.Domain.FileStorage
open Store.Domain.Interfaces


type IFileMetaRepository =
  inherit IRepository<FileMeta>
  abstract member GetByIdentifier: string -> Async<ServiceOutput<List<FileMeta>>>

