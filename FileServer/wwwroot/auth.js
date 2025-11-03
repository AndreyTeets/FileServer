class Auth {
    static #loginInfoStorageKey = "FileServer-LoginInfo";
    static #antiforgeryTokenStorageKey = "FileServer-AntiforgeryToken";

    static get() {
        const loginInfo = JSON.parse(localStorage.getItem(Auth.#loginInfoStorageKey));
        if (loginInfo && (Date.parse(loginInfo.tokensExpire) - Date.now() > 5 * 1000)) {
            const antiforgeryToken = localStorage.getItem(Auth.#antiforgeryTokenStorageKey);
            return {
                loginInfo: loginInfo,
                antiforgeryToken: antiforgeryToken,
            };
        } else {
            if (loginInfo)
                Auth.clear();
            return undefined;
        }
    }

    static set(loginInfo, antiforgeryToken) {
        localStorage.setItem(Auth.#loginInfoStorageKey, JSON.stringify(loginInfo));
        localStorage.setItem(Auth.#antiforgeryTokenStorageKey, antiforgeryToken);
    }

    static clear() {
        localStorage.removeItem(Auth.#loginInfoStorageKey);
        localStorage.removeItem(Auth.#antiforgeryTokenStorageKey);
    }
}
