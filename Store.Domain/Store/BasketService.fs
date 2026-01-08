module Store.Domain.Store.BasketService

open System
open Store.Domain
open Store.Domain.Store.Catalog
open Store.Domain.Store.Entities

type SaveEffect = Effect<Basket>
type FetchEffect = FetchSingleEffect<string, Basket>
type FetchProduct = FetchSingleEffect<string, Product>

type BasketActionType =
  | AddItem
  | RemoveItem
  | RemoveAll

type BasketInput =
  { productId: string
    quantity: int
    action: BasketActionType

  }

let private addItem (product: Product) (quantity: int) (basket: Basket) =
  let existingItem = basket.items |> List.tryFind (fun i -> i.product.id = product.id)

  let updatedItems =
    match existingItem with
    | Some item ->
      basket.items
      |> List.map (fun i ->
        if i.product.id = product.id then
          { i with
              quantity = i.quantity + quantity }
        else
          i)
    | None ->
      { product = product
        quantity = quantity }
      :: basket.items

  { basket with items = updatedItems } |> succeed

let private removeItem (product: Product) (quantity: int) (basket: Basket) =
  let existingItem = basket.items |> List.tryFind (fun i -> i.product.id = product.id)

  let updatedItems =
    match existingItem with
    | Some item when item.quantity > quantity ->
      basket.items
      |> List.map (fun i ->
        if i.product.id = product.id then
          { i with
              quantity = i.quantity - quantity }
        else
          i)
    | Some _ -> basket.items |> List.filter (fun i -> i.product.id <> product.id)
    | None -> basket.items

  { basket with items = updatedItems } |> succeed

let private newBasket (userId: Id) =
  { id = userId
    items = []
    occurred = DateTime.UtcNow }

let private getOrCreateBasket (userId: Id) (basketResult: ServiceOutput<Basket>) =
  match basketResult with
  | Success basket -> basket
  | Failure _ -> newBasket userId


let createOrUpdate
  (us: UserSession)
  (saveEffect: SaveEffect)
  (fetchBasket: FetchEffect)
  (fetchProduct: FetchProduct)
  (input: BasketInput)
  =

  let newBasket product =
    { id = us.userId
      items = []
      occurred = DateTime.UtcNow }

  let updateBasket (product: Product) (basket: Basket) =
    if input.quantity > 0 then //ADD ITEM
      let existingItem = basket.items |> List.tryFind (fun i -> i.product.id = product.id)

      let updatedItems =
        match existingItem with
        | Some item ->
          basket.items
          |> List.map (fun i ->
            if i.product.id = product.id then
              { i with
                  quantity = i.quantity + input.quantity }
            else
              i)
        | None ->
          { product = product
            quantity = input.quantity }
          :: basket.items

      { basket with items = updatedItems } |> succeed

    elif input.quantity < 0 || input.action.IsRemoveAll then //REMOVE ITEM
      let existingItem = basket.items |> List.tryFind (fun i -> i.product.id = product.id)

      let updatedItems =
        match existingItem with
        | Some item when item.quantity > input.quantity && input.action.IsRemoveAll |> not ->
          basket.items
          |> List.map (fun i ->
            if i.product.id = product.id then
              { i with
                  quantity = i.quantity - input.quantity }
            else
              i)
        | Some _ -> basket.items |> List.filter (fun i -> i.product.id <> product.id)
        | None -> basket.items

      { basket with items = updatedItems } |> succeed

    else
      { basket with items = [] } |> succeed

  let work () =
    async {
      let! product = fetchProduct input.productId
      let! basket = fetchBasket us.userId

      return!
        product
        ?> (fun p ->
          match basket with
          | Success basket -> updateBasket p basket
          | Failure _ -> p |> newBasket |> updateBasket p)
        ?>< saveEffect

    }

  work()
