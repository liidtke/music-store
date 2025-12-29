// src/pages/api/basket/add.ts
// Astro API Route (server-side endpoint)
const apiKey = import.meta.env.API_URL;

import type { APIRoute } from 'astro';

export const POST: APIRoute = async ({ url, request, redirect, cookies }) => {
  const productId = url.searchParams.get('productId');

  if (!productId) {
    return new Response(
      JSON.stringify({ error: 'productId is required' }),
      { status: 400, headers: { 'Content-Type': 'application/json' } }
    );
  }

  // Default values
  const quantity = 1;
  const action: 'AddItem' | 'RemoveItem' | 'RemoveAll' = 'AddItem'; // Adjust if you support other actions later

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
      return new Response(
        JSON.stringify({ error: 'Failed to update basket', details: errorText }),
        { status: response.status, headers: { 'Content-Type': 'application/json' } }
      );
    }

    const data = await response.json();

    // Optional: redirect back or return JSON
    // If you want to use this with HTMX and swap content, return JSON or HTML
    return new Response(JSON.stringify({ success: true, data }), {
      status: 200,
      headers: { 'Content-Type': 'application/json' }
    });

  } catch (err) {
    console.error('Fetch error:', err);
    return new Response(
      JSON.stringify({ error: 'Internal server error' }),
      { status: 500, headers: { 'Content-Type': 'application/json' } }
    );
  }
};
