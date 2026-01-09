<script>
  // Props in Svelte are accessed via export
  export let id;
  export let type;

  const isAdd = type === "add";
  const action = isAdd ? "AddItem" : "RemoveItem"

  const count = isAdd ? 1 : -1;
</script>

<form action="" method="post" use:enhance hx-post="/basket">
  <input type="hidden" name="productId" value={id} />
  <input type="hidden" name="quantity" value={count} />
  <input type="hidden" name="action" value={action} />

  <button class="cursor-pointer" type="submit">
  {#if !isAdd}
    <svg
      xmlns="http://www.w3.org/2000/svg"
      class="w-2.5 fill-current"
      viewBox="0 0 124 124"
    >
      <path
        d="M112 50H12C5.4 50 0 55.4 0 62s5.4 12 12 12h100c6.6 0 12-5.4 12-12s-5.4-12-12-12z"
        data-original="#000000"
      />
    </svg>
    {:else}
    <svg
                    xmlns="http://www.w3.org/2000/svg"
                    class="w-2.5 fill-current"
                    viewBox="0 0 42 42"
                  >
                    <path
                      d="M37.059 16H26V4.941C26 2.224 23.718 0 21 0s-5 2.224-5 4.941V16H4.941C2.224 16 0 18.282 0 21s2.224 5 4.941 5H16v11.059C16 39.776 18.282 42 21 42s5-2.224 5-4.941V26h11.059C39.776 26 42 23.718 42 21s-2.224-5-4.941-5z"
                      data-original="#000000"></path>
                  </svg>
    {/if}
  </button>
</form>
