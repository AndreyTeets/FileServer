class AppLogic {
    static #downloadPage;
    static #uploadPage;
    static #authPage;

    static async setDownloadPage() {
        if (!AppLogic.#downloadPage) {
            const pageData = AppLogic.#createProxifiedPageData(DownloadPageComponent);
            pageData.fileActionFunc = async (file, action) => {
                const a = document.createElement("a");
                a.href = Api.createFileLink(file.path, action, file.anon);
                a.target = `_blank`;
                a.click();
            };
            AppLogic.#downloadPage = new DownloadPageComponent(pageData);
        }

        AppLogic.#downloadPage.data.state = { status: { text: "Loading..." } };
        App.page = AppLogic.#downloadPage;

        const [response, errorText] = await Api.getFilesList();
        if (errorText)
            AppLogic.#downloadPage.data.state.status = { error: errorText };
        else
            AppLogic.#downloadPage.data.state = { files: response.files, status: {} };
    }

    static setUploadPage() {
        if (!AppLogic.#uploadPage) {
            const pageData = AppLogic.#createProxifiedPageData(UploadPageComponent);
            pageData.uploadFunc = async (file, onUploadStarted, onUploadCompleted) => {
                const fileFormData = new FormData();
                fileFormData.append("file", file);

                pageData.state.status = { text: "Uploading..." };
                let timeOfLastProgressUpdate = Date.now();
                const [responsePromise, abortRequestFunc] = Api.uploadFile(fileFormData, (progress) => {
                    if (Date.now() - timeOfLastProgressUpdate >= 500) {
                        pageData.state.status = { text: `Uploading... ${progress.toFixed(2)}%` };
                        timeOfLastProgressUpdate = Date.now();
                    }
                });

                onUploadStarted(abortRequestFunc);
                const [response, errorText] = await responsePromise;
                onUploadCompleted(!errorText);

                if (errorText)
                    pageData.state.status = { error: errorText };
                else
                    pageData.state.status = { text: `Uploaded: ${response.createdFileName}` };
            };
            pageData.state = { status: {} };
            AppLogic.#uploadPage = new UploadPageComponent(pageData);
        }

        App.page = AppLogic.#uploadPage;
    }

    static setAuthPage() {
        if (!AppLogic.#authPage) {
            const pageData = AppLogic.#createProxifiedPageData(AuthPageComponent);
            pageData.loginFunc = async (password) => {
                const credentials = { password: password };

                pageData.state.status = { text: "Authenticating..." };
                const [response, errorText] = await Api.login(credentials);
                if (errorText) {
                    pageData.state.status = { error: errorText };
                } else {
                    Auth.set(response.loginInfo, response.antiforgeryToken);
                    pageData.state = { loggedIn: true, status: {} };
                }
            };
            pageData.logoutFunc = async () => {
                pageData.state.status = { text: "Logging out..." };
                const [_, errorText] = await Api.logout();
                if (errorText) {
                    pageData.state = { loggedIn: !!Auth.get(), status: { error: errorText } };
                } else {
                    Auth.clear();
                    pageData.state = { loggedIn: false, status: {} };
                }
            };
            AppLogic.#authPage = new AuthPageComponent(pageData);
        }

        AppLogic.#authPage.data.state = { loggedIn: !!Auth.get(), status: {} };
        App.page = AppLogic.#authPage;
    }

    static #createProxifiedPageData(pageClass) {
        const isDataForCurrentPage = () => App.page instanceof pageClass;
        return App.proxifyObjectPropertiesForRenderOnChange({ state: {} }, isDataForCurrentPage);
    }
}
