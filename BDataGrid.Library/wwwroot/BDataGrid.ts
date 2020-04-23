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

        var headerTable = $(element).find('table')[0] as HTMLBaseElement;
        var bodyTable = $(element).find('table')[1] as HTMLBaseElement;
        var footerTable = $(element).find('table')[2] as HTMLBaseElement;
        var scrollBar = $(element).find('.scrollBarDiv')[0] as HTMLBaseElement;
        bodyTable.onkeydown = ev => {
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

        let interval = setInterval(function () {
            if ($(element)[0])
                $(scrollBar).css('width', $(headerTable).width());
            else
                clearInterval(interval);
        }, 500);

        footerTable.parentElement.onscroll = function (sc) {
            headerTable.parentElement.scrollLeft = (this as any).scrollLeft;
            bodyTable.parentElement.scrollLeft = (this as any).scrollLeft;
        }

        return true;
    }

    export function saveAsFile(filename, bytesBase64) {
        if (navigator.msSaveBlob) {
            //Download document in Edge browser
            var data = window.atob(bytesBase64);
            var bytes = new Uint8Array(data.length);
            for (var i = 0; i < data.length; i++) {
                bytes[i] = data.charCodeAt(i);
            }
            var blob = new Blob([bytes.buffer], { type: "application/octet-stream" });
            navigator.msSaveBlob(blob, filename);
        }
        else {
            var link = document.createElement('a');
            link.download = filename;
            link.href = "data:application/octet-stream;base64," + bytesBase64;
            document.body.appendChild(link); // Needed for Firefox
            link.click();
            document.body.removeChild(link);
        }
    }
}
