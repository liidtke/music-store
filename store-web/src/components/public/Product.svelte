<script>
  import dateConverter from "@utils/date-converter.ts";

  const formatter = new Intl.NumberFormat("pt-BR", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  });

  export let data;

  // let dueDate = dateConverter.dayjs().add(data.minDeadLine, 'day')

</script>

<div class="p-4 bg-gray-100">
  <div class="lg:max-w-6xl max-w-xl mx-auto">
    <div class="grid items-start grid-cols-1 lg:grid-cols-2 gap-8 max-lg:gap-12 max-sm:gap-8">
      <div class="w-full lg:sticky top-0">
        <div class="flex flex-col gap-4">
          <div class="bg-white shadow-sm p-2">
            <img src={data.imageUrl ?? data.productType.imageUrl} alt="Product"
              class="w-full aspect-[11/8] object-cover object-top" />
          </div>
        </div>
      </div>
      <div class="w-full">
        <div>
          <h2 class="text-base mt-4 text-slate-500 font-semibold">{data.productType.name}</h2>
          <h3 class="text-lg sm:text-xl font-semibold text-slate-900">{data.name}</h3>
          <div class="mt-4">
            <p class="text-slate-500 mt-1 text-sm">{data.description}</p>

            <h4 class="text-base mt-4 text-slate-500 font-semibold">por</h4>
            <p class="text-lg text-slate-500">{data.professionalName}</p>
            <a class="table-action" href={"/professionals/"+data.professionalId}>Mais Detalhes</a>
          </div>

          <div class="flex items-center flex-wrap gap-2 mt-6">
            {#if data.unitPrice != data.oldUnitPrice}
              <p class="text-slate-400 text-base"><strike>R$ {data.oldUnitPrice}</strike></p>
              {/if}
            <h4 class="text-hs text-2xl sm:text-3xl ml-auto font-semibold">R$ {formatter.format(data.unitPrice)} </h4>
          </div>

          <div>
            <h4 class="text-base mt-4 text-slate-500 font-semibold">Prazo</h4>
            <p class="text-sm text-slate-500">{data.minDeadLine} dias úteis</p>
          </div>
        </div>

        <hr class="my-6 border-gray-300" />

        <div>
          <div class="flex gap-4 items-center border border-gray-300 bg-white px-4 py-2.5 w-max">
            <button type="button" class="border-0 outline-0 cursor-pointer">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-2.5 h-2.5" viewBox="0 0 121.805 121.804">
                <path
                  d="M7.308 68.211h107.188a7.309 7.309 0 0 0 7.309-7.31 7.308 7.308 0 0 0-7.309-7.309H7.308a7.31 7.31 0 0 0 0 14.619z"
                  data-original="#000000" />
              </svg>
            </button>
            <span clsemiass="text-slate-900 text-sm font-semibold px-6 block">1</span>
            <button type="button" class="border-0 outline-0 cursor-pointer">
              <svg xmlns="http://www.w3.org/2000/svg" class="w-2.5 h-2.5" viewBox="0 0 512 512">
                <path
                  d="M256 509.892c-19.058 0-34.5-15.442-34.5-34.5V36.608c0-19.058 15.442-34.5 34.5-34.5s34.5 15.442 34.5 34.5v438.784c0 19.058-15.442 34.5-34.5 34.5z"
                  data-original="#000000" />
                <path
                  d="M475.392 290.5H36.608c-19.058 0-34.5-15.442-34.5-34.5s15.442-34.5 34.5-34.5h438.784c19.058 0 34.5 15.442 34.5 34.5s-15.442 34.5-34.5 34.5z"
                  data-original="#000000" />
              </svg>
            </button>
          </div>

          <div class="mt-4 flex flex-wrap gap-4">
            <button type="button"
              class="btn-ghost">Adicionar ao Carrinho</button>
            <button type="button" class="btn-primary">Comprar Agora</button>
          </div>
        </div>

        <hr class="my-6 border-gray-300" />

      </div>
    </div>
  </div>
</div>
