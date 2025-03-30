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
    uploadFunc;

    constructor(uploadFunc) {
        this.uploadFunc = uploadFunc;
    }

    create() {
        const div = document.createElement("div");

        const fileInput = document.createElement("input");
        fileInput.type = "file";
        div.appendChild(fileInput);

        const buttonInput = document.createElement("input");
        buttonInput.type = "submit";
        buttonInput.value = "Upload";
        buttonInput.onclick = () => this.uploadFunc(fileInput.files[0]);
        buttonInput.disabled = true;
        div.appendChild(buttonInput);

        fileInput.addEventListener("change", () => { buttonInput.disabled = !fileInput.files[0]; });

        return div;
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
