<script lang="ts">
  import { onMount, onDestroy } from 'svelte';

  // Import Quill styles
  import 'quill/dist/quill.core.css';
  import 'quill/dist/quill.snow.css';

  // Props
  export let value: string = ''; // Initial raw HTML or Delta (as stringified JSON)
  export let name: string = 'content'; // Name for the hidden input
  export let format: 'html' | 'delta' = 'html'; // What to store in hidden input

  let container: HTMLDivElement;
  let quill: any;
  let hiddenInput: HTMLInputElement;

  // Dynamically import Quill only in browser
  onMount(async () => {
    const Quill = (await import('quill')).default;

    quill = new Quill(container, {
      theme: 'snow',
      modules: {
        toolbar: [
          [{ header: [1, 2, 3, false] }],
          ['bold', 'italic', 'underline', 'strike'],
          [{ list: 'ordered' }, { list: 'bullet' }],
          [{ color: [] }, { background: [] }],
          ['link', 'image', 'video'],
          ['clean']
        ]
      },
      placeholder: 'Write something...',
    });

    // If initial value is provided
    if (value) {
      if (format === 'html') {
        quill.root.innerHTML = value;
      } else if (format === 'delta') {
        try {
          const delta = JSON.parse(value);
          quill.setContents(delta);
        } catch (e) {
          console.warn('Invalid Delta JSON');
        }
      }
    }

    // Update hidden input on text change
    quill.on('text-change', updateHiddenInput);

    // Initial update
    updateHiddenInput();
  });

  function updateHiddenInput() {
    if (!quill || !hiddenInput) return;

    let content;
    if (format === 'html') {
      content = quill.root.innerHTML;
    } else {
      content = JSON.stringify(quill.getContents());
    }

    hiddenInput.value = content;
    // Dispatch input event so frameworks (like Astro forms) detect change
    hiddenInput.dispatchEvent(new Event('input', { bubbles: true }));
  }

  onDestroy(() => {
    if (quill) {
      quill.off('text-change', updateHiddenInput);
    }
  });
</script>

<div class="quill-wrapper">
  <div bind:this={container}></div>
</div>
<input id="profile" type="hidden" {name} bind:this={hiddenInput} />

<style>
  .quill-wrapper {
    margin: 1rem 0;
    border: 1px solid #ccc;
    border-radius: 8px;
    overflow: hidden;
  }

  :global(.ql-editor) {
    min-height: 200px;
    font-size: 16px;
  }

  :global(.ql-toolbar) {
    border-top: none;
    border-left: none;
    border-right: none;
  }

  :global(.ql-container) {
    border-bottom: none;
    border-left: none;
    border-right: none;
  }
</style>
