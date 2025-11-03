class Api {
    static #antiforgeryTokenHeaderName = "FileServer-AntiforgeryToken";
    static #antiforgeryTokenQueryParamName = "antiforgeryToken";

    static createFileLink(filePath, action, fileAnon) {
        const urlEncodedFilePath = filePath.split("/").map(x => encodeURIComponent(x)).join("/");
        const authData = Auth.get();
        if (fileAnon === "yes")
            return `/api/files/${action}anon/${urlEncodedFilePath}`;
        else if (authData && fileAnon === "no")
            return `/api/files/${action}/${urlEncodedFilePath}?${Api.#antiforgeryTokenQueryParamName}=${authData.antiforgeryToken}`;
        else
            throw new Error(`No auth data or bad anon='${fileAnon}' while creating file link.`);
    }

    static getFilesList() {
        const url = "api/files/list";
        const params = Api.#createParams("GET");
        return Api.#send(url, params)[0];
    }

    static uploadFile(fileFormData, onProgressFunc) {
        const url = "api/files/upload";
        const params = Api.#createParams("POST", fileFormData);
        return Api.#send(url, params, onProgressFunc);
    }

    static login(credentials) {
        const url = "api/auth/login";
        const params = Api.#createParams("POST", JSON.stringify(credentials));
        params.headers["Content-Type"] = "application/json";
        return Api.#send(url, params)[0];
    }

    static logout() {
        const url = "api/auth/logout";
        const params = Api.#createParams("POST");
        return Api.#send(url, params)[0];
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

    static #send(url, params, onProgressFunc) {
        const [fetchResponsePromise, abortRequestFunc] = Api.#xhrFetch(url, params, onProgressFunc);
        const responsePromise = (async () => {
            let errorText;
            const responseData = await fetchResponsePromise
                .then(async response => {
                    if (response.status === 401)
                        Auth.clear();

                    if (response.ok) {
                        const contentType = response.headers.get("Content-Type");
                        if (contentType && contentType.trim().startsWith("application/json"))
                            return await response.json();
                        else
                            return await response.text();
                    } else {
                        const text = await response.text();
                        throw new Error(`Response status: ${response.status}. Response body: ${text}`);
                    }
                })
                .catch(error => {
                    errorText = `Failed to fetch: ${error.message}`;
                });
            return [responseData, errorText];
        })();

        return [responsePromise, abortRequestFunc];
    }

    static #xhrFetch(url, params, onProgressFunc) {
        const xhr = new XMLHttpRequest();
        const method = params.method || "GET";
        const headers = params.headers || {};
        const body = params.body || null;
        const abortRequestFunc = () => xhr.abort();

        const responsePromise = new Promise((resolve, reject) => {
            xhr.open(method, url);
            for (const headerName in headers)
                xhr.setRequestHeader(headerName, headers[headerName]);

            xhr.onload = () => {
                const response = {
                    ok: xhr.status >= 200 && xhr.status <= 299,
                    status: xhr.status,
                    headers: new Headers(),
                    json: () => Promise.resolve(JSON.parse(xhr.responseText)),
                    text: () => Promise.resolve(xhr.responseText),
                };

                xhr.getAllResponseHeaders().split("\r\n").forEach(header => {
                    const headerName = header.split(": ", 1)[0];
                    if (headerName.trim() !== "")
                        response.headers.append(headerName, header.substring(headerName.length + 2));
                });

                resolve(response);
            };

            xhr.onerror = () => { reject(new Error("HTTP request failed due to a network error.")); };
            xhr.onabort = () => { reject(new Error("HTTP request was aborted.")); };

            if (onProgressFunc) {
                xhr.upload.onprogress = (event) => {
                    if (event.lengthComputable) {
                        const percentComplete = (event.loaded / event.total) * 100;
                        onProgressFunc(percentComplete);
                    }
                };
            }

            xhr.send(body);
        });

        return [responsePromise, abortRequestFunc];
    }
}
