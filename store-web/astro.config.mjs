// @ts-check
import { defineConfig, envField } from 'astro/config';
import UnoCSS from 'unocss/astro'
import svelte from '@astrojs/svelte';

import { memoryCache } from 'astro/config';
import node from '@astrojs/node';

import cloudflare from '@astrojs/cloudflare';

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
  },
  experimental: {
    cache: { provider: memoryCache() },
  },
  adapter: cloudflare()
});