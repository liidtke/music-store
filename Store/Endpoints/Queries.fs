module Store.Endpoints.Queries

open Store.Domain
open Store.Domain.Store.Interfaces

type ProfessionalQuery(repo: IProfessionalRepository) =
  member _.GetOne (us: UserSession) (id: string) =
    if us.role = Role.AdminRole || us.role = Role.ProfessionalRole then
      repo.Find(id)
    else
      async { return invalidate "Access Denied" }

  member _.GetAllPrivate(us: UserSession) =
    try
      if us.role = Role.AdminRole || us.role = Role.ProfessionalRole then
        async {
          let! result = repo.All()
          return result
        }
      else
        async { return invalidate "Access Denied" }
    with ex -> async { return invalidate ex.Message }

  member _.GetAllPublic(us: UserSession) =
    async {
      let! result = repo.AllActive()
      return result ?> (fun r -> r |> List.map Models.ProfessionalPublicModel.map |> succeed)

    }
