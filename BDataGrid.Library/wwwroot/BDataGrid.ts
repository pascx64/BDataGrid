﻿declare var $: any;
namespace BDataGrid {
    var pageX, curCol, currentColIndex, curColWidth, alreadyBinded = false, currentDotNet: any;

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
                if (curCol && currentDotNet && currentColIndex) {
                    datagridDotnet.invokeMethodAsync('OnColResizedFromClient', currentColIndex, curCol.style.width);

                    curCol = undefined;
                    pageX = undefined;
                    curColWidth = undefined;
                    currentColIndex = undefined;
                    currentDotNet = undefined;
                }
            });
        }
        resizableGrid(headerTable as any, datagridDotnet);


        return true;
    }

    function resizableGrid(table: HTMLTableElement, dotnet: any) {
        var row = table.getElementsByTagName('tr')[0];
        var cols = row ? row.children : undefined;

        if (!cols)
            return;

        for (var i = 0; i < cols.length; i++) {
            var div = createDiv(table.offsetHeight);
            cols[i].appendChild(div);
            (cols[i] as any).style.position = 'relative';
            setListeners(i, div, dotnet);
        }
    }

    function createDiv(height) {
        var div = document.createElement('div') as any;
        div.style.top = 0;
        div.style.right = 0;
        div.style.width = '5px';
        div.style.position = 'absolute';
        div.style.cursor = 'col-resize';
        div.style.userSelect = 'none';
        /* table height */
        div.style.height = height + 'px';
        return div;
    }
    function setListeners(index, div: HTMLDivElement, dotnet) {
        div.addEventListener('mousedown', function (e) {
            curCol = (e.target as any).parentElement;
            pageX = e.pageX;
            curColWidth = curCol.offsetWidth
            currentColIndex = index;
            currentDotNet = dotnet;

            e.preventDefault();
        });
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
