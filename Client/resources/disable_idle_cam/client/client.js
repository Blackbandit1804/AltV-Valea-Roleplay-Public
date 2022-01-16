import alt from 'alt-client';
import native from 'natives';

let idle = alt.setInterval(() => {
    native.invalidateIdleCam(); 
    native.invalidateVehicleIdleCam();
}, 20000); 