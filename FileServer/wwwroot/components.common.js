class TextComponent extends ComponentBase {
    cssClass = "text";

    constructor() {
        super();
    }

    renderCore() {
        const p = VDom.createElement("p");
        p.append(VDom.createTextNode(this.props.text));
        p.vClasses.push(this.cssClass);
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
        const input = VDom.createElement("input");
        input.type = "submit";
        input.value = this.props.text;
        input.onclick = this.props.onclickFunc;
        return input;
    }
}
