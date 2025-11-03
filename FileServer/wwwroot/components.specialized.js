class FilesListComponent {
    filesList;
    fileActionFunc;

    constructor(filesList, fileActionFunc) {
        this.filesList = filesList;
        this.fileActionFunc = fileActionFunc;
    }

    create() {
        const div = document.createElement("div");

        const table = document.createElement("table");
        const thead = document.createElement("thead");
        const tbody = document.createElement("tbody");
        table.append(thead);
        table.append(tbody);

        thead.append(this.#createTableRow(["Anon", "Path", "Size"]));
        for (const file of this.filesList) {
            const downloadButton = document.createElement("input");
            downloadButton.type = "submit";
            downloadButton.value = "Download";
            downloadButton.onclick = () => this.fileActionFunc(file, 'download');

            const viewButton = document.createElement("input");
            viewButton.type = "submit";
            viewButton.value = "View";
            viewButton.onclick = () => this.fileActionFunc(file, 'view');

            tbody.appendChild(this.#createTableRow([file.anon, file.path, file.size, downloadButton, viewButton]));
        }

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
            this.#uploadButton = this.#createUploadButton();
        div.appendChild(this.#uploadButton);

        if (!this.#cancelButton)
            this.#cancelButton = this.#createCancelButton();
        div.appendChild(this.#cancelButton);

        return div;
    }

    #createFileInput() {
        const fileInput = document.createElement("input");
        fileInput.type = "file";
        fileInput.addEventListener("change", () => this.#uploadButton.disabled = !fileInput.files[0] || this.#isUploadInProgress);
        return fileInput;
    }

    #createUploadButton() {
        const uploadButton = document.createElement("input");
        uploadButton.type = "submit";
        uploadButton.value = "Upload";
        uploadButton.disabled = true;
        uploadButton.onclick = () => {
            this.#isUploadInProgress = true;
            uploadButton.disabled = true;
            this.uploadFunc(this.#fileInput.files[0], (abortRequestFunc) => {
                this.#cancelButton.onclick = abortRequestFunc;
                this.#cancelButton.disabled = false;
            }, (success) => {
                this.#cancelButton.disabled = true;
                if (success)
                    this.#fileInput = this.#createFileInput();
                this.#isUploadInProgress = false;
                uploadButton.disabled = !this.#fileInput.files[0];
            });
        };
        return uploadButton;
    }

    #createCancelButton() {
        const button = document.createElement("input");
        button.type = "submit";
        button.value = "Cancel";
        button.disabled = true;
        return button;
    }
}

class LoginFormComponent {
    loginFunc;

    constructor(loginFunc) {
        this.loginFunc = loginFunc;
    }

    create() {
        const div = document.createElement("div");

        const passwordInput = document.createElement("input");
        passwordInput.type = "password";
        passwordInput.name = "key";
        div.append("Key:");
        div.appendChild(passwordInput);

        const buttonInput = document.createElement("input");
        buttonInput.type = "submit";
        buttonInput.value = "Login";
        buttonInput.onclick = () => this.loginFunc(passwordInput.value);
        buttonInput.disabled = true;
        div.appendChild(buttonInput);

        passwordInput.addEventListener("input", () => { buttonInput.disabled = !passwordInput.value; });

        return div;
    }
}
