class FilesListComponent {
    filesList;
    fileActionFunc;

    constructor(filesList, fileActionFunc) {
        this.filesList = filesList;
        this.fileActionFunc = fileActionFunc;
    }

    create() {
        const div = document.createElement("div");

        const thead = document.createElement("thead");
        thead.append(this.#createTableRow(["Anon", "Path", "Size"]));

        const tbody = document.createElement("tbody");
        for (const file of this.filesList) {
            const downloadButton = this.#createButton("Download", () => this.fileActionFunc(file, "download"));
            const viewButton = this.#createButton("View", () => this.fileActionFunc(file, "view"));
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
}

class FileUploadComponent {
    #fileInput;
    #uploadButton;
    #cancelButton;
    #isUploadInProgress;
    uploadFunc;

    constructor(uploadFunc) {
        this.uploadFunc = uploadFunc;
    }

    create() {
        const div = document.createElement("div");

        if (!this.#fileInput)
            this.#fileInput = this.#createFileInput();
        div.appendChild(this.#fileInput);

        if (!this.#uploadButton)
            this.#uploadButton = this.#createButton("Upload", () => this.#onUploadButtonClick());
        div.appendChild(this.#uploadButton);

        if (!this.#cancelButton)
            this.#cancelButton = this.#createButton("Cancel", null);
        div.appendChild(this.#cancelButton);

        return div;
    }

    #createFileInput() {
        const fileInput = document.createElement("input");
        fileInput.type = "file";
        fileInput.addEventListener("change", () => this.#uploadButton.disabled = !fileInput.files[0] || this.#isUploadInProgress);
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

    async #onUploadButtonClick() {
        this.#uploadButton.disabled = true;
        this.#isUploadInProgress = true;
        await this.uploadFunc(this.#fileInput.files[0],
            (abortRequestFunc) => this.#onUploadStarted(abortRequestFunc),
            (success) => this.#onUploadCompleted(success));
        this.#isUploadInProgress = false;
        this.#uploadButton.disabled = !this.#fileInput.files[0];
    }

    #onUploadStarted(abortRequestFunc) {
        this.#cancelButton.onclick = abortRequestFunc;
        this.#cancelButton.disabled = false;
    }

    #onUploadCompleted(success) {
        this.#cancelButton.disabled = true;
        this.#cancelButton.onclick = null;
        if (success)
            this.#fileInput = this.#createFileInput();
    }
}

class LoginFormComponent {
    #passwordInput;
    #loginButton;
    loginFunc;

    constructor(loginFunc) {
        this.loginFunc = loginFunc;
    }

    create() {
        const div = document.createElement("div");

        div.append("Key:");
        if (!this.#passwordInput)
            this.#passwordInput = this.#createPasswordInput();
        div.appendChild(this.#passwordInput);

        if (!this.#loginButton)
            this.#loginButton = this.#createLoginButton();
        div.appendChild(this.#loginButton);

        return div;
    }

    #createPasswordInput() {
        const passwordInput = document.createElement("input");
        passwordInput.type = "password";
        passwordInput.name = "key";
        passwordInput.addEventListener("input", () => this.#loginButton.disabled = !passwordInput.value);
        return passwordInput;
    }

    #createLoginButton() {
        const loginButton = document.createElement("input");
        loginButton.type = "submit";
        loginButton.value = "Login";
        loginButton.disabled = true;
        loginButton.onclick = () => this.loginFunc(this.#passwordInput.value);
        return loginButton;
    }
}
