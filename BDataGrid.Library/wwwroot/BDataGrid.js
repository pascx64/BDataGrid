var BDataGrid;
(function (BDataGrid) {
    function focus(element, additionalSelector) {
        if (additionalSelector) {
            $(element).find(additionalSelector)[0].focus();
        }
        else
            element.focus();
    }
    BDataGrid.focus = focus;
    function onkeypress(ev) {
        ev.preventDefault();
        ev.stopImmediatePropagation();
        return false;
    }
    BDataGrid.onkeypress = onkeypress;
    function initializeDatagrid(datagridDotnet, element) {
        element.onkeydown = function (ev) {
            if (ev.keyCode >= 37 && ev.keyCode <= 40) { // arrows keys
                ev.preventDefault();
                ev.stopPropagation();
                datagridDotnet.invokeMethodAsync('OnKeyDownPressed', ev.keyCode);
                return false;
            }
            return true;
        };
        return true;
    }
    BDataGrid.initializeDatagrid = initializeDatagrid;
})(BDataGrid || (BDataGrid = {}));
//# sourceMappingURL=BDataGrid.js.map