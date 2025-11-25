<script lang="ts">
  import { onMount, onDestroy } from 'svelte';

  // Props
  export let raw: string = '';        // Initial JSON string from Editor.js
  export let name: string = 'content'; // Name for the hidden input

  let editorElement: HTMLDivElement;
  let editor: any;
  let jsonOutput = raw || '';

  // Initialize Editor.js
  onMount(async () => {
    const { default: EditorJS } = await import('@editorjs/editorjs');
    const { default: Header } = await import('@editorjs/header');
    const { default: List } = await import('@editorjs/list');
    const { default: Quote } = await import('@editorjs/quote');
    const { default: Table } = await import('@editorjs/table');
    editor = new EditorJS({
      holder: editorElement,
      tools: {
        header: Header,
        list: List,
        quote: Quote,
        table: Table,
      },
      data: raw ? JSON.parse(raw) : undefined,
      placeholder: 'Start writing...',
      onChange: async () => {
        try {
          const output = await editor.save();
          jsonOutput = JSON.stringify(output);
        } catch (e) {
          console.error('Error saving editor data:', e);
        }
      },
    });
  });

  onDestroy(() => {
    if (editor && typeof editor.destroy === 'function') {
      editor.destroy();
    }
  });
</script>

<div class="editor-container">
  <div bind:this={editorElement} class="editorjs"></div>

  <!-- Hidden input that holds the JSON output -->
  <input type="hidden" {name} value={jsonOutput} />
</div>

<style>

</style>
