<script>

export let data;

</script>

<div class="bg-white md:p-8 p-4">
  <div class="mx-auto lg:max-w-7xl md:max-w-4xl sm:max-w-xl max-sm:max-w-sm">
    <h2 class="text-2xl sm:text-3xl font-semibold mb-6 sm:mb-8">Produtos</h2>

    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 max-xl:gap-4 gap-6">

      {#each data as item }
      <div class="bg-white shadow-sm border border-gray-200 rounded-lg p-3">
        <button
          hx-get={"/partials/public/product?id="+ item.id}
          hx-target="#details"
          class="block"
        >
          <div class="aspect-[12/11] bg-gray-100 rounded-lg p-4">
            <img src="/image2.jpg" alt="Product 1"
              class="w-full h-full object-contain" />
          </div>

          <div class="flex gap-2 mt-4">
            <h5 class="text-base font-semibold text-slate-900">{item.name}</h5>
            <h6 class="text-base text-slate-900 font-bold ml-auto">R$ {item.unitPrice}</h6>
          </div>
          <p class="text-slate-600 text-[13px] mt-2">{item.description}</p>
        </button>
        <form action="">
        <div class="flex items-center gap-2 mt-6">
            <button type="button" class="btn-primary w-full">Adicionar ao Carrinho</button>
        </div>
        </form>
      </div>
      {/each}

    </div>
  </div>
</div>

<div id="details"></div>
