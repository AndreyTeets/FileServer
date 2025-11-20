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
        if (this.#lastRenderedElemIsAttached() && this.#lastRenderedPropsAndStateAreIdenticalToCurrent())
            return this.#lastRenderedElem;
        return this.#renderCoreAndSaveLastRenderInfo();
    }

    setState(state) {
        this.state = { ...this.state, ...state };
        if (this.#lastRenderedElemIsAttached() && !this.#lastRenderedPropsAndStateAreIdenticalToCurrent())
            this.#rerender();
    }

    #rerender() {
        const oldElem = this.#lastRenderedElem;
        const newElem = this.#renderCoreAndSaveLastRenderInfo();
        VDom.replace(newElem, oldElem);
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

    #lastRenderedElemIsAttached() {
        return this.#lastRenderedElem && VDom.isConnected(this.#lastRenderedElem);
    }
}
