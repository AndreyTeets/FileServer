class VDomElement {
    vParent = null;
    vChildren = [];
    vClasses = [];
    vEventListeners = {};
    vType;
    #domElem;

    constructor(type) {
        this.vType = type;
    }

    append(elem) {
        this.vChildren.push(elem);
        elem.vParent = this;
    }

    getDomElem() {
        if (!this.#domElem) {
            if (this.vType === "vText") {
                this.#domElem = document.createTextNode(this.vText);
            } else {
                this.#domElem = document.createElement(this.vType);
                for (const prop of Object.keys(this).filter(x => !Object.keys(new VDomElement("")).includes(x)))
                    this.#domElem[prop] = this[prop];
                for (const cls of this.vClasses)
                    this.#domElem.classList.add(cls);
                for (const event of Object.keys(this.vEventListeners)) {
                    for (const eventListener of this.vEventListeners[event])
                        this.#domElem.addEventListener(event, eventListener);
                }
                for (const child of this.vChildren)
                    this.#domElem.append(child.getDomElem());
            }
        }
        return this.#domElem;
    }

    setDomElem(elem) {
        this.#domElem = elem;
    }
}

class VDom {
    static rootElem;

    static createElement(type) {
        return new VDomElement(type);
    }

    static createTextNode(text) {
        const elem = new VDomElement("vText");
        elem.vText = text;
        return elem;
    }

    static isConnected(elem) {
        if (elem === VDom.rootElem)
            return true;
        return elem.vParent && VDom.isConnected(elem.vParent);
    }

    static replace(newElem, oldElem) {
        if (newElem === oldElem) {
            throw new Error("Attempt to replace equal VDOM elements.");
        } else if (VDomElemComparer.vDomElementsAreIdentical(newElem, oldElem)) {
            VDom.#replaceInVDom(newElem, oldElem);
            VDom.#setDomElem_InNewFromOld_IncludingChildren_ExceptWhenChanged(newElem, oldElem, null);
        } else {
            const [newMinimalElem, oldMinimalElem, oldMinimalElemParent] = VDom.#findMinimalChangedElement(newElem, oldElem);
            VDom.#replaceInVDom(newElem, oldElem);
            VDom.#replaceInDom(newMinimalElem, oldMinimalElem, oldMinimalElemParent);
            VDom.#setDomElem_InNewFromOld_IncludingChildren_ExceptWhenChanged(newElem, oldElem, newMinimalElem);
        }
    }

    static #replaceInVDom(newElem, oldElem) {
        const parent = oldElem.vParent;
        parent.vChildren[parent.vChildren.indexOf(oldElem)] = newElem;
        newElem.vParent = parent;
        oldElem.vParent = null;
    }

    static #replaceInDom(newElem, oldElem, oldElemParent) {
        if (oldElem === null)
            oldElemParent.getDomElem().append(newElem.getDomElem());
        else if (newElem === null)
            oldElemParent.getDomElem().removeChild(oldElem.getDomElem());
        else
            oldElemParent.getDomElem().replaceChild(newElem.getDomElem(), oldElem.getDomElem());
    }

    static #setDomElem_InNewFromOld_IncludingChildren_ExceptWhenChanged(newElem, oldElem, changedElemNew) {
        if (newElem === changedElemNew)
            return;
        newElem.setDomElem(oldElem.getDomElem());
        for (let i = 0; i < Math.min(newElem.vChildren.length, oldElem.vChildren.length); i++)
            VDom.#setDomElem_InNewFromOld_IncludingChildren_ExceptWhenChanged(newElem.vChildren[i], oldElem.vChildren[i], changedElemNew);
    }

    static #findMinimalChangedElement(newElem, oldElem) {
        if (!VDomElemComparer.vDomElementsAreIdenticalWithoutChildren(newElem, oldElem)) // Difference in non-child props
            return [newElem, oldElem, oldElem.vParent];

        const childrenLengthDiff = newElem.vChildren.length - oldElem.vChildren.length;
        const changedChildrenIndexes = [];
        for (let i = 0; i < Math.min(newElem.vChildren.length, oldElem.vChildren.length); i++) {
            const oldChild = oldElem.vChildren[i];
            const newChild = newElem.vChildren[i];
            if (!VDomElemComparer.vDomElementsAreIdentical(newChild, oldChild))
                changedChildrenIndexes.push(i);
        }

        if (Math.abs(childrenLengthDiff) + changedChildrenIndexes.length === 0) // Identical non-child props and children
            throw new Error("Identical VDOM elements while searching for minimal changed.");
        if (Math.abs(childrenLengthDiff) + changedChildrenIndexes.length > 1) // Difference in more than one child
            return [newElem, oldElem, oldElem.vParent];

        if (childrenLengthDiff === 1) // Last child added
            return [newElem.vChildren.at(-1), null, oldElem];
        if (childrenLengthDiff === -1) // Last child removed
            return [null, oldElem.vChildren.at(-1), oldElem];

        const i = changedChildrenIndexes[0]; // Difference in exactly one child
        return VDom.#findMinimalChangedElement(newElem.vChildren[i], oldElem.vChildren[i]);
    }
}

class VDomElemComparer {
    static vDomElementsAreIdentical(elem1, elem2) {
        return Comparer.objectsAreIdentical(elem1, elem2, ["vParent"]);
    }

    static vDomElementsAreIdenticalWithoutChildren(elem1, elem2) {
        return Comparer.objectsAreIdentical(elem1, elem2, ["vParent", "vChildren"]);
    }
}
