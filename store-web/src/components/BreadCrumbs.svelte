<script>
let { path, name } = $props();

let paths = path.split(',')
let names = name.split(',')

console.log(names);
</script>

<nav aria-label="Breadcrumb">
  <ol class="flex items-center gap-1 text-sm text-gray-700">
    <li>
      <a href="#" class="block transition-colors hover:text-gray-900"> Início </a>
    </li>


{#each paths as path, index}

    <li class="rtl:rotate-180">
      <svg xmlns="http://www.w3.org/2000/svg" class="size-4" viewBox="0 0 20 20" fill="currentColor">
        <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd"></path>
      </svg>
    </li>

    <li>
      <a href={"/app/" + path} class="block transition-colors hover:text-gray-900"> {names[index]} </a>
    </li>

{/each}

  </ol>
</nav>
