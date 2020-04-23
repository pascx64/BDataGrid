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
        var headerTable = $(element).find('table')[0];
        var bodyTable = $(element).find('table')[1];
        var footerTable = $(element).find('table')[2];
        var scrollBar = $(element).find('.scrollBarDiv')[0];
        bodyTable.onkeydown = function (ev) {
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
        var interval = setInterval(function () {
            if ($(element)[0])
                $(scrollBar).css('width', $(headerTable).width());
            else
                clearInterval(interval);
        }, 500);
        footerTable.parentElement.onscroll = function (sc) {
            headerTable.parentElement.scrollLeft = this.scrollLeft;
            bodyTable.parentElement.scrollLeft = this.scrollLeft;
        };
        return true;
    }
    BDataGrid.initializeDatagrid = initializeDatagrid;
    function saveAsFile(filename, bytesBase64) {
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
    BDataGrid.saveAsFile = saveAsFile;
})(BDataGrid || (BDataGrid = {}));
//# sourceMappingURL=BDataGrid.js.map