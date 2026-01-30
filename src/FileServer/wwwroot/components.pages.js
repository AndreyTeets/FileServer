class DownloadPageComponent extends ComponentBase {
    #filesListComponent;

    constructor() {
        super();
        this.state = { files: null, status: {} };
        this.#filesListComponent = new FilesListComponent();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(new HeaderComponent().render({ text: "DownloadPage:" }));

        if (this.state.files)
            div.append(this.#filesListComponent.render({ files: this.state.files }));

        div.append(new StatusComponent().render({ status: this.state.status }));
        return div;
    }

    async loadFilesList() {
        this.setState({ files: null, status: { text: "Loading..." } });
        const [response, errorText] = await Api.getFilesList();
        if (errorText)
            this.setState({ status: { error: errorText } });
        else
            this.setState({ files: response.files, status: {} });
    }
}

class UploadPageComponent extends ComponentBase {
    #fileUploadComponent;

    constructor() {
        super();
        this.state = { status: {} };
        this.#fileUploadComponent = new FileUploadComponent();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(new HeaderComponent().render({ text: "UploadPage:" }));
        div.append(this.#fileUploadComponent.render({ uploadFileFunc: this.#uploadFile }));
        div.append(new StatusComponent().render({ status: this.state.status }));
        return div;
    }

    #uploadFile = async (file, onUploadStarted, onUploadCompleted) => {
        const fileFormData = new FormData();
        fileFormData.append("file", file);

        this.setState({ status: { text: "Uploading..." } });
        let timeOfLastProgressUpdate = Date.now();
        const [responsePromise, abortRequestFunc] = Api.uploadFile(fileFormData, (progress) => {
            if (Date.now() - timeOfLastProgressUpdate >= 500) {
                this.setState({ status: { text: `Uploading... ${progress.toFixed(2)}%` } });
                timeOfLastProgressUpdate = Date.now();
            }
        });

        onUploadStarted(abortRequestFunc);
        const [response, errorText] = await responsePromise;
        onUploadCompleted(!errorText);

        if (errorText)
            this.setState({ status: { error: errorText } });
        else
            this.setState({ status: { text: `Uploaded: ${response.createdFileName}` } });
    }
}

class AuthPageComponent extends ComponentBase {
    #loginFormComponent;

    constructor() {
        super();
        this.state = { loggedIn: false, status: {} };
        this.#loginFormComponent = new LoginFormComponent();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(new HeaderComponent().render({ text: "AuthPage:" }));

        if (this.state.loggedIn)
            div.append(new ButtonComponent().render({ text: "Logout", onclick: this.#logout }));
        else
            div.append(this.#loginFormComponent.render({ loginFunc: this.#login }));

        div.append(new StatusComponent().render({ status: this.state.status }));
        return div;
    }

    setAuthState() {
        this.setState({ loggedIn: !!Auth.get() });
    }

    #login = async (password) => {
        const credentials = { password: password };

        this.setState({ status: { text: "Authenticating..." } });
        const [response, errorText] = await Api.login(credentials);
        if (errorText) {
            this.setState({ loggedIn: false, status: { error: errorText } });
        } else {
            Auth.set(response.loginInfo, response.antiforgeryToken);
            this.setState({ loggedIn: true, status: {} });
        }
    }

    #logout = async () => {
        this.setState({ status: { text: "Logging out..." } });
        const [_, errorText] = await Api.logout();
        if (errorText) { // Stay logged-in on errors except 401 (Api call already clears Auth on 401)
            this.setState({ loggedIn: !!Auth.get(), status: { error: errorText } });
        } else {
            Auth.clear();
            this.setState({ loggedIn: false, status: {} });
        }
    }
}
