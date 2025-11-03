class DownloadPageComponent {
    data;

    constructor(data) {
        this.data = data
    }

    create() {
        const div = document.createElement("div");
        div.appendChild(new HeaderComponent("DownloadPage:").create());

        if (this.data.state.files)
            div.appendChild(new FilesListComponent(this.data.state.files, this.data.fileActionFunc).create());
        if (this.data.state.status.text)
            div.appendChild(new TextComponent(this.data.state.status.text).create());
        if (this.data.state.status.error)
            div.appendChild(new ErrorComponent(this.data.state.status.error).create());

        return div;
    }
}

class UploadPageComponent {
    #fileUploadComponent;
    data;

    constructor(data) {
        this.data = data
    }

    create() {
        const div = document.createElement("div");
        div.appendChild(new HeaderComponent("UploadPage:").create());

        if (!this.#fileUploadComponent)
            this.#fileUploadComponent = new FileUploadComponent(this.data.uploadFunc);
        div.appendChild(this.#fileUploadComponent.create());

        if (this.data.state.status.text)
            div.appendChild(new TextComponent(this.data.state.status.text).create());
        if (this.data.state.status.error)
            div.appendChild(new ErrorComponent(this.data.state.status.error).create());

        return div;
    }
}

class AuthPageComponent {
    #loginFormComponent;
    data;

    constructor(data) {
        this.data = data
    }

    create() {
        const div = document.createElement("div");
        div.appendChild(new HeaderComponent("AuthPage:").create());

        if (this.data.state.loggedIn) {
            div.appendChild(new ButtonComponent("Logout", this.data.logoutFunc).create());
        } else {
            if (!this.#loginFormComponent)
                this.#loginFormComponent = new LoginFormComponent(this.data.loginFunc);
            div.appendChild(this.#loginFormComponent.create());
        }

        if (this.data.state.status.text)
            div.appendChild(new TextComponent(this.data.state.status.text).create());
        if (this.data.state.status.error)
            div.appendChild(new ErrorComponent(this.data.state.status.error).create());

        return div;
    }
}
