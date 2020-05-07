var BDataGrid;
(function (BDataGrid) {
    var pageX, curCol, currentColIndex, curColWidth, alreadyBinded = false, currentDotNet;
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
            if ($(element).data('BDataGrid_EditionMode'))
                return;
            if (ev.keyCode == 9 || // tab
                (ev.keyCode >= 37 && ev.keyCode <= 40)) { // arrows keys
                ev.preventDefault();
                ev.stopPropagation();
                if (ev.keyCode == 9)
                    datagridDotnet.invokeMethodAsync('OnArrowKeysPressed', ev.shiftKey ? 37 : 39); // simulate 'left' and 'right' arrow key
                else
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
        var blockScrollEvent = false;
        footerTable.parentElement.onscroll = headerTable.parentElement.onscroll = bodyTable.parentElement.onscroll = function (sc) {
            if (blockScrollEvent)
                return;
            blockScrollEvent = true;
            footerTable.parentElement.scrollLeft = this.scrollLeft;
            headerTable.parentElement.scrollLeft = this.scrollLeft;
            bodyTable.parentElement.scrollLeft = this.scrollLeft;
            var pos = this.scrollLeft == 0 ? 0 : this.scrollLeft - 1; // used to hide imperfection on the left side of the col
            $(element).find('.fixedCol').css('left', pos + 'px');
            blockScrollEvent = false;
        };
        if (!alreadyBinded) {
            alreadyBinded = true;
            document.addEventListener('mousemove', function (e) {
                if (curCol) {
                    var diffX = e.pageX - pageX;
                    curCol.style.width = (curColWidth + diffX) + 'px';
                    curCol.style.minWidth = curCol.style.width;
                    curCol.style.maxWidth = curCol.style.width;
                }
            });
            document.addEventListener('mouseup', function (e) {
                if (curCol && currentDotNet && currentColIndex >= 0) {
                    currentDotNet.invokeMethodAsync('OnColResizedFromClient', currentColIndex, $(curCol).outerWidth() + "px");
                    curCol = undefined;
                    pageX = undefined;
                    curColWidth = undefined;
                    currentColIndex = undefined;
                    currentDotNet = undefined;
                    e.preventDefault();
                }
            });
        }
        resizableGrid(headerTable, datagridDotnet);
        return true;
    }
    BDataGrid.initializeDatagrid = initializeDatagrid;
    function switchEditionMode(element, mode) {
        $(element).data('BDataGrid_EditionMode', mode);
    }
    BDataGrid.switchEditionMode = switchEditionMode;
    function resizableGrid(table, dotnet) {
        var row = table.getElementsByTagName('tr')[0];
        var cols = row ? row.children : undefined;
        if (!cols)
            return;
        for (var i = 0; i < cols.length; i++) {
            var div = createDiv();
            cols[i].appendChild(div);
            cols[i].style.position = 'relative';
            setListeners(i, div, dotnet);
        }
    }
    function createDiv() {
        var div = document.createElement('div');
        div.style.top = 0;
        div.style.right = 0;
        div.style.width = '5px';
        div.style.position = 'absolute';
        div.style.cursor = 'col-resize';
        div.style.userSelect = 'none';
        /* table height */
        div.style.height = '100%';
        return div;
    }
    function setListeners(index, div, dotnet) {
        div.addEventListener('mousedown', function (e) {
            curCol = e.target.parentElement;
            pageX = e.pageX;
            curColWidth = curCol.offsetWidth;
            currentColIndex = index;
            currentDotNet = dotnet;
            e.preventDefault();
        });
        div.onclick = function (ev) {
            ev.stopImmediatePropagation();
        };
    }
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
    function editorError(elementSelector, errorMessage) {
        if ($(elementSelector).hasClass('BDataGridError'))
            return;
        $(elementSelector).addClass('BDataGridError');
        setTimeout(function () {
            $(elementSelector).removeClass('BDataGridError');
        }, 1000);
        new Noty({
            layout: 'topCenter',
            theme: 'mint',
            timeout: 3000,
            progressBar: true,
            text: errorMessage,
            type: 'error',
            animation: {
                open: 'noty_effects_open',
                close: 'noty_effects_close'
            },
        }).show();
    }
    BDataGrid.editorError = editorError;
})(BDataGrid || (BDataGrid = {}));
//# sourceMappingURL=BDataGrid.js.map