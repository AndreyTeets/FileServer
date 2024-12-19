class FilesListComponent {
    filesList;
    downloadFunc;

    constructor(filesList, downloadFunc) {
        this.filesList = filesList;
        this.downloadFunc = downloadFunc;
    }

    create() {
        const div = document.createElement("div");

        const table = document.createElement("table");
        const thead = document.createElement("thead");
        const tbody = document.createElement("tbody");
        table.append(thead);
        table.append(tbody);

        thead.append(this.#createTableRow(["Path", "Size"]));
        for (const file of this.filesList) {
            const button = document.createElement("input");
            button.type = "submit";
            button.value = "Download";
            button.onclick = () => this.downloadFunc(file);

            tbody.appendChild(this.#createTableRow([file.path, file.size, button]));
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
