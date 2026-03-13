// src/pages/api/samples/[id]/download.ts
import type { APIRoute } from 'astro';
const apiUrl = import.meta.env.API_URL;

export const GET: APIRoute = async ({ params, request }) => {
  const { id } = params;

  if (!id) {
    return new Response('Sample ID is required', { status: 400 });
  }

  try {
    // Call your existing F# Falco endpoint that serves the raw file
    const fileResponse = await fetch(`${apiUrl}/samples/${id}`);

    if (!fileResponse.ok) {
      return new Response('Sample not found', { status: 404 });
    }

    const arrayBuffer = await fileResponse.arrayBuffer();
    const contentType = fileResponse.headers.get('content-type') || 'application/octet-stream';
    const contentDisposition = fileResponse.headers.get('content-disposition');

    console.log('len', arrayBuffer.byteLength);

    // Try to extract filename from header, fallback to generic
    let fileName = `sample-${id}`;
    if (contentDisposition) {
      const match = contentDisposition.match(/filename="?([^"]+)"?/);
      if (match?.[1]) fileName = match[1];
    }

    return new Response(arrayBuffer, {
      status: 200,
      headers: {
        'Content-Type': contentType,
        'Content-Disposition': `attachment; filename="${fileName}"`,
        'Cache-Control': 'no-cache',
      },
    });
  } catch (error) {
    console.error('Download failed:', error);
    return new Response('Download failed', { status: 500 });
  }
};
