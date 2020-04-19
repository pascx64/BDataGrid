declare var $: any;
namespace BDataGrid {
    export function focus(element: HTMLBaseElement, additionalSelector?: string) {

        if (additionalSelector) {
            $(element).find(additionalSelector)[0].focus();
        }
        else
            element.focus();
    }

    export function onkeypress(ev: KeyboardEvent) {

        ev.preventDefault();
        ev.stopImmediatePropagation();

        return false;
    }

    export function initializeDatagrid(datagridDotnet: any, element: HTMLBaseElement) {

        element.onkeydown = ev => {
            if (ev.keyCode >= 37 && ev.keyCode <= 40) { // arrows keys
                ev.preventDefault();
                ev.stopPropagation();

                datagridDotnet.invokeMethodAsync('OnKeyDownPressed', ev.keyCode)
                return false;
            }
            
            return true;
        };

        return true;
    }
}
