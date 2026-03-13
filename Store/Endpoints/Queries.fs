module Store.Endpoints.Queries

open Store.Domain
open Store.Domain.Store.Interfaces

type ProfessionalQuery(repo: IProfessionalRepository) =
  member _.GetOne (us: UserSession) (id: string) =
    if us.role = Role.AdminRole || us.role = Role.ProfessionalRole then
      repo.Find(id)
    else
      async { return invalidate "Access Denied" }

  member _.GetOnePublic (us: UserSession) (id: string) =
    async {
      let! result = repo.Find(id)
      return result ?> (fun r -> r |> Models.ProfessionalPublicModel.map |> succeed)
    }

  member _.GetAllPrivate(us: UserSession) =
    try
      if us.role = Role.AdminRole || us.role = Role.ProfessionalRole then
        async {
          let! result = repo.All()
          return result
        }
      else
        async { return invalidate "Access Denied" }
    with ex ->
      async { return invalidate ex.Message }

  member _.GetAllPublic(us: UserSession) =
    async {
      let! result = repo.AllActive()
      return result ?> (fun r -> r |> List.map Models.ProfessionalPublicModel.map |> succeed)

    }


type OrderQuery(repo: IOrderRepository) =
  member _.GetMine(us: UserSession) = repo.Find(us.userId)

  member _.Get(id: string, us: UserSession) =
    async {
      let! result = repo.Find(id)

      match result with
      | Success order ->
        if us.role = Role.AdminRole || us.role = Role.ProfessionalRole then
          return result
        elif order.customer.userId = us.userId then
          return result
        else
          return invalidate "Access Denied"
      | Failure errorResult -> return result
    }

  member _.GetAll(input: OrderSearchCriteria, us: UserSession) =
    if input.userId.IsSome && input.userId.Value = us.userId then
      repo.Fetch(input)
    elif us.role = Role.AdminRole || us.role = Role.ProfessionalRole then
      repo.Fetch(input)
    else
      repo.Fetch({ input with userId = Some(us.userId) })