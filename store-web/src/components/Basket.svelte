<script>

export let data = [];


</script>


<div class="max-w-6xl max-lg:max-w-2xl mx-auto bg-white p-4 md:p-8 mt-4">
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

      {#each data as items}

      <div class="flex gap-4 max-sm:flex-col">
        <div class="w-24 h-24 shrink-0 bg-white p-2 rounded-md">
          <img
            src="https://readymadeui.com/images/product14.webp"
            class="w-full h-full object-contain"
          />
        </div>
        <div class="w-full flex justify-between gap-4">
          <div>
            <h3 class="text-[15px] font-medium text-slate-900">
              Velvet Sneaker
            </h3>
            <h6
              class="text-[15px] text-slate-900 font-semibold cursor-pointer mt-4"
            >
              $18.00
            </h6>
          </div>
          <div class="flex flex-col justify-between items-end gap-4">
            <div>
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
            </div>
            <div
              class="flex items-center px-2.5 py-1.5 border border-gray-300 text-slate-900 text-xs rounded-md"
            >
              <span class="cursor-pointer">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  class="w-2.5 fill-current"
                  viewBox="0 0 124 124"
                >
                  <path
                    d="M112 50H12C5.4 50 0 55.4 0 62s5.4 12 12 12h100c6.6 0 12-5.4 12-12s-5.4-12-12-12z"
                    data-original="#000000"></path>
                </svg>
              </span>

              <span class="mx-3">2</span>
              <span class="cursor-pointer">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  class="w-2.5 fill-current"
                  viewBox="0 0 42 42"
                >
                  <path
                    d="M37.059 16H26V4.941C26 2.224 23.718 0 21 0s-5 2.224-5 4.941V16H4.941C2.224 16 0 18.282 0 21s2.224 5 4.941 5H16v11.059C16 39.776 18.282 42 21 42s5-2.224 5-4.941V26h11.059C39.776 26 42 23.718 42 21s-2.224-5-4.941-5z"
                    data-original="#000000"></path>
                </svg>
              </span>
            </div>
          </div>
        </div>
      </div>

      <hr class="border-gray-300 mb-4" />
      {/each}
    </div>
  </div>

  <div class="mt-8">
    <div class="bg-gray-100 p-6 rounded-md">
      <div class="flex md:items-center max-md:flex-col gap-x-12 gap-y-6">
        <div class="max-md:flex max-md:justify-between max-md:gap-4">
          <a
            href="/products"
            type="button"
            class="cursor-pointer text-sm text-slate-900 font-medium bg-gray-200 border border-gray-300 outline-0 px-4 py-2 rounded-md max-md:w-full"
          >
            Continuar Comprando
          </a>
        </div>
        <div class="max-md:flex max-md:justify-between max-md:gap-4">
          <h6 class="text-[15px] font-medium text-slate-900">Total a Pagar</h6>
          <p class="text-sm text-slate-500 font-medium md:mt-2">R$ 78.00</p>
        </div>
        <div class="md:ml-auto">
          <button
            type="button"
            class="cursor-pointer text-sm text-slate-900 font-medium bg-gray-200 border border-gray-300 outline-0 px-4 py-2 rounded-md max-md:w-full"
          >
            Finalizar Compra
          </button>
        </div>
      </div>
    </div>
  </div>

</div>
