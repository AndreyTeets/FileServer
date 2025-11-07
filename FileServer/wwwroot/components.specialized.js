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
            const downloadButton = new ButtonComponent().render({ text: "Download", onclick: () => this.#openFile(file, "download") });
            const viewButton = new ButtonComponent().render({ text: "View", onclick: () => this.#openFile(file, "view") });
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

    #openFile(file, action) {
        const a = document.createElement("a");
        a.href = Api.createFileLink(file.path, action, file.anon);
        a.target = `_blank`;
        a.click();
    }
}

class FileUploadComponent extends ComponentBase {
    #fileInput;
    #fileInputCreationCounter = 0;

    constructor() {
        super();
        this.state = { isFileSelected: false, isUploadInProgress: false, cancelButtonFunc: null };
        this.#fileInput = this.#createFileInput();
    }

    renderCore() {
        const div = VDom.createElement("div");
        div.append(this.#fileInput);
        div.append(new ButtonComponent().render({ text: "Upload", onclick: this.#onUploadButtonClick, disabled: this.#isUploadButtonDisabled() }));
        div.append(new ButtonComponent().render({ text: "Cancel", onclick: this.#onCancelButtonClick, disabled: this.#isCancelButtonDisabled() }));
        return div;
    }

    #createFileInput() {
        const fileInput = VDom.createElement("input");
        fileInput.type = "file";
        fileInput.vEventListeners["change"] = [this.#onFileInputChange];
        fileInput.vIdToForceChange = this.#fileInputCreationCounter++;
        return fileInput;
    }

    #isUploadButtonDisabled= () => !this.state.isFileSelected || this.state.isUploadInProgress;
    #isCancelButtonDisabled = () => !this.state.isUploadInProgress;
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
        div.append(new ButtonComponent().render({ text: "Login", onclick: this.#onLoginButtonClick, disabled: this.#isLoginButtonDisabled() }));
        return div;
    }

    #createPasswordInput() {
        const passwordInput = VDom.createElement("input");
        passwordInput.type = "password";
        passwordInput.name = "key";
        passwordInput.vEventListeners["input"] = [this.#onPasswordInputChange];
        return passwordInput;
    }

    #isLoginButtonDisabled = () => this.state.isPasswordEmpty;
    #onLoginButtonClick = () => this.props.loginFunc(this.#passwordInput.getDomElem().value);
    #onPasswordInputChange = () => this.setState({ isPasswordEmpty: !this.#passwordInput.getDomElem().value });
}
