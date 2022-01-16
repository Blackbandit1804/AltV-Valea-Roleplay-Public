import * as alt from 'alt';
import * as game from 'natives';
import {hudBrowser} from "./hud";

var canUseEKey = true;
var lastInteract = 0;
let toggleCrouch = false;
let autoFarm = false;

function canInteract() { return lastInteract + 1000 < Date.now() }

alt.setInterval(() => {
    if (!autoFarm) return;
    alt.log(`Toggled: ${autoFarm}`)
    alt.emitServer('Server:Farm:AutoCollect');
}, 1000);

alt.on('keyup', (key) => {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (key === 69) {
        alt.emitServer("Server:KeyHandler:PressE");
    } else if (key == 'U'.charCodeAt(0)) {
        alt.emitServer("Server:KeyHandler:PressU");
        if (!alt.gameControlsEnabled()) return;
    }  else if (key === 188) {
        alt.emitServer("Server:KeyHandler:PressComma");
    }  else if (key === 190) {
        alt.emitServer("Server:KeyHandler:PressPeriod");
    } else if (key === 113) /*F2*/ {
        autoFarm ^= 1;
        hudBrowser.emit("CEF:HUD:sendNotification", (autoFarm ? 2 : 4), 3000, `Autofarming ist nun ${autoFarm ? 'aktviert' : 'deaktiviert'}`);
    }
});

alt.on('keydown', (key) => {
    if (!canInteract) return;
    lastInteract = Date.now();
    if (key === 17) { //STRG
        game.disableControlAction(0, 36, true);
        if (!game.isPlayerDead(alt.Player.local) && !game.isPedSittingInAnyVehicle(alt.Player.local.scriptID)) {
            if (!game.isPauseMenuActive()) {
                
                game.requestAnimSet("move_ped_crouched");
                if (!toggleCrouch) {
                    game.setPedMovementClipset(alt.Player.local.scriptID, "move_ped_crouched", 0.45);
                    toggleCrouch = true;
                } else {
                    game.clearPedTasks(alt.Player.local.scriptID);
                    game.resetPedMovementClipset(alt.Player.local.scriptID, 0.45);
                    toggleCrouch = false;
                }
            }
        }
    } 
});

alt.onServer("Client:DoorManager:ManageDoor", (hash, pos, isLocked) => {
    if (hash != undefined && pos != undefined && isLocked != undefined) {
        // game.doorControl(game.getHashKey(hash), pos.x, pos.y, pos.z, isLocked, 0.0, 50.0, 0.0); //isLocked (0) = Open | isLocked (1) = True
        game.setStateOfClosestDoorOfType(game.getHashKey(hash), pos.x, pos.y, pos.z, isLocked, 0, 0);
    }
});