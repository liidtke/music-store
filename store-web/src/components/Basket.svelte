<script>
import Minus from '@components/basket/Minus.svelte';

export let data = {};
const formatter = new Intl.NumberFormat("pt-BR", {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
  })
</script>

<div id="wrapper" class="max-w-6xl max-lg:max-w-2xl mx-auto bg-white p-4 md:p-8 mt-4">
  <div class="border-b border-gray-300 pb-4">
    <div class="flex gap-4">
      <div>
        <h3 class="text-2xl font-semibold text-slate-900">
          Carrinho de Compras
        </h3>
        <p class="text-[15px] text-slate-500 mt-3">
          Consulte os itens adicionados e prossiga para o pagamento
        </p>
      </div>
    </div>
  </div>

  <div class="bg-gray-100 p-6 rounded-md mt-8">
    <div class="space-y-4">


      {#if !data || data.quantity == 0}
      <div class="max-w-md text-center">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="mx-auto size-20 text-gray-400">
            <path stroke-linecap="round" stroke-linejoin="round" d="M2.25 3h1.386c.51 0 .955.343 1.087.835l.383 1.437M7.5 14.25a3 3 0 0 0-3 3h15.75m-12.75-3h11.218c1.121-2.3 2.1-4.684 2.924-7.138a60.114 60.114 0 0 0-16.536-1.84M7.5 14.25 5.106 5.272M6 20.25a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Zm12.75 0a.75.75 0 1 1-1.5 0 .75.75 0 0 1 1.5 0Z"></path>
          </svg>

        <h2 class="mt-6 text-2xl font-bold text-gray-900">Seu carrinho está vazio</h2>

        <p class="mt-4 text-pretty text-gray-700">
          Adicione novos itens, eles aparecerão aqui
        </p>
      </div>
      {:else}

      {#each data.items as item}

      <div class="flex gap-4 max-sm:flex-col">
        <div class="w-24 h-24 shrink-0 bg-white p-2 rounded-md">
          <img
            src={item.product.imageUrl}
            class="w-full h-full object-cover"
            alt="imagem"
          />
        </div>
        <div class="w-full flex justify-between gap-4">
          <div>
            <h3 class="text-[15px] font-medium text-slate-900">
              {item.product.name}
            </h3>
            <h6
              class="text-[15px] text-slate-900 font-semibold cursor-pointer mt-4"
            >
              R$ {formatter.format(item.product.unitPrice * item.quantity)}
            </h6>
          </div>
          <div class="flex flex-col justify-between items-end gap-4">
            <div>
              <form action="" hx-post='/basket'
              >
              <input type="hidden" name="productId" value={item.product.id}>
              <input type="hidden" name="quantity" value=0>
              <input type="hidden" name="action" value="RemoveAll">

              <button type="submit">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="w-4 fill-red-500 inline cursor-pointer"
                viewBox="0 0 24 24"
              >
                <path
                  d="M19 7a1 1 0 0 0-1 1v11.191A1.92 1.92 0 0 1 15.99 21H8.01A1.92 1.92 0 0 1 6 19.191V8a1 1 0 0 0-2 0v11.191A3.918 3.918 0 0 0 8.01 23h7.98A3.918 3.918 0 0 0 20 19.191V8a1 1 0 0 0-1-1Zm1-3h-4V2a1 1 0 0 0-1-1H9a1 1 0 0 0-1 1v2H4a1 1 0 0 0 0 2h16a1 1 0 0 0 0-2ZM10 4V3h4v1Z"
                  data-original="#000000"></path>
                <path
                  d="M11 17v-7a1 1 0 0 0-2 0v7a1 1 0 0 0 2 0Zm4 0v-7a1 1 0 0 0-2 0v7a1 1 0 0 0 2 0Z"
                  data-original="#000000"></path>
              </svg>
              </button>
              </form>
            </div>
            <div
              class="flex items-center px-2.5 py-1.5 border border-gray-300 text-slate-900 text-xs rounded-md"
            >
             <Minus id={item.product.id}></Minus>

              <span class="mx-3">{item.quantity}</span>

              <Minus type="add" id={item.product.id}></Minus>
            </div>
          </div>
        </div>
      </div>

      <hr class="border-gray-300 mb-4" />
      {/each}
      {/if}
    </div>


    {#if data && data.quantity > 0}
    <div class="max-md:flex max-md:justify-between max-md:gap-4">
    <h6 class="text-[15px] font-medium text-slate-900">Total a Pagar</h6>
    <p class="text-sm text-slate-500 font-medium md:mt-2">R$ {formatter.format(data.price)}</p>
    </div>
    {/if}

  </div>

  <div class="mt-8">
    <div class="bg-zinc-800 p-6 rounded-md">
      <div class="grid md:grid-cols-2 grid-gap-2">
          <a
            href="/products"
            type="button"
            class="btn-ghost"
          >
            Continuar Comprando
          </a>

          {#if data && data.quantity > 0}
          <button
            hx-get="/basket/checkout"
            hx-target="main"
            hx-select="main"
            type="button"
            class="btn-primary"
          >
            Finalizar Compra
          </button>
          {/if}
      </div>
    </div>
  </div>

</div>
