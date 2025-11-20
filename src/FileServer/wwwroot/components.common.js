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

class StatusComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const div = VDom.createElement("div");
        if (this.props.status.text)
            div.append(new TextComponent().render({ text: this.props.status.text }));
        if (this.props.status.error)
            div.append(new ErrorComponent().render({ text: this.props.status.error }));
        return div;
    }
}

class ButtonComponent extends ComponentBase {
    constructor() {
        super();
    }

    renderCore() {
        const input = VDom.createElement("input");
        input.type = "submit";
        input.value = this.props.text;
        input.disabled = !!this.props.disabled;
        input.onclick = this.props.onclick;
        return input;
    }
}
