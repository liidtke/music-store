<script>

export let data;

const formatter = new Intl.NumberFormat("pt-BR", {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

</script>

<div class="bg-white md:p-8 p-4 mt-4">
  <div class="md:mx-auto lg:max-w-7xl md:max-w-4xl sm:max-w-xl max-sm:max-w-md">
    <h2 class="text-2xl sm:text-3xl font-semibold mb-6 sm:mb-8">Produtos</h2>

    <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 sm:gap-1 gap-6">

      {#each data as item }
      <div id="item" class="bg-white shadow-sm border border-gray-200 rounded-lg p-3 flex flex-col justify-between">
        <button
          hx-get={"/partials/public/product?id="+ item.id}
          hx-target="#details"
          class="block"
        >
          <!-- <div class="aspect-[12/11] bg-gray-100 rounded-md pt-4 pb-4 ">
            <img src={item.imageUrl ?? item.productType.imageUrl} alt="Product 1"
              class="rounded-md w-full md:h-full max-h-sm object-contain" />

          </div> -->

          <article class="relative isolate flex flex-col justify-end overflow-hidden rounded-md px-8 pb-8 pt-30 md:mx-auto mt-2">
              <img src="{item.imageUrl ?? item.productType.imageUrl}"  class="absolute inset-0 h-full w-full object-cover">
              <div class="absolute inset-0 bg-gradient-to-t from-gray-900 via-gray-900/40"></div>
              <h3 class="z-10 mt-3 text-3xl font-bold text-white mr-auto">{item.name}</h3>
              <div class="z-10 gap-y-1 overflow-hidden text-left text-md leading-6 text-gray-300 mr-auto">{item.description}</div>
          </article>
        </button>

          <div class="text-slate-600 text-md mr-100% font-semibold">
          <p class="text-slate-600 text-[13px] mt-2">por</p>
            {item.professionalName}</div>
          <div class="flex gap-2 mt-4">
            <h5 class="text-base font-semibold text-slate-900">Valor</h5>
            <h6 class="text-base text-slate-900 font-bold ml-auto">R$ {formatter.format(item.unitPrice)}</h6>
          </div>
        <div class="flex items-center gap-2 mt-6 align-bottom">
            <button
              hx-get={"/partials/public/product?id="+ item.id}
              hx-target="#details"
              type="button" class="btn-secondary w-full">Mais Detalhes</button>
            <button type="button" class="btn-primary w-full">Adicionar ao Carrinho</button>
        </div>
      </div>
      {/each}

    </div>


  </div>
</div>

<div id="details"></div>
