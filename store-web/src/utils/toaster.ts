export class Toaster {

  private baseToaster(msg = 'Salvo com Sucesso') {
    return `
<div hx-swap-oob="beforeend:#toaster-wrapper" >
<div _="on load wait 4s then remove me" hx-ext="class-tools" classes="add done" class="fade-in toaster" role="alert" tabindex="-1" aria-labelledby="hs-toast-normal-example-label">
    <div class="flex p-4">
      <div class="shrink-0">
        <svg class="shrink-0 size-4 text-hs mt-0.5" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
          <path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm.93-9.412-1 4.705c-.07.34.029.533.304.533.194 0 .487-.07.686-.246l-.088.416c-.287.346-.92.598-1.465.598-.703 0-1.002-.422-.808-1.319l.738-3.468c.064-.293.006-.399-.287-.47l-.451-.081.082-.381 2.29-.287zM8 5.5a1 1 0 1 1 0-2 1 1 0 0 1 0 2z"></path>
        </svg>
      </div>
      <div class="ms-3">
        <p id="hs-toast-normal-example-label" class="text-sm text-gray-700 dark:text-neutral-400">
          ${msg}
        </p>
      </div>
    </div>
  </div>
</div>
`;

  }

  successObj(obj:string) {
    return `
${this.baseToaster()}
${obj}

`
  }
  successMessage(msg = "Salvo com Sucesso") {
    return `
 ${this.baseToaster(msg)}
`
  }

  success(url, target = "main", select = "main") {
    return `
${this.baseToaster()}
<div hx-get="${url}" hx-replace-url="true" hx-target="${target}" hx-select="${select}" hx-swap="outerHTML" hx-trigger="load delay:300ms"   >
</div>
`
  }

  successOptions(url, target = "main", select = "main", replace = "false", msg = "Salvo com Sucesso") {
    return `
${this.baseToaster(msg)}
<div hx-get="${url}" hx-replace-url="${replace}" hx-target="${target}" hx-select="${select}" hx-swap="outerHTML" hx-trigger="load delay:300ms"   >
</div>
`
  }


  alert(alert) {
    return `
<div hx-swap-oob="beforeend:#toaster-wrapper" >
<div _="on load wait 4s then remove me" hx-ext="class-tools" classes="add done" class="fade-in max-w-xs bg-white border border-gray-200 rounded-md shadow-lg dark:bg-neutral-800 dark:border-neutral-700" role="alert" tabindex="-1" aria-labelledby="hs-toast-normal-example-label">
     
    <div class="flex p-4">
      <div class="shrink-0">
        <svg class="shrink-0 size-4 text-hs-negative mt-0.5" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
          <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM8 4a.905.905 0 0 0-.9.995l.35 3.507a.552.552 0 0 0 1.1 0l.35-3.507A.905.905 0 0 0 8 4zm.002 6a1 1 0 1 0 0 2 1 1 0 0 0 0-2z"></path>
        </svg>
      </div>
      <div class="ms-3">
        <p id="hs-toast-warning-example-label" class="text-sm text-gray-700 dark:text-neutral-400">
${alert}
        </p>
      </div>
    </div>
  </div>
</div>


    `};

  error(alert) {
    return `
<div hx-swap-oob="beforeend:#toaster-wrapper" >
<div _="on load wait 4s then remove me" hx-ext="class-tools" classes="add done" class="fade-in max-w-xs bg-white border border-gray-200 rounded-md shadow-lg dark:bg-neutral-800 dark:border-neutral-700" role="alert" tabindex="-1" aria-labelledby="hs-toast-normal-example-label">
     <div class="flex p-4">
      <div class="shrink-0">
        <svg class="shrink-0 size-4 text-red mt-0.5" xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
          <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"></path>
        </svg>
      </div>
      <div class="ms-3">
        <p id="hs-toast-error-example-label" class="text-sm text-gray-700 dark:text-neutral-400">
${alert}
        </p>
      </div>
    </div>
  </div>
</div>
    `};


  inputValidation(id, message, tag = "input"){
    let el = `
<p class="prose text-xs mt-1 uppercase text-red-600 mb-0" hx-swap-oob="true" id=${id} _="on load add .error to the previous <${tag}/> "> ${message} </p>
`;
    return new Response(el, { 
      status: 200,
      headers: { "Content-Type": "text/html" }
    });
  }


}

const toaster = new Toaster();
export default toaster;
