module Store.Security

open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Security.Cryptography
open System.Text
open Store.Domain
open Microsoft.AspNetCore.Authentication
open Microsoft.IdentityModel.Tokens

module Crypto =
  open System.Security.Cryptography

  /// Makes byte[] from Base64 string.
  let private bytesFromBase64 (str: string) = Convert.FromBase64String str

  /// Makes Base64 string from byte[].
  let private bytesToBase64 (bytes: byte[]) = Convert.ToBase64String bytes

  /// Generates a random Int32 between range.
  let randomInt min max =
    RandomNumberGenerator.GetInt32(min, max)

  /// Generates cryptographically-sound random salt.
  ///
  /// Example: `createSalt 16 (generates a 128-bit (i.e. 128 / 8) salt).`
  let createSalt len =
    let rndAry = Array.zeroCreate<byte> len
    use rng = RandomNumberGenerator.Create()
    rng.GetBytes rndAry
    rndAry |> bytesToBase64

  /// Performs key derivation using the provided algorithm.
  let pbkdf2 (algo: HashAlgorithmName) (iterations: int) (numBytesRequested: int) (salt: byte[]) (input: byte[]) =
    let bytes = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, algo, numBytesRequested)
    bytesToBase64 bytes

  /// Performs PBKDF2 key derivation using HMACSHA256.
  let sha256 (iterations: int) (numBytesRequested: int) (salt: string) (strToHash: string) =
    pbkdf2
      HashAlgorithmName.SHA256
      iterations
      numBytesRequested
      (Encoding.UTF8.GetBytes salt)
      (Encoding.UTF8.GetBytes strToHash)

  /// Performs key derivation using HMACSHA512.
  let sha512 (iterations: int) (numBytesRequested: int) (salt: string) (strToHash: string) =
    pbkdf2
      HashAlgorithmName.SHA512
      iterations
      numBytesRequested
      (Encoding.UTF8.GetBytes salt)
      (Encoding.UTF8.GetBytes strToHash)

type Token =
  { token: string
    token_type: string
    expiration: DateTime }

type SecuritySettings =
  { Iterations: int32
    JwtSecurityKey: string
    TokenExpirationTime: int32
    JwtIssuer: string
    JwtAudience: string }

let getClaims (user: User) =
  [
    // Claim("Email", user.email)
    // Claim("Name", user.name)
    Claim("UserId", user.id.ToString())
    Claim("Role", user.role.ToString())
    Claim(ClaimTypes.Role, (if user.isVerified then "User" else "Anon")) ]

let authConfig (scheme: string) (options: AuthenticationOptions) = options.DefaultScheme <- scheme

let getProperties =
  let p = new AuthenticationProperties()
  p.ExpiresUtc <- System.Nullable()
  p


let generateToken (config: SecuritySettings) (user: User) =
  let symmetricKey =
    config.JwtSecurityKey |> Encoding.UTF8.GetBytes |> SymmetricSecurityKey

  let credentials = SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256)

  let expiration = config.TokenExpirationTime |> DateTime.Now.AddHours

  let notBefore = DateTime.Now

  let claims = getClaims user

  let securityToken =
    JwtSecurityToken(config.JwtIssuer, config.JwtAudience, claims, System.Nullable(), expiration, credentials)

  let handler = JwtSecurityTokenHandler()
  let token = handler.WriteToken securityToken

  { token = token
    token_type = "Bearer"
    expiration = securityToken.ValidTo }


let keyLength = 32


let private hash (iterations: int) (salt: byte[]) (password: string) =
  let hash =
    use pbkdf2 =
      new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256)

    pbkdf2.GetBytes(keyLength)

  Array.concat [ salt; hash ] |> Convert.ToBase64String

let hashPassword (iterations: int) =
  let salt = RandomNumberGenerator.GetBytes(16)
  hash iterations salt


let validateUserPassword (iterations:int) =
  let validate (password: string) (user: User) =
    let existing =
      match user.password with
      | None -> "missing-password"
      | Some value -> value

    let hashedBytes = Convert.FromBase64String existing
    let salt = hashedBytes.[..15]
    let hashedPassword = password |> hash iterations salt
    hashedPassword = existing

  validate

let private key = "5wEvdriKT7Q1"

let hashInt (value: int) =
  Crypto.sha256 4 4 key <| value.ToString()

let validateAdmin (user: User) (password: string) =
  user.name = "admin" && user.email = "admin" && password = "admin"

module Cypher =
  let nk = "xK0cQ4Tb2Xp3uV1L9f6H"
  let seed = 238

  let enc (value: int) =
    let str = (value + seed).ToString()

    let getValue total chunk =
      let cutNumber = str[total .. total + chunk - 1] |> int //part of the int number

      if cutNumber < nk.Length then
        (chunk, [ cutNumber ])
      else
        let items =
          cutNumber.ToString()
          |> Seq.toList
          |> Seq.map (fun i -> i.ToString() |> int)
          |> Seq.toList

        let super =
          if items |> List.exists (fun i -> i = 0) then
            items
          else
            [ items[0] ]

        (super.Length, super)


    let rec encode index result =
      if index >= str.Length then
        result
      else
        let size = if str.Length >= 2 then 2 else 1
        let chunk, items = getValue index size

        let results =
          items
          |> List.map (fun i -> nk[i].ToString())
          |> List.fold (fun state i -> state + i) ""

        encode (index + chunk) (result + results)

    encode 0 ""

  let dec (value: string) =
    if value = "" then
      -1
    else
      let rec decode index (result: string) =
        if index >= value.Length then
          result
        else
          let i = nk |> Seq.toList |> List.findIndex (fun c -> c = value[index])
          decode (index + 1) (result + i.ToString())

      let i = decode 0 "" |> int
      i - seed
