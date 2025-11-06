class FilesListComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const div = document.createElement("div");

        const thead = document.createElement("thead");
        thead.append(this.#createTableRow(["Anon", "Path", "Size"]));

        const tbody = document.createElement("tbody");
        for (const file of this.props.files) {
            const downloadButton = this.#createButton("Download", () => this.#openFile(file, "download"));
            const viewButton = this.#createButton("View", () => this.#openFile(file, "view"));
            tbody.appendChild(this.#createTableRow([file.anon, file.path, file.size, downloadButton, viewButton]));
        }

        const table = document.createElement("table");
        table.append(thead);
        table.append(tbody);
        div.appendChild(table);

        return div;
    }

    #createTableRow(columns) {
        const tr = document.createElement("tr");
        for (const col of columns) {
            const td = document.createElement("td");
            td.append(col);
            tr.append(td);
        }
        return tr;
    }

    #createButton(name, onclickFunc) {
        const button = document.createElement("input");
        button.type = "submit";
        button.value = name;
        button.onclick = onclickFunc;
        return button;
    }

    #openFile(file, action) {
        const a = document.createElement("a");
        a.href = Api.createFileLink(file.path, action, file.anon);
        a.target = `_blank`;
        a.click();
    }
}

class FileUploadComponent extends ComponentBase {
    #fileInput;
    #uploadButton;
    #cancelButton;

    constructor() {
        super();
        this.state = { isFileSelected: false, isUploadInProgress: false };
        this.#fileInput = this.#createFileInput();
        this.#uploadButton = this.#createButton("Upload", this.#onUploadButtonClick);
        this.#cancelButton = this.#createButton("Cancel", null);
    }

    renderCore() {
        const div = document.createElement("div");

        this.#uploadButton.disabled = !this.state.isFileSelected || this.state.isUploadInProgress;
        this.#cancelButton.disabled = !this.state.isUploadInProgress;

        div.appendChild(this.#fileInput);
        div.appendChild(this.#uploadButton);
        div.appendChild(this.#cancelButton);

        return div;
    }

    #createFileInput() {
        const fileInput = document.createElement("input");
        fileInput.type = "file";
        fileInput.addEventListener("change", () => this.setState({ isFileSelected: !!fileInput.files[0] }));
        return fileInput;
    }

    #createButton(name, onclickFunc) {
        const button = document.createElement("input");
        button.type = "submit";
        button.value = name;
        button.disabled = true;
        button.onclick = onclickFunc;
        return button;
    }

    #onUploadButtonClick = async () => {
        this.setState({ isUploadInProgress: true });
        await this.props.uploadFileFunc(this.#fileInput.files[0], this.#onUploadStarted, this.#onUploadCompleted);
        this.setState({ isUploadInProgress: false });
    }

    #onUploadStarted = (abortRequestFunc) => {
        this.#cancelButton.onclick = abortRequestFunc;
    }

    #onUploadCompleted = (success) => {
        this.#cancelButton.onclick = null;
        if (success)
            this.#fileInput = this.#createFileInput();
    }
}

class LoginFormComponent extends ComponentBase {
    #passwordInput;
    #loginButton;

    constructor() {
        super();
        this.state = { isPasswordEmpty: true };
        this.#passwordInput = this.#createPasswordInput();
        this.#loginButton = this.#createLoginButton();
    }

    renderCore() {
        const div = document.createElement("div");

        this.#loginButton.disabled = this.state.isPasswordEmpty;

        div.append("Key:");
        div.appendChild(this.#passwordInput);
        div.appendChild(this.#loginButton);

        return div;
    }

    #createPasswordInput() {
        const passwordInput = document.createElement("input");
        passwordInput.type = "password";
        passwordInput.name = "key";
        passwordInput.addEventListener("input", () => this.setState({ isPasswordEmpty: !passwordInput.value }));
        return passwordInput;
    }

    #createLoginButton() {
        const loginButton = document.createElement("input");
        loginButton.type = "submit";
        loginButton.value = "Login";
        loginButton.disabled = true;
        loginButton.onclick = () => this.props.loginFunc(this.#passwordInput.value);
        return loginButton;
    }
}
