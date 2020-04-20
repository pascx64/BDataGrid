declare var $: any;
namespace BDataGrid {
    export function focus(element: HTMLBaseElement, additionalSelector?: string) {

        if (additionalSelector) {
            $(element).find(additionalSelector)[0].focus();
        }
        else
            element.focus();
    }

    export function initializeDatagrid(datagridDotnet: any, element: HTMLBaseElement) {

        element.onkeydown = ev => {
            if (ev.keyCode >= 37 && ev.keyCode <= 40) { // arrows keys
                ev.preventDefault();
                ev.stopPropagation();

                datagridDotnet.invokeMethodAsync('OnArrowKeysPressed', ev.keyCode)
                return false;
            }

            let key = ev.key;
            let keyCode = ev.keyCode
            let isEnter = keyCode == 13;

            var regExp = /^[A-Za-z0-9]+$/;
            let isAlphaNumeric = (!!key.match(regExp) && key.length == 1) ||
                (['-', '+', '*', '/', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '=', '\\', 'é', 'è', 'à', 'É', 'È', 'À', '.'].indexOf(key) != -1);

            if (isAlphaNumeric || isEnter) {

                datagridDotnet.invokeMethodAsync('OnKeyDown', isEnter ? "" : key);

            }

            return true;
        };

        return true;
    }
}
