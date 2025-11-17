module Models

open Store.Domain.Health
open Store.Domain.Health.Access

module Role =
  let convert (str: string)  =
      match str with
      | "AdminRole" -> Role.AdminRole
      | "ProfessionalRole" -> Role.ProfessionalRole
      | "CustomerRole" -> Role.CustomerRole
      | _ -> Role.Anon

  let toString role =
    match role with
    | Role.AdminRole -> "AdminRole"
    | Role.ProfessionalRole -> "ProfessionalRole"
    | Role.CustomerRole -> "CustomRole"
    | Role.Anon -> "Anon"
