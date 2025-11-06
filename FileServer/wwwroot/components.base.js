class ComponentBase {
    props;
    state;
    #lastRenderedProps;
    #lastRenderedState;
    #lastRenderedElem;

    constructor() {
        this.props = {};
        this.state = {};
    }

    render(props) {
        this.props = props || {};
        if (this.#lastRenderedElemIsAttachedToDom() && this.#lastRenderedPropsAndStateAreIdenticalToCurrent())
            return this.#lastRenderedElem;
        return this.#renderCoreAndSaveLastRenderInfo();
    }

    setState(state) {
        this.state = { ...this.state, ...state };
        if (this.#lastRenderedElemIsAttachedToDom() && !this.#lastRenderedPropsAndStateAreIdenticalToCurrent())
            this.#rerender();
    }

    #rerender() {
        const oldElem = this.#lastRenderedElem;
        const newElem = this.#renderCoreAndSaveLastRenderInfo();
        VDom.replaceAndRerender(newElem, oldElem);
    }

    #renderCoreAndSaveLastRenderInfo() {
        const elem = this.renderCore();
        this.#lastRenderedProps = this.props;
        this.#lastRenderedState = this.state;
        this.#lastRenderedElem = elem;
        return elem;
    }

    #lastRenderedPropsAndStateAreIdenticalToCurrent() {
        if (this.#lastRenderedProps && this.#lastRenderedState
            && Comparer.objectsAreIdentical(this.#lastRenderedProps, this.props)
            && Comparer.objectsAreIdentical(this.#lastRenderedState, this.state))
            return true;
        return false;
    }

    #lastRenderedElemIsAttachedToDom() {
        return this.#lastRenderedElem && VDom.isConnected(this.#lastRenderedElem);
    }
}
