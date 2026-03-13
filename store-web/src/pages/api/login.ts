import type { APIRoute } from "astro";
import { API_URL } from 'astro:env/server';

export const POST: APIRoute = async ({ cookies, redirect, request }) => {
  try {
    // Parse the incoming JSON request body
    const form = await request.formData();
    const email = form.get("email");
    const password = form.get("password");
    const rememberMe = form.get("remember-me");

    const body = {
      email,
      password,
      rememberMe: rememberMe === "on"
    };

    // Forward the request to the backend
    const backendUrl = `${API_URL}/login`;
    const response = await fetch(backendUrl, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    });

    // Check if the backend response is successful
    if (!response.ok) {
      var message =
        (await response.text()) ?? "Não foi possível completar sua requisição";

      return new Response(
        `
          <div id="message">
          <div role="alert" class="mt-4 border-s-4 border-orange-700 bg-orange-50 p-4">
            <div class="flex items-center gap-2 text-orange-700">
              <strong class="block leading-tight font-medium text-orange-800"> Atenção </strong>
            </div>
            <p class="mt-1 text-sm">
              ${message}
            </p>
          </div>
          </div>
      `,
        {
          status: 422,
          headers: { "Content-Type": "text/html" },
        },
      );
    }

    // Get the backend response data
    const data = await response.json();

    let role = ''
    let isAdmin = false;

    if(response.headers.has('x-us-role')){
      role = response.headers.get('x-us-role')
      isAdmin = role == "AdminRole";
    }

    let location = "/";
    if(isAdmin){
      location = "/management";
    }

    // Store token, token_type, and expiration in cookies
    cookies.set("auth_token", data.token, {
      expires: new Date(data.expiration), // Set cookie expiration to match token
      secure: true, // Use secure cookies in production (HTTPS)
      httpOnly: true,
      sameSite: "strict", // Prevent CSRF
      path: "/",
    });

    // cookies.set("status", "Success!!!", {
    //   httpOnly: true,
    //   sameSite: "strict",
    //   path: "/",
    // });

    return new Response(null, {
      status: 302,
      headers: {
        Location: location,
        // 'Set-Cookie': { token: data.token }
      },
    });
  } catch (error) {
    // Handle errors (e.g., invalid JSON or network issues)
    return new Response(JSON.stringify({ error: "Internal server error" }), {
      status: 500,
      headers: { "Content-Type": "application/json" },
    });
  }
};
