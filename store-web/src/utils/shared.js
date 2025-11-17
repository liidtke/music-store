
export function getMessage(msg){
  return  `
<div role="alert" class="alert alert-warning mt-4" _="on click hide me">
  <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 shrink-0 stroke-current" fill="none" viewBox="0 0 24 24">
    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
  </svg>
<span>${msg}</span>
</div>`
};


export function successMessage(url){
    return `
<div role="alert" class="mt-4 alert alert-success mt-p" hx-get="${url}" hx-target="body" hx-trigger="load delay:2s"   >
  <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 shrink-0 stroke-current" fill="none" viewBox="0 0 24 24">
    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
  <span>Salvo com Sucesso</span>
</div>`
}


export function formatDate(input) {
  if(!input){return ''};
    let date = new Date(input);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    
    return `${day}/${month}/${year}`;
}


export const roles = [
  {id:"CustomRole", name: "Acesso Personalizado"},
  {id:"AdminRole", name: "Admin"},
  {id:"OwnerRole", name: "Gestor da Conta"},
  {id:"BasicRole", name: "Usuário Básico"},
  {id:"Anon", name: "Não Registrado"},
];

export function trRole(r, customRole){
  if (r == "CustomRole" && customRole){
    return customRole.name;
  }

  let role = roles.find(x=> x.id == r);
  if(role){return role.name}
  else return r
}

export const subscriptions = [
  {id:"Free", name: "Nenhum"},
  {id:"Basic", name: "Básico"},
  {id:"Advanced", name: "Avançado"},
];

export function trSubscription(r){
  let sub = subscriptions.find(x=> x.id == r);
  if (sub) {
    return sub.name
  }
  else return "";
}
