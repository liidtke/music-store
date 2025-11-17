window.data = {};

window.onload = function () {
    console.log("loaded!");
    window.loadTheme();
    document.body.addEventListener('htmx:beforeSwap', function (evt) {
        let status = evt.detail.xhr.status;
        if (status == 404) {
            // alert the user when a 404 occurs (maybe use a nicer mechanism than alert())
        } else if (status == 500 || status == 409) {
            evt.detail.shouldSwap = true;
            // set isError to false to avoid error logging in console
            evt.detail.isError = false;
            evt.detail.target = htmx.find("#message");
            evt.detail.swapOverride = "afterbegin"
        } else if (status == 201) {
            evt.detail.target = htmx.find("#message");
        } else if (status == 400 || status == 404) {
            evt.detail.shouldSwap = true;
            evt.detail.isError = false;

        }
    });

    document.body.addEventListener('htmx:beforeRequest', function (evt) {
        let loader = document.getElementById('loader-wrapper');
        if (loader) {
            loader.hidden = false;
        }
    });

    document.body.addEventListener('htmx:configRequest', function (evt) {
        evt.detail.headers["X-Theme"] = document.theme;
    });

    document.body.addEventListener('htmx:afterRequest', function (evt) {
        let loader = document.getElementById('loader-wrapper');
        if (loader) {
            loader.hidden = true;
        }
        higlightRoute(evt);
    });

}

function higlightRoute(evt) {
    if (evt.detail.target.id != "page-main") {
        console.log('returning');
        return;
    }

    let path = window.location.pathname;
    let all = document.querySelectorAll(`.pf-v6-c-nav__link`);
    for (const item of all) {
        if (item.classList.contains('pf-m-current')) {
            item.classList.remove('pf-m-current');
        }

    }
    let element = document.querySelector(`.pf-v6-c-nav__link[hx-get='${path}']`)
    if (element) {
        element.classList.add('pf-m-current');
    }
}

function Hello() {
    const {a, div, li, p, ul} = van.tags
    div(
        p("👋Hello"),
        ul(
            li("🗺️World"),
            li(a({href: "https://vanjs.org/"}, "🍦VanJS")),
        ),
    )
}

function setOrgId(id) {
    window.data.orgId = id;
}

function test() {
    van.add(document.body, Hello());
}

function toggleAside() {
    console.debug('aside');
    let element = document.getElementById("aside-panel");
    element.classList.toggle("is-open");
    element.classList.toggle("is-collapsed");
}

function createAuthClient(config) {
    return auth0.createAuth0Client({
        // useRefreshTokens: true,
        // cacheLocation: 'localstorage',
        domain: config.domain,
        clientId: config.clientId,
        authorizationParams: {
            redirect_uri: window.location.origin + "/callback",
            scope: "offline_access",
            audience: "Store.
        }
    });

}

window.login = async () => {
    console.log("login");
    let auth0Client = null;
    const fetchAuthConfig = () => fetch("/static/auth_config.json");
    const configureClient = async () => {
        const response = await fetchAuthConfig();
        const config = await response.json();

        auth0Client = await createAuthClient(config);
    };

    await configureClient();
    const login = async () => {
        await auth0Client.loginWithRedirect({
            authorizationParams: {
                redirect_uri: window.location.origin + "/callback",
                scope: "openid profile email offline_access",
                audience: "Store.
            }
        });
    };

    await login();
}


function loginPostComponent(token) {
    const {a, div, li, p, ul} = van.tags
    let header = 'Bearer ' + token;
    return div(
        {
            "hx-get": "/callback-confirm",
            "hx-headers": `{"Authorization": "${header}"}`,
            "hx-push-url": "true",
            "hx-trigger": "load",
            "hx-target": "#page-main",
        })
}

window.postLogin = async () => {
    console.log("postLogin");
    let auth0Client = null;
    const fetchAuthConfig = () => fetch("/static/auth_config.json");
    const configureClient = async () => {
        const response = await fetchAuthConfig();
        const config = await response.json();
        auth0Client = await createAuthClient(config);
    };

    await configureClient();

    //recheck

    const isAuthenticated = await auth0Client.isAuthenticated();


    const query = window.location.search;
    const shouldParseResult = query.includes("code=") && query.includes("state=");

    if (shouldParseResult) {
        console.log("> Parsing redirect");
        try {
            const result = await auth0Client.handleRedirectCallback();

            if (result.appState && result.appState.targetUrl) {
                showContentFromUrl(result.appState.targetUrl);
            }

            console.log("Logged in!");
        } catch (err) {
            window.location.href = window.location.origin + "/login";
            console.log("Error parsing redirect:", err);
        }

    }

    //https://auth0.com/docs/quickstart/spa/vanillajs/02-calling-an-api 
    try {
        const token = await auth0Client.getTokenSilently();
        console.log(token);
        // let response = await fetch("/callback-confirm", {headers: {Authorization: "Bearer " + token}})
        let element = van.add(document.body, loginPostComponent(token));
        htmx.process(document.body);
    } catch (err) {
        console.log("error", err);
    }
}


window.numberChanged = (element) => {
    let allowed = "";
    let actual = element.value;
    for (const letter of actual) {
        if (!isNaN(letter)) {
            allowed = allowed + letter;
        }

    }
    if (Number(allowed) > 31) {
        element.value = 31;
        return;
    }
    element.value = allowed;
}


window.valueInputKeyUp = (event) => {
    if (event.key === "Backspace" && event.target.value == "R$ ") {
        event.preventDefault();
    }
    if (isNaN(event.key) && event.key !== 'Backspace' && event.key !== ',') {
        event.preventDefault();
    }
    console.log(event);
}


window.dateInputKeyUp = (event) => {
    if (isNaN(event.key) && event.key !== 'Backspace' && event.key !== '/') {
        event.preventDefault();
    }
    console.log(event);
}

window.saveTheme = (t) => {
    localStorage.setItem("theme", t);
    document.theme = t;
    applyTheme(t);
}

window.loadTheme = () => {
    var t = localStorage.getItem("theme");
    document.theme = t;
    applyTheme(t);
}

function applyTheme(t) {
    let el = document.getElementsByTagName("html")[0];
    if (t == "dark") {
        el.classList.add("pf-v6-theme-dark");
        document.getElementById("b-light").classList.remove("pf-m-selected")
        document.getElementById("b-dark").classList.add("pf-m-selected")
    } else {
        el.classList.remove("pf-v6-theme-dark");
        document.getElementById("b-light").classList.add("pf-m-selected")
        document.getElementById("b-dark").classList.remove("pf-m-selected")
    }
}


window.addQuill = (id) => {
    const quill = new Quill(id, {
        theme: 'snow'
    });
    const element = document.getElementById('note__text');
 
    
  /*  quill.on('text-change', (d,o, source) => {
        element.value = quill.container.innerHTML;
    })*/

}
