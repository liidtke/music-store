// @ts-check
import { defineConfig, envField } from 'astro/config';
import UnoCSS from 'unocss/astro'
import svelte from '@astrojs/svelte';

// https://astro.build/config
export default defineConfig({
  output: 'server',
  devToolbar: {
    enabled: false
  },
  vite: {},
  integrations: [svelte(), 
    UnoCSS(
    {
      injectReset: true // or a path to the reset file
    }
  )

  ],
  env: {
    schema: {
      API_URL: envField.string({ context: "client", access: "public", optional: true }),
      // ...
    }
  }
});
