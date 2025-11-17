module Store.Domain.FileStorageService

open Store.Domain.Health.FileStorage
open Store.Domain.Health.Interfaces




type IFileMetaRepository =
  inherit IRepository<FileMeta>
  abstract member GetByIdentifier: string -> Async<ServiceOutput<List<FileMeta>>>

