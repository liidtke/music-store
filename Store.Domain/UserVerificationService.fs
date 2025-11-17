module Store.Domain.Health.UserVerificationService

open System

type GetByUserEmail = FetchEffect<string, UserVerification>
type SaveEffect = Effect<UserVerification>
type GenerateSaltEffect = int -> string
type GenerateCodeEffect = string -> string -> string //salt

let requestPasswordReset
  (email: string)
  (getUserEffect: UserService.Effects.GetUserByEmail)
  (saveEffect: SaveEffect)
  (sEff: GenerateSaltEffect)
  (cEff: GenerateCodeEffect)
  =
  let salt = sEff 16
  let hash = cEff salt email

  async {
    let! userOut = getUserEffect email

    match userOut with
    | Success user ->
      let verification: UserVerification =
        { id = 0
          expiresIn = DateTime.UtcNow.AddHours(1)
          hash = Some(hash)
          user = Some(user)
          isUsed = false
          userId = user.id
          verificationCode = salt }

      return! saveEffect verification
    | Failure errorResult -> return failure errorResult
  }

let decode (email: string) (code: string) (cEff: GenerateCodeEffect) =
  let salt = code
  let hash = cEff salt email
  hash

let confirmChangePassword
  (email: string)
  (hash: string)
  (get: GetByUserEmail)
  (saveEffect: SaveEffect)
  (cEff: GenerateCodeEffect)
  =
  let getHash salt = decode email salt cEff

  async {
    let! uvs = get email

    let! result =
      match uvs with
      | Success userVerification ->
        let any =
          userVerification |> List.tryFind (fun uv -> hash = getHash uv.verificationCode && (not uv.isUsed) && (DateTime.UtcNow <= uv.expiresIn))

        match any with
        | None -> async { return invalidate "Não encontrado" }
        | Some value -> { value with isUsed = true } |> saveEffect
      //succeed userVerification
      | Failure errorResult -> async { return invalidate "Não Encontrado" }

    return result
  }
