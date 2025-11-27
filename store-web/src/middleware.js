import { Result } from "./utils/result";
const apiKey = import.meta.env.API_URL;

export async function onRequest(context, next) {
  const { request, locals } = context;

  // Configurar interceptador de fetch
  locals.fetchWithAuth = async (part, options = {}) => {
    const url = `${apiKey}/${part}`;
    const token = context.cookies.get('auth_token')?.value;

    return fetch(url, {
      ...options,
      headers: {
        'Authorization': token ? `Bearer ${token}` : '',
        'Content-Type': 'application/json',
        'X-Requested-With': 'Astro-SSR',
        ...options.headers
      }
    });

  };


  locals.fetchWithResult = async (part, options = {}) => {
    const url = `${apiKey}/${part}`;
    const token = context.cookies.get('auth_token')?.value;

    try {
      var response = await fetch(url, {
        ...options,
        headers: {
          'Authorization': token ? `Bearer ${token}` : '',
          'Content-Type': 'application/json',
          'X-Requested-With': 'Astro-SSR',
          ...options.headers
        }
      });

      return await Result.ParseResponse(response)

    }
    catch (e) {
      return Result.ParseError(e);

    }

  };

  locals.fetchWithResultCrude = async (part, options = {}) => {
    const url = `${apiKey}/${part}`;
    const token = context.cookies.get('auth_token')?.value;

    try {
      var response = await fetch(url, {
        ...options,
        headers: {
          'Authorization': token ? `Bearer ${token}` : '',
          'X-Requested-With': 'Astro-SSR',
          ...options.headers
        }
      });

      return await Result.ParseResponse(response)

    }
    catch (e) {
      return Result.ParseError(e);

    }

  };

  return next();
}
