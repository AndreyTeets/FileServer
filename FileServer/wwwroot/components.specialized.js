class FilesListComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const div = VDom.createElement("div");

        const thead = VDom.createElement("thead");
        thead.append(this.#createTableRow(["Anon", "Path", "Size"]));

        const tbody = VDom.createElement("tbody");
        for (const file of this.props.files) {
            const downloadButton = this.#createButton("Download", () => this.#openFile(file, "download"));
            const viewButton = this.#createButton("View", () => this.#openFile(file, "view"));
            tbody.append(this.#createTableRow([file.anon, file.path, file.size, downloadButton, viewButton]));
        }

        const table = VDom.createElement("table");
        table.append(thead);
        table.append(tbody);
        div.append(table);

        return div;
    }

    #createTableRow(columns) {
        const tr = VDom.createElement("tr");
        for (const col of columns) {
            const td = VDom.createElement("td");
            td.append(["string", "number"].includes(typeof(col)) ? VDom.createTextNode(col) : col);
            tr.append(td);
        }
        return tr;
    }

    #createButton(name, onclickFunc) {
        const button = VDom.createElement("input");
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
    #fileInputVDomForceResetId = 0;

    constructor() {
        super();
        this.state = { isFileSelected: false, isUploadInProgress: false, cancelButtonFunc: null };
        this.#fileInput = this.#createFileInput();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(this.#fileInput);
        div.append(this.#createButton("Upload", this.#onUploadButtonClick, this.#isUploadButtonEnabled()));
        div.append(this.#createButton("Cancel", this.#onCancelButtonClick, this.#isCancelButtonEnabled()));
        return div;
    }

    #createFileInput() {
        const fileInput = VDom.createElement("input");
        fileInput.type = "file";
        fileInput.vEventListeners["change"] = [this.#onFileInputChange];
        fileInput.vDomForceResetId = this.#fileInputVDomForceResetId++;
        return fileInput;
    }

    #createButton(name, onclickFunc, disabled) {
        const button = VDom.createElement("input");
        button.type = "submit";
        button.value = name;
        button.disabled = disabled;
        button.onclick = onclickFunc;
        return button;
    }

    #isUploadButtonEnabled = () => !this.state.isFileSelected || this.state.isUploadInProgress;
    #isCancelButtonEnabled = () => !this.state.isUploadInProgress;
    #onFileInputChange = () => this.setState({ isFileSelected: !!this.#fileInput.getDomElem().files[0] });

    #onUploadButtonClick = async () => {
        this.setState({ isUploadInProgress: true });
        await this.props.uploadFileFunc(this.#fileInput.getDomElem().files[0], this.#onUploadStarted, this.#onUploadCompleted);
        this.setState({ isUploadInProgress: false });
    }

    #onCancelButtonClick = () => {
        if (this.state.cancelButtonFunc)
            this.state.cancelButtonFunc();
    }

    #onUploadStarted = (abortRequestFunc) => {
        this.setState({ cancelButtonFunc: abortRequestFunc });
    }

    #onUploadCompleted = (success) => {
        this.setState({ cancelButtonFunc: null });
        if (success) {
            this.#fileInput = this.#createFileInput();
            this.setState({ isFileSelected : false });
        }
    }
}

class LoginFormComponent extends ComponentBase {
    #passwordInput;

    constructor() {
        super();
        this.state = { isPasswordEmpty: true };
        this.#passwordInput = this.#createPasswordInput();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(VDom.createTextNode("Key:"));
        div.append(this.#passwordInput);
        div.append(this.#createLoginButton(this.state.isPasswordEmpty));
        return div;
    }

    #createPasswordInput() {
        const passwordInput = VDom.createElement("input");
        passwordInput.type = "password";
        passwordInput.name = "key";
        passwordInput.vEventListeners["input"] = [this.#onPasswordInputChange];
        return passwordInput;
    }

    #createLoginButton(disabled) {
        const loginButton = VDom.createElement("input");
        loginButton.type = "submit";
        loginButton.value = "Login";
        loginButton.disabled = disabled;
        loginButton.onclick = this.#onLoginButtonClick;
        return loginButton;
    }

    #onPasswordInputChange = () => this.setState({ isPasswordEmpty: !this.#passwordInput.getDomElem().value });
    #onLoginButtonClick = () => this.props.loginFunc(this.#passwordInput.getDomElem().value);
}
