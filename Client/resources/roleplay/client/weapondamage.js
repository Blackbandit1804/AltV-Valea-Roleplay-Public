import * as alt from 'alt-client';
import * as native from 'natives';

alt.everyTick(() => {
    native.setPedSuffersCriticalHits(alt.Player.local.scriptID, false); // disable headshot
    if (native.isPedArmed(alt.Player.local.scriptID, 6)) {
        native.disableControlAction(0, 140, true);
        native.disableControlAction(0, 141, true);
        native.disableControlAction(0, 142, true);
    }
    //bullpup mk2
    native.setWeaponDamageModifierThisFrame(0x84D6FAFD, 0.3);
    // Bullpup
    native.setWeaponDamageModifierThisFrame(0x7F229F94, 0.25);
    //pump action
    native.setWeaponDamageModifierThisFrame(0x1D073A89, 0.3);
    //assault
    native.setWeaponDamageModifierThisFrame(0xBFEFFF6D, 0.25);
    //carbine
    native.setWeaponDamageModifierThisFrame(0x83BF0278, 0.2);
    //advanced
    native.setWeaponDamageModifierThisFrame(0xAF113F99, 0.3);
    //gusenberg
    native.setWeaponDamageModifierThisFrame(0x61012683, 0.25);
    //micro mp
    native.setWeaponDamageModifierThisFrame(0x13532244, 0.25);
    //mini mp
    native.setWeaponDamageModifierThisFrame(0xBD248B55, 0.25);
    //mp
    native.setWeaponDamageModifierThisFrame(0x2BE6766B, 0.3);
    //mp mk2
    native.setWeaponDamageModifierThisFrame(0x78A97CD0, 0.3);
    //combat pdw
    native.setWeaponDamageModifierThisFrame(0xA3D4D34, 0.3);
    //specialcarbine
    native.setWeaponDamageModifierThisFrame(0xC0A3098D, 0.25);
    //pistol
    native.setWeaponDamageModifierThisFrame(0x1B06D571, 0.3);
    //pistol50
    native.setWeaponDamageModifierThisFrame(0x99AEEB3B, 0.35);
    //combat pistol
    native.setWeaponDamageModifierThisFrame(0x5EF9FEC4, 0.3);
    //Marksman Rifle
    native.setWeaponDamageModifierThisFrame(0xC734385A, 0.25);
    //Sniper Rifle
    native.setWeaponDamageModifierThisFrame(0x05FC3C11, 0.35);
});