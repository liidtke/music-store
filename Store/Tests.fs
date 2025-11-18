module Store.Tests

open System
open Store.Domain
open Store.Domain.Health
open Xunit.Abstractions
open Xunit
open Store.Security

type SecurityShould(output: ITestOutputHelper) =
  let write result = output.WriteLine $"Result: %s{result}"

  [<Fact>]
  let encryptInt () =
    let value = Security.hashInt 12
    let expected = "AstVFA=="
    write value
    Assert.Equal(expected, value)

  (*[<Fact>]
  let encryptPassword () =
    let config: Security.SecuritySettings =
      { Iterations = 1344
        Salt = "SOQbltfCDhyyviTAba321" }

    let pwd = "admin"
    let value = Security.hashPassword config pwd
    let expected = "gWbWX/SUSPY1OUq888TQbk/2zUU8blO0CHl2LIIgEjc="
    write value
    Assert.Equal(expected, value)*)

  [<Fact>]
  let encodeInt () =
    let number = 611
    let expected = "T3"
    let value = Security.Cypher.enc number
    write value
    Assert.Equal(expected, value)


  [<Fact>]
  let encodeInt2 () =
    let number = 7
    let expected = "0Q4"
    let value = Security.Cypher.enc number
    write value
    Assert.Equal(expected, value)

  [<Fact>]
  let encodeIntLange () =
    let number = 2019804
    let expected = "0xH2xQ"
    let value = Security.Cypher.enc number
    write value
    Assert.Equal(expected, value)

  [<Fact>]
  let decodeInt () =
    let input = "0xH2xQ"
    let expected = 2019804
    let value = Security.Cypher.dec input
    write <| value.ToString()
    Assert.Equal(expected, value)


  [<Fact>]
  let decodeIntSeven () =
    let input = "0Q4"
    let expected = 7
    let value = Security.Cypher.dec input
    write <| value.ToString()
    Assert.Equal(expected, value)


  [<Fact>]
  let createKey () =
    let email = "aldo.liidtke@gmail.com"
    let expected = "123"

    let user: User =
      { id = ""
        name = "John Doe"
        email = "john@example.com"
        password = Some "secure123"
        role = Role.AdminRole // Assuming Role is an existing type with a User variant
        isDeleted = false
        isVerified = true }

    let getuser s = async { return user |> succeed }
    let saveEffect u = async { return u |> succeed }
    let salt = Security.Crypto.createSalt
    let genCode = Security.Crypto.sha256 2 32

    let value =
      UserVerificationService.requestPasswordReset email getuser saveEffect salt genCode

    let result = value |> Async.RunSynchronously

    match result with
    | Success userVerification ->
      write userVerification.hash.Value |> ignore
      write userVerification.verificationCode |> ignore
      Assert.NotNull(value)

  [<Fact>]
  let hashPassword () =
    let password = "123456"
    let h1 = Security.hashPassword 32 password
    let h2 = Security.hashPassword 32 password
    Assert.True(h1 <> h2)

  [<Fact>]
  let comparePasswords () =
    let password = "123456"
    let hashed = "EGBqsUsEzuAfbcLvuBzH/3L5UTal1ctStf1uHhaYQyEBYmqrdBB2yzN0iISvFi3F"

    let result =
      Security.validateUserPassword
        32
        password
        { password = Some <| hashed
          id = ""
          name = ""
          email = ""
          role = Role.AdminRole
          isDeleted = false
          isVerified = true }

    Assert.True(result)

// hash   Result: FUvRJOCLyYdXP/IvRAnCmJNV9O2WCqN4IFcL14KedCM=
// salt   Result: T020/QjxUVmXG4nTsg/8gw==

// [<Fact>]
// let parseMoneyShould () =
//   let original = "R$ 233,01"
//   let expected = 233.01m
//   let result = Money.parse original
//   Assert.Equal(result, expected)
