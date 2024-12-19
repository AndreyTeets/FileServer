class Api {
    static #antiforgeryTokenHeaderName = "FileServer-AntiforgeryToken";
    static #antiforgeryTokenQueryParamName = "antiforgeryToken";

    static getFileLink(filePath) {
        const urlEncodedFilePath = filePath.split("/").map(x => encodeURIComponent(x)).join("/");
        const authData = Auth.get();
        if (authData)
            return `/api/files/download/${urlEncodedFilePath}?${Api.#antiforgeryTokenQueryParamName}=${authData.antiforgeryToken}`;
        else
            return undefined;
    }

    static async getFilesList() {
        const url = "api/files/list";
        const params = Api.#createParams("GET");
        return await Api.#send(url, params);
    }

    static async uploadFile(fileFormData) {
        const url = "api/files/upload";
        const params = Api.#createParams("POST", fileFormData);
        return await Api.#send(url, params);
    }

    static async login(credentials) {
        const url = "api/auth/login";
        const params = Api.#createParams("POST", JSON.stringify(credentials));
        params.headers["Content-Type"] = "application/json";
        return await Api.#send(url, params);
    }

    static async logout() {
        const url = "api/auth/logout";
        const params = Api.#createParams("POST");
        return await Api.#send(url, params);
    }

    static #createParams(method, body) {
        const params = {
            method: method,
            headers: {},
            body: body
        };

        const authData = Auth.get();
        if (authData)
            params.headers[Api.#antiforgeryTokenHeaderName] = authData.antiforgeryToken;

        return params;
    }

    static async #send(url, params) {
        var errorText;
        const responseData = await fetch(url, params)
            .then(async response => {
                if (response.status === 401)
                    Auth.clear();

                if (response.status === 200) {
                    const contentType = response.headers.get("Content-Type");
                    if (contentType && contentType.trim().startsWith("application/json"))
                        return await response.json();
                    else
                        return await response.text();
                } else {
                    const text = await response.text();
                    throw new Error(`Response status: ${response.status} ${response.statusText}. Response body: ${text}`);
                }
            })
            .catch(error => {
                errorText = `Fetch error: ${error}`;
            });
        return [responseData, errorText];
    }
}
