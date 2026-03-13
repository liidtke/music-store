// src/pages/api/basket/add.ts
// Astro API Route (server-side endpoint)
const apiKey = import.meta.env.API_URL;
import toaster from '@utils/toaster'

import type { APIRoute } from 'astro';

export const POST: APIRoute = async ({ url, request, redirect, cookies }) => {
  const form = await request.formData();
  const productId = form.get("productId");
  const quantity = form.get("quantity") ?? 1;
  const action = form.get("action") ?? 'AddItem';

  // const productId = url.searchParams.get('productId');

  if (!productId) {
    return new Response(
      JSON.stringify({ error: 'productId is required' }),
      { status: 400, headers: { 'Content-Type': 'application/json' } }
    );
  }

  const body = {
    productId,
    quantity,
    action
  };

  try {
    let part = "my-basket";
    const url = `${apiKey}/${part}`;
    const token = cookies.get('auth_token')?.value;
    const guestId = cookies.get('guest_id')?.value;

    const response = await fetch(url, {
      method: 'PUT',
      body: JSON.stringify(body),
      headers: {
        'Authorization': token ? `Bearer ${token}` : '',
        'Content-Type': 'application/json',
        'X-Requested-With': 'Astro-SSR',
        'X-Guest-Id': guestId
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Backend error:', response.status, errorText);


      if (response.status == 401) {

        let indicator = `
          <span _="on load go to url /login">
          Adicionar ao Carrinho
          </span>
          `;
        return new Response(indicator, {
          status: 200,
          headers: { "Content-Type": "text/html" }
        });

      }
        return new Response(toaster.error("Erro ao adicionar item:" + errorText), {
          status: 203,
          headers: { "Content-Type": "text/html" }
        })
    }

    const data = await response.json();

    let indicator = `
      <span _="on load wait 3s then put 'Adicionar Produto' into me">
      Produto Adicionado
      </span>
        <span id="basket-indicator" hx-swap-oob="true" class="rounded-full bg-blue-100 px-2.5 py-0.5 text-sm whitespace-nowrap text-blue-700">
          ${data?.quantity}
        </span>
      `;
    return new Response(indicator, {
      status: 200,
      headers: { "Content-Type": "text/html" }
    });

  } catch (err) {
    console.error('Fetch error:', err);
    return new Response(
      JSON.stringify({ error: 'Internal server error' }),
      { status: 500, headers: { 'Content-Type': 'application/json' } }
    );
  }
};
