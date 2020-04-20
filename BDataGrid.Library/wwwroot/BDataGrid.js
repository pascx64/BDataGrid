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
    function initializeDatagrid(datagridDotnet, element) {
        element.onkeydown = function (ev) {
            if (ev.keyCode >= 37 && ev.keyCode <= 40) { // arrows keys
                ev.preventDefault();
                ev.stopPropagation();
                datagridDotnet.invokeMethodAsync('OnArrowKeysPressed', ev.keyCode);
                return false;
            }
            var key = ev.key;
            var keyCode = ev.keyCode;
            var isEnter = keyCode == 13;
            var regExp = /^[A-Za-z0-9]+$/;
            var isAlphaNumeric = (!!key.match(regExp) && key.length == 1) ||
                (['-', '+', '*', '/', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '=', '\\', 'é', 'è', 'à', 'É', 'È', 'À', '.'].indexOf(key) != -1);
            if (isAlphaNumeric || isEnter) {
                datagridDotnet.invokeMethodAsync('OnKeyDown', isEnter ? "" : key);
            }
            return true;
        };
        return true;
    }
    BDataGrid.initializeDatagrid = initializeDatagrid;
})(BDataGrid || (BDataGrid = {}));
//# sourceMappingURL=BDataGrid.js.map