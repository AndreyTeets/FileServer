class TextComponent extends ComponentBase {
    cssClass = "text";

    constructor() {
        super();
    }

    renderCore() {
        const p = document.createElement("p");
        p.append(this.props.text);
        p.classList.add(this.cssClass);
        return p;
    }
}

class ErrorComponent extends TextComponent {
    cssClass = "error";
}

class HeaderComponent extends TextComponent {
    cssClass = "header";
}

class ButtonComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const input = document.createElement("input");
        input.type = "submit";
        input.value = this.props.text;
        input.onclick = this.props.onclickFunc;
        return input;
    }
}
