import * as alt from 'alt';
import * as game from 'natives';

export let storageBrowser = null;

let isLaborCEFOpened = false;
let isStorageCEFOpened = false;
let isDynasty8CEFOpened = false;

alt.onServer('Client:HUD:CreateCEF', () => {
    if (storageBrowser == null) {
        storageBrowser = new alt.WebView("http://resource/client/cef/storage/index.html");
        storageBrowser.focus();

        // Storage
        storageBrowser.on("Client:Storage:switchItemToStorage", (storageType, identifierId, name, amount) => {
            alt.emitServer("Server:Storage:switchItemToStorage", parseInt(identifierId), name, parseInt(amount));
        });

        storageBrowser.on("Client:Storage:switchItemToInventory", (storageType, identifierId, name, amount) => {
                alt.emitServer("Server:Storage:switchItemToInventory", parseInt(identifierId), name, parseInt(amount));
        });

        storageBrowser.on("Client:Storage:destroy", () => {
            isStorageCEFOpened = false;
            alt.showCursor(false);
            game.freezeEntityPosition(alt.Player.local.scriptID, false);
            alt.toggleGameControls(true);
            storageBrowser.unfocus();
            alt.emitServer("Server:CEF:setCefStatus", false);
        });

        //Dynasty8
        storageBrowser.on("Client:Utilities:locatePos", (x, y) => {
            game.setNewWaypoint(x, y);
        });

        storageBrowser.on("Client:Dynasty:buyStorage", (storageId) => {
            alt.emitServer("Server:Dynasty:buyStorage", parseInt(storageId));
        });

        storageBrowser.on("Client:Dynasty:sellStorage", (storageId) => {
            alt.emitServer("Server:Dynasty:sellStorage", parseInt(storageId));
        });

        storageBrowser.on("Client:Dynasty8:destroy", () => {
            isDynasty8CEFOpened = false;
            alt.showCursor(false);
            game.freezeEntityPosition(alt.Player.local.scriptID, false);
            alt.toggleGameControls(true);
            storageBrowser.unfocus();
            alt.emitServer("Server:CEF:setCefStatus", false);
        });
    }
});

alt.onServer("Client:Dynasty8:create", (type, myItems, freeItems) => {
    if (storageBrowser == null || isDynasty8CEFOpened) return;
    isDynasty8CEFOpened = true;
    storageBrowser.emit("CEF:Dynasty8:openDynasty8HUD", type, myItems, freeItems);
    alt.showCursor(true);
    alt.toggleGameControls(false);
    storageBrowser.focus();
    alt.emitServer("Server:CEF:setCefStatus", true);
});

alt.onServer("Client:Storage:openStorage", (type, id, invItems, storageItems) => {
    if (storageBrowser == null || isStorageCEFOpened) return;
    isLaborCEFOpened = true;
    storageBrowser.emit("CEF:Storage:openStorage", type, id, invItems, storageItems);
    alt.showCursor(true);
    alt.toggleGameControls(false);
    storageBrowser.focus();
    alt.emitServer("Server:CEF:setCefStatus", true);
});

