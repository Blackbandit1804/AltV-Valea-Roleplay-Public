import * as alt from 'alt';
import * as game from 'natives';

export let laborBrowser = null;
let isLaborCEFOpened = false;
// let laborBrowser = null;

alt.onServer('Client:HUD:CreateCEF', () => {
    if (laborBrowser == null) {
        laborBrowser = new alt.WebView("http://resource/client/cef/labor/index.html");
        laborBrowser.focus();

        laborBrowser.on("Client:Labor:switchItemToInventory", (name, amount) => {
            alt.emitServer("Server:Labor:switchItemToInventory", name, parseInt(amount));
        });

        laborBrowser.on("Client:Labor:switchItemToLabor", (name, amount) => {
            alt.emitServer("Server:Labor:switchItemToLabor", name, parseInt(amount));
        });

        laborBrowser.on("Client:Labor:destroy", () => {
            isLaborCEFOpened = false;
            alt.showCursor(false);
            game.freezeEntityPosition(alt.Player.local.scriptID, false);
            alt.toggleGameControls(true);
            laborBrowser.unfocus();
            alt.emitServer("Server:CEF:setCefStatus", false);
        });
    }
});

alt.onServer("Client:Labor:openLabor", (invItems, laborItems) => {
    if (laborBrowser == null || isLaborCEFOpened) return;
    isLaborCEFOpened = true;
    laborBrowser.emit("CEF:Labor:openLabor", invItems, laborItems);
    alt.showCursor(true);
    alt.toggleGameControls(false);
    laborBrowser.focus();
    alt.emitServer("Server:CEF:setCefStatus", true);
});
