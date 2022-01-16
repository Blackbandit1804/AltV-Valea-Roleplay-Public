import * as alt from 'alt';
import * as native from "natives";
import * as game from 'natives';
import { Player, Vector3} from "alt";
import { closeInventoryCEF } from './inventory.js';
import { closeTabletCEF } from './tablet.js';
import { Raycast, GetDirectionFromRotation, setClothes, setTattoo, clearTattoos, setCorrectTattoos } from './utilities.js';

export let hudBrowser = null;
export let browserReady = false;
let deathScreen = null;
let identityCardApplyCEFopened = false;
let BankAccountManageFormOpened = false;
let ATMcefOpened = false;
let ShopCefOpened = false;
let BarberCefOpened = false;
let GarageCefOpened = false;
let VehicleShopCefOpened = false;
let JobcenterCefOpened = false;
let FuelStationCefOpened = false;
let ClothesShopCefOpened = false;
let bankFactionATMCefOpened = false;
let GivePlayerBillCefOpened = false;
let FactionStorageCefOpened = false;
let RecievePlayerBillCefOpened = false;
let VehicleTrunkCefOpened = false;
let VehicleLicensingCefOpened = false;
let PlayerSearchInventoryCefOpened = false;
let GivePlayerLicenseCefOpened = false;
let MinijobPilotCefOpened = false;
let MinijobBusdriverCefOpened = false;
let HotelRentCefOpened = false;
let HouseEntranceCefOpened = false;
let DeathscreenCefOpened = false;
let HouseManageCefOpened = false;
let TownhallHouseSelectorCefOpened = false;
let AnimationMenuCefOpened = false;
let ClothesRadialCefOpened = false;
let TuningMenuCefOpened = false;
let ClothesStorageCefOpened = false;
let curSpeed = 0;
let curKm = 0;
let curTuningVeh = null;
let isPhoneEquipped = false;
let isPlayerDead = false;
let currentRadioFrequence = null;
let isTattooShopOpened = false;
let isPlayerUsingMegaphone = false;
let changeVehOwnerCefOpened = false;
let isLaptopActivated = false;
let isLaptopCEFOpened = false;
let playerSeat  = null;
let playerVehicle = null;
let hasHandsUp = false;
let curVehTrunkId = -1;

alt.onServer("Client:HUD:CreateCEF", (armor, health, hunger, thirst, currentmoney, currentmoneyblack) => {
    if (hudBrowser == null) {
        hudBrowser = new alt.WebView("http://resource/client/cef/hud/index.html");

        hudBrowser.on("Client:HUD:cefIsReady", () => {
            alt.setTimeout(function() {
                hudBrowser.emit("CEF:HUD:updateDesireHUD", armor, health, hunger, thirst);
                hudBrowser.emit("CEF:HUD:updateMoney", currentmoney);
                hudBrowser.emit("CEF:HUD:updateMoneyBlack", currentmoneyblack);
                browserReady = true;
            }, 1000);
        });

        hudBrowser.on("Client:Farming:StartProcessing", (neededItem, producedItem, neededItemAmount, producedItemAmount, duration, neededItemTWO, neededItemTHREE, neededItemTWOAmount, neededItemTHREEAmount) => {
            if (neededItemTHREE == null)  {
                alt.emitServer("Server:Farming:StartProcessing", neededItem.itemName, producedItem.itemName, neededItemAmount, producedItemAmount, duration, "none", "none", neededItemTWOAmount, 0);
            } else if (neededItemTWO == null) {
                alt.emitServer("Server:Farming:StartProcessing", neededItem.itemName, producedItem.itemName, neededItemAmount, producedItemAmount, duration, "none", "none", 0, 0);
            } else if (neededItem != null && neededItemTWO != null && neededItemTHREE != null) {
                alt.emitServer("Server:Farming:StartProcessing", neededItem.itemName, producedItem.itemName, neededItemAmount, producedItemAmount, duration, neededItemTWO.itemName, neededItemTHREE.itemName, neededItemTWOAmount, neededItemTHREEAmount);
            }
            
        });

        hudBrowser.on("Client:Farming:closeCEF", () => {
            closeFarmingCEF();
        });

        alt.onServer("Client:Farming:createCEF", (neededItem, producedItem, neededItemAmount, producedItemAmount, duration, neededItemTWO, neededItemTHREE, neededItemTWOAmount, neededItemTHREEAmount) => {
            if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && ShopCefOpened == false) {
                if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
                hudBrowser.emit("CEF:Farming:createCEF", JSON.parse(neededItem)[0], JSON.parse(producedItem)[0], neededItemAmount, producedItemAmount, duration, JSON.parse(neededItemTWO)[0], JSON.parse(neededItemTHREE)[0], neededItemTWOAmount, neededItemTHREEAmount);
                alt.emitServer("Server:CEF:setCefStatus", true);
                alt.showCursor(true);
                alt.toggleGameControls(false);
                hudBrowser.focus();
            }
        });


        hudBrowser.on("Client:Vehicle:changeVehOwner", (targetId) => {
            alt.emitServer("Server:Vehicle:changeVehOwner", parseInt(targetId));
        });
        
        hudBrowser.on("Client:Vehicle:closeChangeVehOwnerHUD", () => {
            alt.emitServer("Server:CEF:setCefStatus", false);
            game.freezeEntityPosition(alt.Player.local.scriptID, false);
            alt.showCursor(false);
            alt.toggleGameControls(true);
            hudBrowser.unfocus();
            changeVehOwnerCefOpened = false;
        });
        //Tattoo Shop
        hudBrowser.on("Client:TattooShop:closeShop", () => {
            isTattooShopOpened = false;
            alt.showCursor(false);
            alt.toggleGameControls(true);
            hudBrowser.unfocus();
            alt.emitServer("Server:CEF:setCefStatus", false);
            alt.emitServer("Server:ClothesShop:RequestCurrentSkin");
            clearTattoos(alt.Player.local.scriptID);
            setCorrectTattoos();
        });

        hudBrowser.on("Client:TattooShop:buyTattoo", (shopId, tattooId) => {
            alt.emitServer("Server:TattooShop:buyTattoo", parseInt(shopId), parseInt(tattooId));
        });

        hudBrowser.on("Client:TattooShop:deleteTattoo", (id) => {
            alt.emitServer("Server:TattooShop:deleteTattoo", parseInt(id));
        });

        hudBrowser.on("Client:TattooShop:previewTattoo", (hash, collection) => {
            clearTattoos(alt.Player.local.scriptID);
            setTattoo(alt.Player.local.scriptID, collection, hash);
        });

        //Rotation HUD
        hudBrowser.on("Client:Utilities:setRotation", (rotZ) => {
            game.setEntityRotation(alt.Player.local.scriptID, game.getEntityPitch(alt.Player.local.scriptID), game.getEntityRoll(alt.Player.local.scriptID), parseFloat(rotZ), 2, true);
        });

        hudBrowser.on("Client:HUD:sendIdentityCardApplyForm", (birthplace) => {
            alt.emitServer("Server:HUD:sendIdentityCardApplyForm", birthplace);
            alt.emitServer("Server:CEF:setCefStatus", false);
            game.freezeEntityPosition(game.playerPedId(), false);
            alt.showCursor(false);
            alt.toggleGameControls(true);
            hudBrowser.unfocus();
            identityCardApplyCEFopened = false;
        });

        hudBrowser.on("Client:Bank:BankAccountdestroyCEF", () => {
            closeBankCEF();
        });

        hudBrowser.on("Client:Bank:BankAccountCreateNewAccount", (selectedBank) => {
            alt.emitServer("Server:Bank:CreateNewBankAccount", selectedBank);
            closeBankCEF();
        });

        hudBrowser.on("Client:Bank:BankAccountAction", (action, accountNumber) => {
            alt.emitServer("Server:Bank:BankAccountAction", action, accountNumber);
            closeBankCEF();
        });

        hudBrowser.on("Client:ATM:requestBankData", (accountNr) => {
            alt.emitServer("Server:ATM:requestBankData", accountNr);
        });

        hudBrowser.on("Client:ATM:WithdrawMoney", (accountNr, amount, zoneName) => {
            alt.emitServer("Server:ATM:WithdrawMoney", accountNr, parseInt(amount), zoneName);
        });

        hudBrowser.on("Client:ATM:DepositMoney", (accountNr, amount, zoneName) => {
            alt.emitServer("Server:ATM:DepositMoney", accountNr, parseInt(amount), zoneName);
        });

        hudBrowser.on("Client:ATM:TransferMoney", (accountNr, targetNr, amount, zoneName) => {
            alt.emitServer("Server:ATM:TransferMoney", accountNr, parseInt(targetNr), parseInt(amount), zoneName);
        });

        hudBrowser.on("Client:ATM:TryPin", (action, curATMAccountNumber) => {
            alt.emitServer("Server:ATM:TryPin", action, curATMAccountNumber);
        });

        hudBrowser.on("Client:ATM:BankATMdestroyCEF", () => {
            closeATMCEF();
        });

        hudBrowser.on("Client:Shop:buyItem", (shopId, amount, itemname) => {
            alt.emitServer("Server:Shop:buyItem", parseInt(shopId), parseInt(amount), itemname);
        });

        hudBrowser.on("Client:Shop:sellItem", (shopId, amount, itemname) => {
            alt.emitServer("Server:Shop:sellItem", parseInt(shopId), parseInt(amount), itemname);
        });

        hudBrowser.on("Client:Barber:UpdateHeadOverlays", (headoverlayarray) => {
            let headoverlays = JSON.parse(headoverlayarray);
            game.setPedHeadOverlayColor(alt.Player.local.scriptID, 1, 1, parseInt(headoverlays[2][1]), 1);
            game.setPedHeadOverlayColor(alt.Player.local.scriptID, 2, 1, parseInt(headoverlays[2][2]), 1);
            game.setPedHeadOverlayColor(alt.Player.local.scriptID, 5, 2, parseInt(headoverlays[2][5]), 1);
            game.setPedHeadOverlayColor(alt.Player.local.scriptID, 8, 2, parseInt(headoverlays[2][8]), 1);
            game.setPedHeadOverlayColor(alt.Player.local.scriptID, 10, 1, parseInt(headoverlays[2][10]), 1);
            game.setPedEyeColor(alt.Player.local.scriptID, parseInt(headoverlays[0][14]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 0, parseInt(headoverlays[0][0]), parseInt(headoverlays[1][0]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 1, parseInt(headoverlays[0][1]), parseFloat(headoverlays[1][1]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 2, parseInt(headoverlays[0][2]), parseFloat(headoverlays[1][2]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 3, parseInt(headoverlays[0][3]), parseInt(headoverlays[1][3]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 4, parseInt(headoverlays[0][4]), parseInt(headoverlays[1][4]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 5, parseInt(headoverlays[0][5]), parseInt(headoverlays[1][5]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 6, parseInt(headoverlays[0][6]), parseInt(headoverlays[1][6]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 7, parseInt(headoverlays[0][7]), parseInt(headoverlays[1][7]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 8, parseInt(headoverlays[0][8]), parseInt(headoverlays[1][8]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 9, parseInt(headoverlays[0][9]), parseInt(headoverlays[1][9]));
            game.setPedHeadOverlay(alt.Player.local.scriptID, 10, parseInt(headoverlays[0][10]), parseInt(headoverlays[1][10]));
            game.setPedComponentVariation(alt.Player.local.scriptID, 2, parseInt(headoverlays[0][13]), 0, 0);
            game.setPedHairColor(alt.Player.local.scriptID, parseInt(headoverlays[2][13]), parseInt(headoverlays[1][13]));
        });

        hudBrowser.on("Client:Barber:finishBarber", (headoverlayarray) => {
            alt.emitServer("Server:Barber:finishBarber", headoverlayarray);
            closeBarberCEF();
        });

        hudBrowser.on("Client:Barber:RequestCurrentSkin", () => {
            alt.emitServer("Server:Barber:RequestCurrentSkin");
        });

        hudBrowser.on("Client:Barber:destroyBarberCEF", () => {
            closeBarberCEF();
        });

        hudBrowser.on("Client:Shop:destroyShopCEF", () => {
            closeShopCEF();
        });

        hudBrowser.on("Client:Garage:destroyGarageCEF", () => {
            closeGarageCEF();
        });

        hudBrowser.on("Client:Smartphone:joinRadioFrequence", (currentRadioFrequence) => {
            alt.emitServer("Server:Smartphone:joinRadioFrequence", `${currentRadioFrequence}`);
        });

        hudBrowser.on("Client:Smartphone:leaveRadioFrequence", () => {
            if (currentRadioFrequence == null) return;
            alt.emitServer("Server:Smartphone:leaveRadioFrequence");
        });

        hudBrowser.on("Client:VehicleShop:destroyVehicleShopCEF", () => {
            closeVehicleShopCEF();
        });

        hudBrowser.on("Client:VehicleShop:BuyVehicle", (shopId, hash) => {
            alt.emitServer("Server:VehicleShop:BuyVehicle", parseInt(shopId), hash);
        });

        hudBrowser.on("Client:Garage:DoAction", (garageid, action, vehid) => {
            alt.emitServer("Server:Garage:DoAction", parseInt(garageid), action, parseInt(vehid));
        });

        hudBrowser.on("Client:Jobcenter:SelectJob", (jobName) => {
            alt.emitServer("Server:Jobcenter:SelectJob", jobName);
            closeJobcenterCEF();
        });

        hudBrowser.on("Client:Jobcenter:destroyCEF", () => {
            closeJobcenterCEF();
        });

        hudBrowser.on("Client:FuelStation:FuelVehicleAction", (vehID, fuelStationId, fueltype, selectedLiterAmount, selectedLiterPrice) => {
            alt.emitServer("Server:FuelStation:FuelVehicleAction", parseInt(vehID), parseInt(fuelStationId), fueltype, parseInt(selectedLiterAmount), parseInt(selectedLiterPrice));
        });

        hudBrowser.on("Client:FuelStation:destroyCEF", () => {
            closeFuelstationCEF();
        });

        hudBrowser.on("Client:ClothesShop:setClothes", (componentId, drawableId, textureId) => {
            game.setPedComponentVariation(alt.Player.local.scriptID, parseInt(componentId), parseInt(drawableId), parseInt(textureId), 0);
        });

        hudBrowser.on("Client:ClothesShop:setAccessory", (componentId, drawableId, textureId) => {
            game.setPedPropIndex(alt.Player.local.scriptID, componentId, drawableId, textureId, false);
        });

        hudBrowser.on("Client:ClothesShop:RequestCurrentSkin", () => {
            alt.emitServer("Server:ClothesShop:RequestCurrentSkin");
        });

        hudBrowser.on("Client:ClothesShop:destroyCEF", () => {
            closeClothesShopCEF();
        });

        hudBrowser.on("Client:ClothesStorage:destroyCEF", () => {
            closeClothesStorageCEF();
        });

        hudBrowser.on("Client:ClothesShop:buyItem", (shopId, itemName) => {
            alt.emitServer("Server:ClothesShop:buyItem", parseInt(shopId), parseInt(1), itemName);
        });

        hudBrowser.on("Client:InteractionMenu:giveRequestedAction", (type, action) => {
            InterActionMenuDoAction(type, action);
        });

        hudBrowser.on("Client:AnimationMenu:giveRequestedAction", (action) => {
            InterActionMenuDoActionAnimationMenu(action);
        });

        hudBrowser.on("Client:AnimationMenuPage2:giveRequestedAction", (action) => {
            InterActionMenuDoActionAnimationMenuPage2(action);
        });

        hudBrowser.on("Client:AnimationMenuPage3:giveRequestedAction", (action) => {
            InterActionMenuDoActionAnimationMenuPage3(action);
        });

        hudBrowser.on("Client:ClothesRadial:giveRequestedAction", (action) => {
            InterActionMenuDoActionClothesRadialMenu(action);
        });

        hudBrowser.on("Client:FactionBank:destroyCEF", () => {
            closeBankFactionATMCEF();
        });

        hudBrowser.on("Client:FactionBank:DepositMoney", (type, factionId, amount) => {
            alt.emitServer("Server:FactionBank:DepositMoney", type, parseInt(factionId), parseInt(amount));
        });

        hudBrowser.on("Client:FactionBank:WithdrawMoney", (type, factionId, amount) => {
            alt.emitServer("Server:FactionBank:WithdrawMoney", type, parseInt(factionId), parseInt(amount));
        });

        hudBrowser.on("Client:GivePlayerBill:giveBill", (type, targetCharId, reason, moneyamount) => {
            alt.emitServer("Server:PlayerBill:giveBill", type, reason, parseInt(targetCharId), parseInt(moneyamount));
        });

        hudBrowser.on("Client:GivePlayerBill:destroyCEF", () => {
            closeGivePlayerBillCEF();
        });

        hudBrowser.on("Client:PlayerBill:BillAction", (action, billType, factionCompanyId, moneyAmount, reason, charId) => {
            alt.emitServer("Server:PlayerBill:BillAction", action, billType, parseInt(factionCompanyId), parseInt(moneyAmount), reason, parseInt(charId));
        });

        hudBrowser.on("Client:RecievePlayerBill:destroyCEF", () => {
            closeRecievePlayerBillCEF();
        });

        hudBrowser.on("Client:FactionStorage:destroyCEF", () => {
            closeFactionStorageCEF();
        });

        hudBrowser.on("Client:FactionStorage:FactionStorageAction", (action, factionId, charId, type, itemName, amount, fromContainer) => {
            if (action == "storage") {
                if (type == "faction") {
                    alt.emitServer("Server:FactionStorage:StorageItem", parseInt(factionId), parseInt(charId), itemName, parseInt(amount), fromContainer);
                } else if (type == "hotel") {
                    alt.emitServer("Server:HotelStorage:StorageItem", parseInt(factionId), itemName, parseInt(amount), fromContainer);
                } else if (type == "house") {
                    alt.emitServer("Server:HouseStorage:StorageItem", parseInt(factionId), itemName, parseInt(amount), fromContainer);
                }
            } else if (action == "take") {
                if (type == "faction") {
                    alt.emitServer("Server:FactionStorage:TakeItem", parseInt(factionId), parseInt(charId), itemName, parseInt(amount));
                } else if (type == "hotel") {
                    alt.emitServer("Server:HotelStorage:TakeItem", parseInt(factionId), itemName, parseInt(amount));
                } else if (type == "house") {
                    alt.emitServer("Server:HouseStorage:TakeItem", parseInt(factionId), itemName, parseInt(amount));
                }
            }
        });

        hudBrowser.on("Client:VehicleTrunk:destroyCEF", () => {
            closeVehicleTrunkCEF();
            alt.emitServer("Server:VehicleTrunk:Close", curVehTrunkId)
            curVehTrunkId = -1;
        });

        hudBrowser.on("Client:VehicleTrunk:VehicleTrunkAction", (action, vehId, charId, itemName, itemAmount, fromContainer, type) => {
            if (action == "storage") {
                alt.emitServer("Server:VehicleTrunk:StorageItem", parseInt(vehId), parseInt(charId), itemName, parseInt(itemAmount), fromContainer, type);
            } else if (action == "take") {
                alt.emitServer("Server:VehicleTrunk:TakeItem", parseInt(vehId), parseInt(charId), itemName, parseInt(itemAmount), type);
            }
        });

        hudBrowser.on("Client:VehicleLicensing:LicensingAction", (action, vehId, vehPlate, newPlate) => {
            alt.emitServer("Server:VehicleLicensing:LicensingAction", action, parseInt(vehId), vehPlate, newPlate);
        });

        hudBrowser.on("Client:VehicleLicensing:destroyCEF", () => {
            closeVehicleLicensingCEF();
        });

        hudBrowser.on("Client:PlayerSearch:TakeItem", (targetCharId, itemName, itemLocation, itemAmount) => {
            alt.emitServer("Server:PlayerSearch:TakeItem", parseInt(targetCharId), itemName, itemLocation, parseInt(itemAmount));
        });

        hudBrowser.on("Client:PlayerSearch:destroyCEF", () => {
            closePlayerSearchCEF();
        });

        hudBrowser.on("Client:GivePlayerLicense:destroyCEF", () => {
            closeGivePlayerLicenseCEF();
        });

        hudBrowser.on("Client:GivePlayerLicense:GiveLicense", (targetCharId, licname) => {
            alt.emitServer("Server:GivePlayerLicense:GiveLicense", parseInt(targetCharId), licname);
        });

        hudBrowser.on("Client:MinijobPilot:SelectJob", (level) => {
            alt.emitServer("Server:MinijobPilot:StartJob", parseInt(level));
        });

        hudBrowser.on("Client:MinijobPilot:destroyCEF", () => {
            closeMinijobPilotCEF();
        });

        hudBrowser.on("Client:MinijobBusdriver:StartJob", (routeId) => {
            alt.emitServer("Server:MinijobBusdriver:StartJob", parseInt(routeId));
        });

        hudBrowser.on("Client:MinijobBusdriver:destroyCEF", () => {
            closeMinijobBusdriverCEF();
        });

        hudBrowser.on("Client:Hotel:destroyCEF", () => {
            closeHotelRentCEF();
        });

        hudBrowser.on("Client:Hotel:RentHotel", (hotelId, apartmentId) => {
            alt.emitServer("Server:Hotel:RentHotel", parseInt(hotelId), parseInt(apartmentId));
        });

        hudBrowser.on("Client:Hotel:LockHotel", (hotelId, apartmentId) => {
            alt.emitServer("Server:Hotel:LockHotel", parseInt(hotelId), parseInt(apartmentId));
        });

        hudBrowser.on("Client:Hotel:EnterHotel", (hotelId, apartmentId) => {
            alt.emitServer("Server:Hotel:EnterHotel", parseInt(hotelId), parseInt(apartmentId));
        });

        hudBrowser.on("Client:HouseEntrance:destroyCEF", () => {
            closeHouseEntranceCEF();
        });

        hudBrowser.on("Client:Shop:robShop", (shopId) => {
            alt.emitServer("Server:Shop:robShop", parseInt(shopId));
        });

        hudBrowser.on("Client:HouseEntrance:BuyHouse", (houseId) => {
            alt.emitServer("Server:House:BuyHouse", parseInt(houseId));
        });

        hudBrowser.on("Client:HouseEntrance:EnterHouse", (houseId) => {
            alt.emitServer("Server:House:EnterHouse", parseInt(houseId));
        });

        hudBrowser.on("Client:HouseEntrance:RentHouse", (houseId) => {
            alt.emitServer("Server:House:RentHouse", parseInt(houseId));
        });

        hudBrowser.on("Client:HouseEntrance:UnrentHouse", (houseId) => {
            alt.emitServer("Server:House:UnrentHouse", parseInt(houseId));
        });

        hudBrowser.on("Client:House:SellHouse", (houseId) => {
            alt.emitServer("Server:House:SellHouse", parseInt(houseId));
        });

        hudBrowser.on("Client:House:setMainHouse", (houseId) => {
            alt.emitServer("Server:House:setMainHouse", parseInt(houseId));
        });

        hudBrowser.on("Client:HouseManage:destroyCEF", () => {
            closeHouseManageCEF();
        });

        hudBrowser.on("Client:HouseManage:setRentPrice", (houseId, rentPrice) => {
            alt.emitServer("Server:HouseManage:setRentPrice", parseInt(houseId), parseInt(rentPrice));
        });

        hudBrowser.on("Client:HouseManage:setRentState", (houseId, rentState) => {
            alt.emitServer("Server:HouseManage:setRentState", parseInt(houseId), `${rentState}`);
        });

        hudBrowser.on("Client:HouseManage:RemoveRenter", (houseId, renterId) => {
            alt.emitServer("Server:HouseManage:RemoveRenter", parseInt(houseId), parseInt(renterId));
        });

        hudBrowser.on("Client:HouseManage:BuyUpgrade", (houseId, upgrade) => {
            alt.emitServer("Server:HouseManage:BuyUpgrade", parseInt(houseId), upgrade);
        });

        hudBrowser.on("Client:HouseManage:TresorAction", (houseId, action, money) => {
            if (action == "withdraw") {
                alt.emitServer("Server:HouseManage:WithdrawMoney", parseInt(houseId), parseInt(money));
            } else if (action == "deposit") {
                alt.emitServer("Server:HouseManage:DepositMoney", parseInt(houseId), parseInt(money));
            }
        });

        hudBrowser.on("Client:Townhall:destroyHouseSelector", () => {
            destroyTownHallHouseSelector();
        });

        hudBrowser.on("Client:Animation:SaveAnimationHotkey", (hotkey, animId, animName, animDict, animFlag, animDuration) => {
            if (hotkey == undefined || animId <= 0) return;
            if (hotkey != "Num1" && hotkey != "Num2" && hotkey != "Num3" && hotkey != "Num4" && hotkey != "Num5" && hotkey != "Num6" && hotkey != "Num7" && hotkey != "Num8" && hotkey != "Num9") {
                alt.emit("Client:HUD:sendNotification", 4, 3500, "Der Inhalt des Hotkeys darf nur aus Num1 bis Num9 bestehen.");
                return;
            }
            alt.LocalStorage.set(`${hotkey}Hotkey`, parseInt(animId));
            alt.LocalStorage.set(`${hotkey}AnimName`, animName);
            alt.LocalStorage.set(`${hotkey}AnimDict`, animDict);
            alt.LocalStorage.set(`${hotkey}AnimFlag`, animFlag);
            alt.LocalStorage.set(`${hotkey}AnimDuration`, animDuration);
            alt.LocalStorage.save();

            alt.emit("Client:HUD:sendNotification", 2, 2500, `Hotkey ${hotkey} erfolgreich belegt - AnimationsID: ${animId}.`);
        });

        hudBrowser.on("Client:Animation:DeleteAnimationHotkey", (hotkey) => {
            if (hotkey == undefined) return;
            if (hotkey != "Num1" && hotkey != "Num2" && hotkey != "Num3" && hotkey != "Num4" && hotkey != "Num5" && hotkey != "Num6" && hotkey != "Num7" && hotkey != "Num8" && hotkey != "Num9") {
                alt.emit("Client:HUD:sendNotification", 4, 3500, "Der Inhalt des Hotkeys darf nur aus Num1 bis Num9 bestehen.");
                return;
            }            
            alt.LocalStorage.delete(`${hotkey}Hotkey`);
            alt.LocalStorage.delete(`${hotkey}AnimName`);
            alt.LocalStorage.delete(`${hotkey}AnimDict`);
            alt.LocalStorage.delete(`${hotkey}AnimFlag`);
            alt.LocalStorage.delete(`${hotkey}AnimDuration`);
            alt.emit("Client:HUD:sendNotification", 2, 2500, `Hotkey ${hotkey} erfolgreich entfernt.`);
        });

        hudBrowser.on("Client:Animation:playAnimation", (animDict, animName, animFlag, animDuration) => {
            playAnimation(animDict, animName, animFlag, animDuration);
        });

        hudBrowser.on("Client:Animations:hideAnimationMenu", () => {
            destroyAnimationMenu();
        });

        hudBrowser.on("Client:Animations:hideClothesRadialMenu", () => {
            destroyClothesRadialMenu();
        });

        hudBrowser.on("Client:Tuning:switchTuningColor", (type, action, r, g, b) => {
            if (curTuningVeh == null) return;
            alt.emitServer("Server:Tuning:switchTuningColor", curTuningVeh, type, action, parseInt(r), parseInt(g), parseInt(b));
        });

        hudBrowser.on("Client:Tuning:switchTuning", (type, id, action) => {
            if (curTuningVeh == null) return;
            alt.emitServer("Server:Tuning:switchTuning", curTuningVeh, type, parseInt(id), action);
        });

        hudBrowser.on("Client:Tuning:closeCEF", () => {
            closeTuningCEF();
        });

        /* Smartphone */
        hudBrowser.on("Client:Smartphone:tryCall", (number) => {
            alt.emitServer("Server:Smartphone:tryCall", parseInt(number));
        });

        hudBrowser.on("Client:Smartphone:denyCall", () => {
            alt.emitServer("Server:Smartphone:denyCall");
        });

        hudBrowser.on("Client:Smartphone:acceptCall", () => {
            alt.emitServer("Server:Smartphone:acceptCall");
        });

        hudBrowser.on("Client:Smartphone:requestChats", () => {
            alt.emitServer("Server:Smartphone:requestChats");
        });

        hudBrowser.on("Client:Smartphone:requestChatMessages", (chatId) => {
            alt.emitServer("Server:Smartphone:requestChatMessages", parseInt(chatId));
        });

        hudBrowser.on("Client:Smartphone:createNewChat", (targetNumber) => {
            alt.emitServer("Server:Smartphone:createNewChat", parseInt(targetNumber));
        });

        hudBrowser.on("Client:Smartphone:sendChatMessage", (selectedChatId, userPhoneNumber, targetMessageUser, unix, encodedText) => {
            if (selectedChatId <= 0 || userPhoneNumber <= 0 || targetMessageUser <= 0) return;
            alt.emitServer("Server:Smartphone:sendChatMessage", parseInt(selectedChatId), parseInt(userPhoneNumber), parseInt(targetMessageUser), parseInt(unix), encodedText);
        });

        hudBrowser.on("Client:Smartphone:deleteChat", (chatId) => {
            if (chatId <= 0) return;
            alt.emitServer("Server:Smartphone:deleteChat", parseInt(chatId));
        });

        hudBrowser.on("Client:Smartphone:setFlyModeEnabled", (isEnabled) => {
            alt.emitServer("Server:Smartphone:setFlyModeEnabled", isEnabled);
        });

        hudBrowser.on("Client:Smartphone:requestPhoneContacts", () => {
            alt.emitServer("Server:Smartphone:requestPhoneContacts");
        });
        hudBrowser.on("Client:Smartphone:requestPhonebusiness", () => {
            alt.emitServer("Server:Smartphone:requestPhonebusiness");
        });
        hudBrowser.on("Client:Smartphone:requestPhoneVerlauf", () => {
            alt.emitServer("Server:Smartphone:requestPhoneVerlauf");
        });

        hudBrowser.on("Client:Smartphone:deleteContact", (contactId) => {
            alt.emitServer("Server:Smartphone:deleteContact", parseInt(contactId));
        });

        hudBrowser.on("Client:Smartphone:addNewContact", (name, number) => {
            alt.emitServer("Server:Smartphone:addNewContact", name, parseInt(number));
        });

        hudBrowser.on("Client:Smartphone:editContact", (id, name, number) => {
            alt.emitServer("Server:Smartphone:editContact", parseInt(id), name, parseInt(number));
        });

        hudBrowser.on("Client:ClothesStorage:setCharacterClothes", (clothesName, clothesTyp) => {
            if (clothesName == undefined || clothesTyp == undefined) return;
            alt.emitServer("Server:ClothesStorage:setCharacterClothes", clothesTyp, clothesName);
        });

        hudBrowser.on("Client:Smartphone:SearchLSPDIntranetPeople", (name) => {
            alt.emitServer("Server:Smartphone:SearchLSPDIntranetPeople", name);
        });

        hudBrowser.on("Client:Smartphone:GiveLSPDIntranetWanteds", (selectedCharId, wantedList) => {
            alt.emitServer("Server:Smartphone:GiveLSPDIntranetWanteds", parseInt(selectedCharId), wantedList);
        });

        hudBrowser.on("Client:Smartphone:requestLSPDIntranetPersonWanteds", (charid) => {
            alt.emitServer("Server:Smartphone:requestLSPDIntranetPersonWanteds", parseInt(charid));
        });

        hudBrowser.on("Client:Smartphone:DeleteLSPDIntranetWanted", (id, charid) => {
            alt.emitServer("Server:Smartphone:DeleteLSPDIntranetWanted", parseInt(id), parseInt(charid));
        });

        hudBrowser.on("Client:Smartphone:requestPoliceAppMostWanteds", () => {
            alt.emitServer("Server:Smartphone:requestPoliceAppMostWanteds");
        });

        hudBrowser.on("Client:Smartphone:locateMostWanted", (X, Y) => {
            game.setNewWaypoint(parseFloat(X), parseFloat(Y));
        });
    }
});

alt.onServer("Client:HUD:setIntoVehicle", (vehicle) => {
    alt.setTimeout(() => {
      native.setPedIntoVehicle(alt.Player.local.scriptID, vehicle.scriptID, -1);
    }, 400);
  })

// Geld-HUD
alt.onServer("Client:HUD:updateMoney", (currentMoney) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:HUD:updateMoney", currentMoney);
    }
});
// Geld-HUD
alt.onServer("Client:HUD:updateMoneyBlack", (currentmoneyblack) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:HUD:updateMoneyBlack", currentmoneyblack);
    }
});

alt.onServer("Client:Smartphone:setCurrentFunkFrequence", (funkfrequence) => {
    if (funkfrequence == null || funkfrequence == "null") {
        currentRadioFrequence = null;
        return;
    }
    currentRadioFrequence = funkfrequence;
});

alt.onServer("Client:HUD:UpdateDesire", (armor, health, hunger, thirst) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:HUD:updateDesireHUD", armor, health, hunger, thirst);
    }
});

alt.onServer("Client:HUD:updateHUDPosInVeh", (state, fuel, km) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:HUD:updateHUDPosInVeh", state, fuel, km);
    }
});

alt.onServer("Client:HUD:sendNotification", (type, duration, msg, delay) => {
    alt.setTimeout(() => {
        if (hudBrowser != null) {
            hudBrowser.emit("CEF:HUD:sendNotification", type, duration, msg);
        }
    }, delay);
});

alt.on("Client:HUD:sendNotification", (type, duration, msg) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:HUD:sendNotification", type, duration, msg);
    }
});

alt.on("Client:SaltyChat:MicStateChanged", (state, voiceRange) => {
    if (hudBrowser == null || !browserReady) return;
    if (state) hudBrowser.emit("CEF:HUD:updateHUDVoice", 0);
    else hudBrowser.emit("CEF:HUD:updateHUDVoice", voiceRange);
});

alt.onServer("client::updateVoiceRange", (voiceRange) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:HUD:updateHUDVoice", voiceRange);
    let drawmarkertick = alt.everyTick(() => {
        native.drawMarker(28, alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z - 0.95, 0, 0, 0, 0, 0, 0, voiceRange, voiceRange, 0.2, 52, 152, 219, 50, 0, false, 2, false, undefined, undefined, false);
    })
    alt.setTimeout(() => {
        alt.clearEveryTick(drawmarkertick);
    }, 500);
});

alt.onServer("Client:HUD:createIdentityCardApplyForm", (charname, gender, adress, birthdate, curBirthpl) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && curBirthpl == "None" && identityCardApplyCEFopened == false) {
        hudBrowser.emit("CEF:HUD:createIdentityCardApplyForm", charname, gender, adress, birthdate);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        identityCardApplyCEFopened = true;
    }
});

alt.onServer("Client:IdentityCard:showIdentityCard", (type, infoArray) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:IdentityCard:showIdentityCard", type, infoArray);
    }
});

alt.onServer("Client:Bank:createBankAccountManageForm", (bankArray, curBank) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && BankAccountManageFormOpened == false) {
        hudBrowser.emit("CEF:Bank:createBankAccountManageForm", bankArray, curBank);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        BankAccountManageFormOpened = true;
    }
});

alt.onServer("Client:ATM:BankATMcreateCEF", (pin, accNumber, zoneName) => {
    alt.emitServer("Server:Inventory:closeCEF");
    alt.setTimeout(function() {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:ATM:BankATMcreateCEF", pin, accNumber, zoneName);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        ATMcefOpened = true;
    }, 500);
});

alt.onServer("Client:ATM:BankATMSetRequestedData", (curBalance, paperArray) => {
    if (hudBrowser != null && ATMcefOpened == true) {
        hudBrowser.emit("CEF:ATM:BankATMSetRequestedData", curBalance, paperArray);
    }
});

alt.onServer("Client:ATM:BankATMdestroyCEFBrowser", () => {
    if (hudBrowser != null && ATMcefOpened == true) {
        hudBrowser.emit("CEF:ATM:BankATMdestroyCEF");
    }
});

alt.onServer("Client:Shop:shopCEFCreateCEF", (itemArray, shopId, isOnlySelling) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && ShopCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:Shop:shopCEFBoxCreateCEF", itemArray, shopId, isOnlySelling);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        ShopCefOpened = true;
    }
});

alt.onServer("Client:Barber:barberCreateCEF", (headoverlayarray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && BarberCefOpened == false) {
        hudBrowser.emit("CEF:Barber:barberCEFBoxCreateCEF", headoverlayarray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        BarberCefOpened = true;

        let barberInterval = alt.setInterval(() => {
            game.invalidateIdleCam();
            if (BarberCefOpened === false) {
                alt.clearInterval(barberInterval);
            }
        }, 5000);
    }
});


alt.onServer("Client:Garage:OpenGarage", (garageId, garagename, garageInArray, garageOutArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && GarageCefOpened == false) {
        hudBrowser.emit("CEF:Garage:OpenGarage", garageId, garagename, garageInArray, garageOutArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        GarageCefOpened = true;
    }
});

alt.onServer("Client:VehicleShop:OpenCEF", (shopId, shopname, itemArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && VehicleShopCefOpened == false) {
        hudBrowser.emit("CEF:VehicleShop:SetListContent", shopId, shopname, itemArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        VehicleShopCefOpened = true;
    }
});

alt.onServer("Client:Jobcenter:OpenCEF", (jobArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && JobcenterCefOpened == false) {
        hudBrowser.emit("CEF:Jobcenter:OpenCEF", jobArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        JobcenterCefOpened = true;
    }
});

alt.onServer("Client:FuelStation:OpenCEF", (fuelStationId, stationName, owner, maxFuel, availableLiter, fuelArray, vehID) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && FuelStationCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:FuelStation:OpenCEF", fuelStationId, stationName, owner, maxFuel, availableLiter, fuelArray, vehID);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        FuelStationCefOpened = true;
    }
});

alt.onServer("Client:ClothesShop:createCEF", (shopId) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && ClothesShopCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:ClothesShop:createCEF", shopId);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        ClothesShopCefOpened = true;
        let shopInterval = alt.setInterval(() => {
            game.invalidateIdleCam();
            if (ClothesShopCefOpened === false) {
                alt.clearInterval(shopInterval);
            }
        }, 5000);
    }
});

alt.onServer("Client:ClothesStorage:createCEF", () => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && ClothesStorageCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:ClothesStorage:createCEF");
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        ClothesStorageCefOpened = true;
        let shopInterval = alt.setInterval(() => {
            game.invalidateIdleCam();
            if (ClothesStorageCefOpened === false) {
                alt.clearInterval(shopInterval);
            }
        }, 5000);
    }
});

alt.onServer("Client:ClothesShop:sendItemsToClient", (items) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:ClothesShop:sendItemsToClient", items);
    }
});

alt.onServer("Client:ClothesStorage:sendItemsToClient", (items) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:ClothesStorage:sendItemsToClient", items);
    }
});

alt.onServer("Client:FactionBank:createCEF", (type, factionId, factionBalance) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && bankFactionATMCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:FactionBank:createCEF", type, factionId, factionBalance);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        bankFactionATMCefOpened = true;
    }
});


alt.onServer("Client:Vehicle:showChangeOwnerHUD", (vehName) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && changeVehOwnerCefOpened == false) {
        hudBrowser.emit("CEF:Vehicle:showChangeOwnerHUD", vehName);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        changeVehOwnerCefOpened = true;
    }
});

alt.onServer("Client:GivePlayerBill:openCEF", (type, targetCharId) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && GivePlayerBillCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:GivePlayerBill:openCEF", type, targetCharId);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        GivePlayerBillCefOpened = true;
    }
});

alt.onServer("Client:RecievePlayerBill:openCEF", (type, factionCompanyId, moneyAmount, reason, factionCompanyName, charId) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && RecievePlayerBillCefOpened == false) {
        hudBrowser.emit("CEF:RecievePlayerBill:openCEF", type, factionCompanyId, moneyAmount, reason, factionCompanyName, charId);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        RecievePlayerBillCefOpened = true;
    }
});

alt.onServer("Client:FactionStorage:openCEF", (charId, factionId, type, invArray, storageArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && FactionStorageCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        alt.log("storageOpenClientside");
        hudBrowser.emit("CEF:FactionStorage:openCEF", charId, factionId, type, invArray, storageArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        FactionStorageCefOpened = true;
    }
});

alt.onServer("Client:VehicleTrunk:openCEF", (charId, vehID, type, invArray, storageArray, currentweight, maxweight) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && VehicleTrunkCefOpened == false) {
        hudBrowser.emit("CEF:VehicleTrunk:openCEF", charId, vehID, type, invArray, storageArray, currentweight, maxweight);
        alt.emitServer("Server:CEF:setCefStatus", true);
        curVehTrunkId = vehID;
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        VehicleTrunkCefOpened = true;
    }
});

alt.onServer("Client:VehicleLicensing:openCEF", (vehArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && VehicleLicensingCefOpened == false) {
        hudBrowser.emit("CEF:VehicleLicensing:openCEF", vehArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        VehicleLicensingCefOpened = true;
    }
});

alt.onServer("Client:PlayerSearch:openCEF", (targetCharId, invArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && PlayerSearchInventoryCefOpened == false) {
        hudBrowser.emit("CEF:PlayerSearch:openCEF", targetCharId, invArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        PlayerSearchInventoryCefOpened = true;
    }
});

alt.onServer("Client:GivePlayerLicense:openCEF", (targetCharId, licArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && GivePlayerLicenseCefOpened == false) {
        hudBrowser.emit("CEF:GivePlayerLicense:SetGivePlayerLicenseCEFContent", targetCharId, licArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        GivePlayerLicenseCefOpened = true;
    }
});

alt.onServer("Client:MinijobPilot:openCEF", () => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && MinijobPilotCefOpened == false) {
        hudBrowser.emit("CEF:MinijobPilot:openCEF");
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        MinijobPilotCefOpened = true;
    }
});

alt.onServer("Client:MinijobBusdriver:openCEF", (routeArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && MinijobBusdriverCefOpened == false) {
        hudBrowser.emit("CEF:MinijobBusdriver:openCEF", routeArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        MinijobBusdriverCefOpened = true;
    }
});

alt.onServer("Client:HouseManage:openCEF", (houseInfoArray, renterArray) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && HouseManageCefOpened == false) {
        hudBrowser.emit("CEF:HouseManage:openCEF", houseInfoArray, renterArray);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        HouseManageCefOpened = true;
    }
});

alt.onServer("Client:Hotel:setApartmentItems", (array) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:Hotel:setApartmentItems", array);
    }
});

alt.onServer("Client:Hotel:openCEF", (hotelname) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && HotelRentCefOpened == false) {
        HotelRentCefOpened = true;
        hudBrowser.emit("CEF:Hotel:openCEF", hotelname);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
    }
});

alt.onServer("Client:HouseEntrance:openCEF", (charId, houseArray, isRentedIn) => {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && HouseEntranceCefOpened == false) {
        hudBrowser.emit("CEF:HouseEntrance:openCEF", charId, houseArray, isRentedIn);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        HouseEntranceCefOpened = true;
    }
});

alt.onServer("Client:Tuning:openTuningMenu", (veh, Items) => {
    if (hudBrowser != null && TuningMenuCefOpened == false) {
        curTuningVeh = veh;
        hudBrowser.emit("CEF:Tuning:openTuningMenu", Items);
        alt.emitServer("Server:CEF:setCefStatus", true);
        alt.toggleGameControls(false);
        alt.showCursor(true);
        hudBrowser.focus();
        TuningMenuCefOpened = true;
    }
});

alt.onServer("Client:Deathscreen:openCEF", () => {
    if (hudBrowser != null && DeathscreenCefOpened == false) {
        closeAllCEFs();
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.setEntityInvincible(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        DeathscreenCefOpened = true;
        isPlayerDead = true;
        deathScreen = new alt.WebView("http://resource/client/cef/deathscreen/death.html");
        deathScreen.focus();
        // alt.setTimeout(() => {
        //     hudBrowser.focus();
        //     hudBrowser.emit("CEF:Deathscreen:openCEF");
        // }, 3000);
    }
});

alt.onServer("Client:Deathscreen:closeCEF", () => {
    if (hudBrowser != null) {
        deathScreen.destroy();
        hudBrowser.emit("CEF:Deathscreen:closeCEF");
        alt.emitServer("Server:CEF:setCefStatus", false);
        game.freezeEntityPosition(alt.Player.local.scriptID, false);
        game.setEntityInvincible(alt.Player.local.scriptID, false);
        alt.showCursor(false);
        alt.toggleGameControls(true);
        hudBrowser.unfocus();
        DeathscreenCefOpened = false;
        isPlayerDead = false;
    }
});


alt.onServer("Client:Townhall:openHouseSelector", (array) => {
    if (hudBrowser != null && !TownhallHouseSelectorCefOpened) {
        hudBrowser.emit("CEF:Townhall:openHouseSelector", array);
        alt.emitServer("Server:CEF:setCefStatus", true);
        game.freezeEntityPosition(alt.Player.local.scriptID, true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        TownhallHouseSelectorCefOpened = true;
    }
});

alt.onServer("Client:Smartphone:equipPhone", (isEquipped, phoneNumber, isFlyModeEnabled) => {
    let interval = alt.setInterval(() => {
        if (hudBrowser != null && browserReady) {
            alt.clearInterval(interval);
            hudBrowser.emit("CEF:Smartphone:equipPhone", isEquipped, phoneNumber, isFlyModeEnabled);
            isPhoneEquipped = isEquipped;
        }
    }, 10);
});

alt.onServer("Client:Smartphone:showPhoneReceiveCall", (number) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:showPhoneReceiveCall", number);
});

alt.onServer("Client:Smartphone:showPhoneCallActive", (number) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:showPhoneCallActive", number);
});

alt.onServer("Client:Smartphone:addChatJSON", (chats) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:addChatJSON", chats);
});

alt.onServer("Client:Smartphone:addMessageJSON", (msg) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:addMessageJSON", msg);
});

alt.onServer("Client:Smartphone:setAllMessages", () => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:setAllMessages");
});

alt.onServer("Client:Smartphone:setAllChats", () => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:setAllChats");
});

alt.onServer("Client:Smartphone:recieveNewMessage", (chatId, phoneNumber, message) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:recieveNewMessage", chatId, phoneNumber, message);
});

alt.onServer("Client:Smartphone:ShowPhoneCallError", (errorId) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:ShowPhoneCallError", errorId);
});

alt.onServer("Client:Smartphone:addContactJSON", (json) => {
    if (hudBrowser == null && !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:addContactJSON", json);
});

alt.onServer("Client:Smartphone:setAllContacts", () => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:setAllContacts");
});
//////////////////////////////////////////////
alt.onServer("Client:Smartphone:addbusinessJSON", (json) => {
    if (hudBrowser == null && !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:addbusinessJSON", json);
});

alt.onServer("Client:Smartphone:setAllbusiness", () => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:setAllbusiness");
});
//////////////////////////////////////////////
alt.onServer("Client:Smartphone:addVerlaufJSON", (json) => {
    if (hudBrowser == null && !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:addVerlaufJSON", json);
});

alt.onServer("Client:Smartphone:setAllVerlauf", () => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:setAllVerlauf");
});

alt.onServer("Client:Smartphone:ShowLSPDIntranetApp", (shouldBeVisible, serverWanteds) => {
    let interval = alt.setInterval(() => {
        if (hudBrowser != null && browserReady) {
            alt.clearInterval(interval);
            hudBrowser.emit("CEF:Smartphone:ShowLSPDIntranetApp", shouldBeVisible, serverWanteds);
        }
    }, 1000);
});

alt.onServer("Client:Smartphone:SetLSPDIntranetSearchedPeople", (searchedPersonsJSON) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:SetLSPDIntranetSearchedPeople", searchedPersonsJSON);
});

alt.onServer("Client:Smartphone:setLSPDIntranetPersonWanteds", (json) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:setLSPDIntranetPersonWanteds", json);
});

alt.onServer("Client:Smartphone:setPoliceAppMostWanteds", (mostWanteds) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:setPoliceAppMostWanteds", mostWanteds);
});

alt.onServer("Client:Smartphone:showNotification", (message, app, fn, sound) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Smartphone:showNotification", message, app, fn, sound);
});

alt.onServer("Client:HUD:SetPlayerHUDVehicleInfos", (fuel, km) => {
    if (hudBrowser != null && alt.Player.local.vehicle != null) {
        hudBrowser.emit("CEF:HUD:SetPlayerHUDVehicleInfos", fuel, km);
    }
});

alt.onServer("Client:Animations:setupItems", (array) => {
    let interval = alt.setInterval(() => {
        if (hudBrowser != null && browserReady) {
            alt.clearInterval(interval);
            hudBrowser.emit("CEF:Animations:setupItems", array);
        }
    }, 10);
});

let OldVehKMPos,
    curVehKMid = 0,
    GetVehKMPos = false;


    
//Laptop
alt.onServer("Client:Laptop:activateLaptop", (isActivated) => {
    isLaptopActivated = isActivated;
});

alt.onServer("Client:Laptop:updateApps", (LSPDApp) => {
    if (hudBrowser == null || !browserReady) return;
    hudBrowser.emit("CEF:Laptop:updateApps", LSPDApp);
});

alt.onServer("Client:HUD:GetDistanceForVehicleKM", () => {
    if (hudBrowser != null && alt.Player.local.vehicle != null) {
        if (curVehKMid == 0) { curVehKMid = alt.Player.local.vehicle.scriptID; }
        if (curVehKMid != alt.Player.local.vehicle.scriptID) { GetVehKMPos = false; }

        if (!GetVehKMPos) {
            OldVehKMPos = alt.Player.local.vehicle.pos;
            GetVehKMPos = true;
            return;
        }

        if (GetVehKMPos) {
            let curPos = alt.Player.local.vehicle.pos;
            let dist = game.getDistanceBetweenCoords(OldVehKMPos.x, OldVehKMPos.y, OldVehKMPos.z, curPos.x, curPos.y, curPos.z, false);
            alt.emitServer("Server:Vehicle:UpdateVehicleKM", dist);
            OldVehKMPos = alt.Player.local.vehicle.pos;
        }
    }
});
//laptop ends

let vehicle = null;
let interactVehicle = null;
let interactPlayer = null;
let playerRC = null;
let selectedRaycastId = null;
let InteractMenuUsing = false;
let AnimationMenuUsing = false;
let AnimationMenuUsingPage2 = false;
let AnimationMenuUsingPage3 = false;
let ClothesRadialMenuUsing = false;

alt.onServer("Client:RaycastMenu:SetMenuItems", (type, itemArray) => { //Type: player, vehicleOut, vehicleIn
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:InteractionMenu:toggleInteractionMenu", true, type, itemArray);
    }
});

alt.onServer("Client:AnimationMenu:SetMenuItems", (itemArray) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:AnimationMenu:toggleInteractionMenu", true, itemArray);
    }
});

alt.onServer("Client:AnimationMenuPage2:SetMenuItems", (itemArray) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:AnimationMenu:toggleInteractionMenu", false);
        hudBrowser.emit("CEF:AnimationMenuPage3:toggleInteractionMenu", false);
        hudBrowser.emit("CEF:AnimationMenuPage2:toggleInteractionMenu", true, itemArray);
    }
});

alt.onServer("Client:AnimationMenuPage3:SetMenuItems", (itemArray) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:AnimationMenu:toggleInteractionMenu", false);
        hudBrowser.emit("CEF:AnimationMenuPage2:toggleInteractionMenu", false);
        hudBrowser.emit("CEF:AnimationMenuPage3:toggleInteractionMenu", true, itemArray);
    }
});

alt.onServer("Client:ClothesRadial:SetMenuItems", (itemArray) => {
    if (hudBrowser != null) {
        hudBrowser.emit("CEF:ClothesRadial:toggleInteractionMenu", true, itemArray);
    }
});

alt.on('keydown', (key) => {
    if (key == 'X'.charCodeAt(0)) {
        if (alt.Player.local.getSyncedMeta("IsCefOpen")) return;
        let result = Raycast.line(1.5, 2.5);
        if (result == undefined && !alt.Player.local.vehicle) return;
        if (!alt.Player.local.vehicle) {
            if (result.isHit && result.entityType != 0) {
                if (result.entityType == 1 && hudBrowser != null) {
                    selectedRaycastId = result.hitEntity;
                    interactPlayer = alt.Player.all.find(x => x.scriptID == selectedRaycastId);
                    if (!interactPlayer) return;
                    InteractMenuUsing = true;
                    hudBrowser.focus();
                    alt.showCursor(true);
                    alt.toggleGameControls(false);
                    alt.emitServer("Server:InteractionMenu:GetMenuPlayerItems", "player", interactPlayer);
                    interactPlayer = null;
                    return;
                } else if (result.entityType == 2 && hudBrowser != null) {
                    selectedRaycastId = result.hitEntity;
                    interactVehicle = alt.Vehicle.all.find(x => x.scriptID == selectedRaycastId);
                    if (!interactVehicle) return;
                    InteractMenuUsing = true;
                    hudBrowser.focus();
                    alt.showCursor(true);
                    alt.toggleGameControls(false);
                    alt.emitServer("Server:InteractionMenu:GetMenuVehicleItems", "vehicleOut", interactVehicle);
                    interactVehicle = null;
                    return;
                }
            }
        }

        if (alt.Player.local.vehicle && hudBrowser != null) {
            selectedRaycastId = alt.Player.local.vehicle.scriptID;
            interactVehicle = alt.Vehicle.all.find(x => x.scriptID == selectedRaycastId);
            InteractMenuUsing = true;
            hudBrowser.focus();
            alt.showCursor(true);
            alt.toggleGameControls(false);
            if (!interactVehicle) return;
            alt.emitServer("Server:InteractionMenu:GetMenuVehicleItems", "vehicleIn", interactVehicle);
            interactVehicle = null;
            return;
        }
    } else if (key == 'M'.charCodeAt(0)) {
        if (alt.Player.local.getSyncedMeta("IsCefOpen") || alt.Player.local.vehicle) return;
        AnimationMenuUsing = true;
        hudBrowser.focus();
        alt.showCursor(true);
        alt.toggleGameControls(false);
        alt.emitServer("Server:AnimationMenu:GetAnimationItems");
        return;
    } 

    else if (key == 122) {
        closeAllCEFs;
    }
    
    else if (key === 39) { 
        if (AnimationMenuUsing == false && AnimationMenuUsingPage2 == false && AnimationMenuUsingPage3 == false) return;

        if (AnimationMenuUsing == false) { 
            if (AnimationMenuUsingPage2 == true) {
                AnimationMenuUsingPage3 = true;
                AnimationMenuUsing, AnimationMenuUsingPage2 = false;
                alt.emitServer("Server:AnimationMenuPage3:GetAnimationItems");
            }
        } else {
            AnimationMenuUsingPage2 = true;
            AnimationMenuUsing = false;
            alt.emitServer("Server:AnimationMenuPage2:GetAnimationItems");
        }
    }

    else if (key === 37) {
        if (AnimationMenuUsing == false && AnimationMenuUsingPage2 == false && AnimationMenuUsingPage3 == false) return;

        if (AnimationMenuUsing == false) {
            if (AnimationMenuUsingPage2 == true) {
                AnimationMenuUsing = true;
                AnimationMenuUsingPage2, AnimationMenuUsingPage3 = false;
                alt.emitServer("Server:AnimationMenu:GetAnimationItems");
            } else if (AnimationMenuUsingPage3 == true) {
                AnimationMenuUsingPage2 = true;
                AnimationMenuUsing, AnimationMenuUsingPage3 = false;
                alt.emitServer("Server:AnimationMenuPage2:GetAnimationItems");
            }
        }
    }
    
    else if (key == 'K'.charCodeAt(0)) {
        if (alt.Player.local.getSyncedMeta("IsCefOpen")) return;
        ClothesRadialMenuUsing = true;
        hudBrowser.focus();
        alt.showCursor(true);
        alt.toggleGameControls(false);
        alt.emitServer("Server:ClothesRadial:GetClothesRadialItems");
        return;
    } 
    // SALTYCHAT 
    else if (key === 'N'.charCodeAt(0)) {
        if (currentRadioFrequence == null || currentRadioFrequence == undefined  || alt.Player.local.getSyncedMeta("IsCefOpen")) return;
        alt.emit("SaltyChat:UseRadio", true, true);
    }  else if (key === 220 && !alt.Player.local.getSyncedMeta("IsCefOpen")) { // Z (Y auf englischer Tastatur)
        alt.emit("SaltyChat:ToggleRange");
    } else if (key === 'B'.charCodeAt(0) && alt.Player.local.vehicle && alt.Player.local.scriptID == game.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1) && game.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18) {
        isPlayerUsingMegaphone = true;
        alt.emit("SaltyChat:UseMegaphone", true);
    }
});

alt.on('keyup', (key) => {
    if (key == 'X'.charCodeAt(0)) {
        if (hudBrowser == null || InteractMenuUsing == false) return;
        hudBrowser.emit("CEF:InteractionMenu:requestAction");
        hudBrowser.emit("CEF:InteractionMenu:toggleInteractionMenu", false);
        InteractMenuUsing = false;
        hudBrowser.unfocus();
        alt.showCursor(false);
        alt.toggleGameControls(true);
    } else if (key == 'M'.charCodeAt(0)) { 
        if (hudBrowser == null) return;
        if (AnimationMenuUsing == true) {
            hudBrowser.emit("CEF:AnimationMenu:requestAction");
            hudBrowser.emit("CEF:AnimationMenu:toggleInteractionMenu", false);
            AnimationMenuUsing = false;
        } else if (AnimationMenuUsingPage2 == true) {
            hudBrowser.emit("CEF:AnimationMenuPage2:requestAction");
            hudBrowser.emit("CEF:AnimationMenuPage2:toggleInteractionMenu", false);
            AnimationMenuUsingPage2 = false;
        } else if (AnimationMenuUsingPage3 == true) {
            hudBrowser.emit("CEF:AnimationMenuPage3:requestAction");
            hudBrowser.emit("CEF:AnimationMenuPage3:toggleInteractionMenu", false);
            AnimationMenuUsingPage3 = false;
        } else return;

        hudBrowser.unfocus();
        alt.showCursor(false);
        alt.toggleGameControls(true);
    }/* else if (key == 116) { //F5
        if (hudBrowser == null) return;
        if (!AnimationMenuCefOpened) {
            initializeFavouriteAnims();
        } else {
            hudBrowser.emit("CEF:Animations:hideAnimationMenu");
        }
        AnimationMenuCefOpened != AnimationMenuCefOpened;
    } */
    else if (key == 'K'.charCodeAt(0)) {
        if (hudBrowser == null || ClothesRadialMenuUsing == false) return;
        hudBrowser.emit("CEF:ClothesRadial:requestAction");
        hudBrowser.emit("CEF:ClothesRadial:toggleInteractionMenu", false);
        ClothesRadialMenuUsing = false;
        hudBrowser.unfocus();
        alt.showCursor(false);
        alt.toggleGameControls(true);
    } else  if (key == 'H'.charCodeAt(0)) {
        if (!alt.gameControlsEnabled()) return;
        if (!hasHandsUp) {
            game.requestAnimDict("missminuteman_1ig_2");
            let interval = alt.setInterval(() => {
                if (game.hasAnimDictLoaded("missminuteman_1ig_2")) {
                    alt.clearInterval(interval);
                    game.taskPlayAnim(alt.Player.local.scriptID, "missminuteman_1ig_2", "handsup_enter", 8.0, 8.0, -1, 50, 0, false, false, false);
                }
            }, 10);

            hasHandsUp = true;
       } else {
            native.clearPedTasks(alt.Player.local.scriptID);
            hasHandsUp = false;
        };
    } else if (key == 33) {
        //Smartphone Bild hoch
        if (hudBrowser == null || !browserReady || isPlayerDead || !isPhoneEquipped || alt.Player.local.getSyncedMeta("IsCefOpen") == true || alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        hudBrowser.emit("CEF:Smartphone:togglePhone", true);
        playAnimation("cellphone@in_car@ds", "cellphone_text_read_base", 49, -1);
        alt.showCursor(true);
        alt.emitServer("Server:CEF:setCefStatus", true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
    } else if (key == 34) {
        //Smartphone Bild runter
        if (hudBrowser == null || !browserReady || !isPhoneEquipped) return;
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == false && alt.Player.local.getSyncedMeta("HasRopeCuffs") == false) game.clearPedTasks(alt.Player.local.scriptID);
        hudBrowser.emit("CEF:Smartphone:togglePhone", false);
        alt.emitServer("Server:CEF:setCefStatus", false);
        alt.showCursor(false);
        alt.toggleGameControls(true);
        hudBrowser.unfocus();
    }
    // SALTYCHAT
    else if (key === 'N'.charCodeAt(0)) {
        if (currentRadioFrequence == null || currentRadioFrequence == undefined || !alt.gameControlsEnabled()) return;
        alt.emit("SaltyChat:UseRadio", true, false);
    } else if (key === 'B'.charCodeAt(0) && isPlayerUsingMegaphone) {
        isPlayerUsingMegaphone = false;
        alt.emit("SaltyChat:UseMegaphone", false);
    }else if (key == 114) {
        if (!isLaptopActivated) return;
        if (isLaptopCEFOpened) {
            hudBrowser.emit("CEF:Laptop:hideLaptop");
            alt.showCursor(false);
            alt.toggleGameControls(true);
            alt.emitServer("Server:CEF:setCefStatus", false);
            hudBrowser.unfocus();
        } else {
            hudBrowser.emit("CEF:Laptop:showLaptop");
            alt.showCursor(true);
            alt.toggleGameControls(false);
            alt.emitServer("Server:CEF:setCefStatus", true);
            hudBrowser.focus();
        }
        isLaptopCEFOpened = !isLaptopCEFOpened;
    }
});

function InterActionMenuDoAction(type, action) {
    if (selectedRaycastId != null && selectedRaycastId != 0 && type != "none") {
        if (type == "vehicleOut" || type == "vehicleIn") { type = "vehicle"; }
        if (type == "vehicle") {
            vehicle = alt.Vehicle.all.find(x => x.scriptID == selectedRaycastId);
            if (!vehicle) return;
            if (action == "vehtoggleLock") {
                alt.emitServer("Server:Raycast:LockVehicle", vehicle);
            } else if (action == "vehtoggleEngine") {
                alt.emitServer("Server:Raycast:ToggleVehicleEngine", vehicle);
            } else if (action == "vehFuelVehicle") {
                alt.emitServer("Server:Raycast:OpenVehicleFuelMenu", vehicle);
            } else if (action == "vehRepair") {
                alt.emitServer("Server:Raycast:RepairVehicle", vehicle);
            } else if (action == "vehOpenCloseTrunk") {
                alt.emitServer("Server:Raycast:OpenCloseVehicleTrunk", vehicle);
            } else if (action == "vehViewTrunkContent") {
                alt.emitServer("Server:Raycast:ViewVehicleTrunk", vehicle);
            } else if (action == "vehViewGloveboxContent") {
                alt.emitServer("Server:Raycast:ViewVehicleGlovebox", vehicle);
            } else if (action == "vehTow") {
                alt.emitServer("Server:Raycast:towVehicle", vehicle);
            } else if (action == "vehTuning") {
                alt.emitServer("Server:Raycast:tuneVehicle", vehicle);
            } else if (action == "vehchangeowner") {
                alt.emitServer("Server:Raycast:showChangeOwnerHUD", vehicle);
            } else if (action == "vehBreakVehicle") {
                alt.emitServer("Server:Raycast:BreakVehicle", vehicle);
            } else if (action == "vehbreakEngine") {
                alt.emitServer("Server:Raycast:BreakVehicleEngine", vehicle);
            }
            vehicle = null;
        } else if (type == "player") {
            playerRC = alt.Player.all.find(x => x.scriptID == selectedRaycastId);
            if (!playerRC) return;
            if (action == "playersupportId") {
                alt.emitServer("Server:Raycast:showPlayerSupportId", playerRC);
            } else if (action == "playergiveItem") {
                alt.emitServer("Server:Raycast:givePlayerItemRequest", playerRC);
            } else if (action == "playergiveFactionBill") {
                alt.emitServer("Server:Raycast:OpenGivePlayerBillCEF", playerRC, "faction");
            } else if (action == "playergiveCompanyBill") {
                alt.emitServer("Server:Raycast:OpenGivePlayerBillCEF", playerRC, "company");
            } else if (action == "playerGiveTakeHandcuffs") {
                alt.emitServer("Server:Raycast:GiveTakeHandcuffs", playerRC);
            } else if (action == "playerGiveTakeRopeCuffs") {
                alt.emitServer("Server:Raycast:GiveTakeRopeCuffs", playerRC);
            } else if (action == "playerSearchInventory") {
                alt.emitServer("Server:Raycast:SearchPlayerInventory", playerRC);
            } else if (action == "playerGiveLicense") {
                alt.emitServer("Server:Raycast:openGivePlayerLicenseCEF", playerRC);
            } else if (action == "playerRevive") {
                alt.emitServer("Server:Raycast:RevivePlayer", playerRC);
            } else if (action == "playerJail") {
                alt.emitServer("Server:Raycast:jailPlayer", playerRC);
            } else if (action == "showIdCard") {
                alt.emitServer("Server:Raycast:showIdcard", playerRC);
            } else if (action == "healPlayer") {
                alt.emitServer("Server:Raycast:healPlayer", playerRC);
            }
            playerRC = null;
        }
        selectedRaycastId = null;
    }
}

function InterActionMenuDoActionAnimationMenu(action) {
    new Promise((resolve, reject) => {
        if (action == "crossarms3") {
            playAnimation("anim@heists@heist_corona@single_team", "single_team_loop_boss", 1, 300000)
        } else if (action == "facepalm") {
            playAnimation("random@car_thief@agitated@idle_a", "agitated_idle_a", 1, 300000)
        } else if (action == "finger2") {
            playAnimation("anim@mp_player_intupperfinger", "idle_a_fp", 1, 300000)
        } else if (action == "wait5") {
            playAnimation("timetable@amanda@ig_3", "ig_3_base_tracy", 1, 300000)
        } else if (action == "hug3") {
            playAnimation("mp_ped_interaction", "hugs_guy_a", 1, 3000)
        } else if (action == "inspect") {
            playAnimation("random@train_tracks", "idle_e", 1, 300000)
        } else if (action == "kneel2") {
            playAnimation("rcmextreme3", "idle", 1, 300000)
        } else if (action == "lean4") {
            playAnimation("amb@world_human_leaning@male@wall@back@foot_up@idle_a", "idle_a", 1, 300000)
        } else if (action == "mechanic") {
            playAnimation("mini@repair", "fixing_a_ped",  1, 300000)
        } else if (action == "pushup") {
            playAnimation("amb@world_human_push_ups@male@idle_a", "idle_d", 1, 300000)
        } else if (action = 'close') {
            return resolve(game.clearPedTasks(alt.Player.local.scriptID));
        }
    }).then(() => {
        console.log("Fehler aufgetreten...")
    });
}

function InterActionMenuDoActionAnimationMenuPage2(action) {
    new Promise((resolve, reject) => {
        if (action == "dancef6") {
            playAnimation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "high_center_up", 1, 300000)
        } else if (action == "danceslow2") {
            playAnimation("anim@amb@nightclub@mini@dance@dance_solo@female@var_a@", "low_center", 1, 300000)
        } else if (action == "dance3") {
            playAnimation("anim@amb@nightclub@mini@dance@dance_solo@male@var_a@", "high_center", 1, 300000)
        } else if (action == "danceupper") {
            playAnimation("anim@amb@nightclub@mini@dance@dance_solo@female@var_b@", "high_center", 1, 300000)
        } else if (action == "danceshy") {
            playAnimation("anim@amb@nightclub@mini@dance@dance_solo@male@var_a@", "low_center", 1, 300000)
        } else if (action == "dance6") {
            playAnimation("misschinese2_crystalmazemcs1_cs", "dance_loop_tao", 1, 300000)
        } else if (action == "dancesilly") {
            playAnimation("special_ped@mountain_dancer@monologue_3@monologue_3a", "mnt_dnc_buttwag", 1, 300000)
        } else if (action == "dancesilly4") {
            playAnimation("anim@amb@nightclub@lazlow@hi_podium@", "danceidle_hi_11_buttwiggle_b_laz", 1, 300000)
        } else if (action == "dancesilly5") {
            playAnimation("timetable@tracy@ig_5@idle_a", "idle_a", 1, 300000)
        } else if (action == "dance5") {
            playAnimation("anim@amb@casino@mini@dance@dance_solo@female@var_a@", "med_center", 1, 300000)
        } else if (action = 'close') {
            return resolve(game.clearPedTasks(alt.Player.local.scriptID));
        }
    }).then(() => {
        console.log("Fehler aufgetreten...")
    });
}

function InterActionMenuDoActionAnimationMenuPage3(action) {
    new Promise((resolve, reject) => {
        if (action == "injured") {
            playWalking("move_m@injured");
        } else if (action == "arrogant") {
            playWalking("move_f@arrogant@a");        
        } else if (action == "casual") {
            playWalking("move_m@casual@a"); 
        } else if (action == "casual4") {
            playWalking("move_m@casual@d");        
        } else if (action == "confident") {
            playWalking("move_m@confident"); 
        } else if (action == "drunk") {
            playWalking("move_m@drunk@a");
        } else if (action == "gangster") {
            playWalking("move_m@gangster@generic");
        } else if (action == "gangster2") {
            playWalking("move_m@gangster@ng");
        } else if (action == "cop") {
            playWalking("move_m@business@a");
        } else if (action == "cop2") {
            playWalking("move_m@business@b");
        } else if (action = 'close') {
            return resolve(playWalking("normal"));
        }
    }).then(() => {
        console.log("Fehler aufgetreten...")
    });
}

function InterActionMenuDoActionClothesRadialMenu(action) {
    new Promise((resolve, reject) => {
        alt.emitServer("Server:ClothesRadial:SetNormalSkin", action);
    }).then(() => {
        console.log("Fehler aufgetreten...")
    });
}

alt.everyTick(() => {
  if (game.isPedArmed(alt.Player.local.scriptID, 6)) {
      game.setPedSuffersCriticalHits(alt.Player.local.scriptID, false);
  }

    // 6 returns true if you are equipped with any weapon except melee weapons.
    if (game.isPedArmed(alt.Player.local.scriptID, 6)) {
        game.disableControlAction(0, 140, true); // INPUT_MELEE_ATTACK_LIGHT (R button - B on controller)
        game.disableControlAction(0, 141, true); // INPUT_MELEE_ATTACK_HEAVY (Q button - A on controller)
        game.disableControlAction(0, 142, true); // INPUT_MELEE_ATTACK_ALTERNATE (LMB - RT on controller)
    }

    if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) {
        game.disableControlAction(0, 24, true);
        game.disableControlAction(0, 25, true);
        game.disableControlAction(0, 12, true);
        game.disableControlAction(0, 13, true);
        game.disableControlAction(0, 14, true);
        game.disableControlAction(0, 15, true);
        game.disableControlAction(0, 16, true);
        game.disableControlAction(0, 17, true);
        game.disableControlAction(0, 37, true);
        game.disableControlAction(0, 44, true);
        game.disableControlAction(0, 45, true);
        game.disableControlAction(0, 263, true);
        game.disableControlAction(0, 264, true);
        game.disableControlAction(0, 140, true);
        game.disableControlAction(0, 141, true);
        game.disableControlAction(0, 257, true);
        game.disableControlAction(0, 345, true);
    } else {
        game.enableControlAction(0, 24, true);
        game.enableControlAction(0, 25, true);
        game.enableControlAction(0, 12, true);
        game.enableControlAction(0, 13, true);
        game.enableControlAction(0, 14, true);
        game.enableControlAction(0, 15, true);
        game.enableControlAction(0, 16, true);
        game.enableControlAction(0, 17, true);
        game.enableControlAction(0, 37, true);
        game.enableControlAction(0, 44, true);
        game.enableControlAction(0, 45, true);
        game.enableControlAction(0, 263, true);
        game.enableControlAction(0, 264, true);
        game.enableControlAction(0, 257, true);
        game.enableControlAction(0, 345, true);
    }
    if (hudBrowser == null) return;
    if (alt.Player.local.vehicle == null) return;
    const street = game.getStreetNameAtCoord(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z);
    const zoneName = game.getLabelText(game.getNameOfZone(alt.Player.local.pos.x, alt.Player.local.pos.y, alt.Player.local.pos.z));
    const streetName = game.getStreetNameFromHashKey(street[1]);
    hudBrowser.emit("CEF:HUD:updateStreetLocation", streetName + ", " + zoneName);
    GetVehicleSpeed();
    hudBrowser.emit("CEF:HUD:SetPlayerHUDVehicleSpeed", curSpeed);
    if (alt.Player.local.vehicle.model != 2621610858 && alt.Player.local.vehicle.model != 1341619767 && alt.Player.local.vehicle.model != 2999939664) {
        game.setPedConfigFlag(alt.Player.local.scriptID, 429, 1);
    } else {
        game.setPedConfigFlag(alt.Player.local.scriptID, 429, 0);
        game.setPedConfigFlag(alt.Player.local.scriptID, 35, false);
    }
    game.setPedConfigFlag(alt.Player.local.scriptID, 184, true);
    game.setDisableAmbientMeleeMove(alt.Player.local.scriptID, false);
    game.setAudioFlag("DisableFlightMusic", true);
    game.replaceHudColourWithRgba(143, 0, 217, 255, 255); /* Menu */
    game.replaceHudColourWithRgba(142, 0, 217, 255, 255); /* Marker */
});


//F�R WEAPONHANDLER
alt.onServer("Client:WeaponAmmoChange:ComingRespond", (weaponhash) => {
    if (alt.Player.local.currentWeapon != null && alt.Player.local.currentWeapon == weaponhash) {
        const ammo = game.getAmmoInPedWeapon(alt.Player.local.scriptID, weaponhash);
        alt.emitServer("Server:WeaponAmmoChange:SecondRespond", parseInt(ammo), parseInt(weaponhash));
        alt.emit("Client:HUD:sendNotification", 4, 4500, "Done: Waffe abgelegt! (+" + ammo + " Schuss)");
    }
    else {
        alt.emit("Client:HUD:sendNotification", 4, 4500, "Fehler: Du musst die Waffe in der Hand halten");
    }
});

//F�R TIMER
alt.onServer("Client:WeaponAmmoChange:ComingTimer", (weaponhash) => {
        if (alt.Player.local.currentWeapon != null && alt.Player.local.currentWeapon == weaponhash) {
            const ammo = game.getAmmoInPedWeapon(alt.Player.local.scriptID, weaponhash);
            alt.emitServer("Server:WeaponAmmoChange:ChangeRespond", parseInt(ammo), parseInt(weaponhash));
        }
});

let closeFarmingCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    alt.toggleGameControls(true);
    alt.showCursor(false);
    hudBrowser.unfocus();
}

let closeBankCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(game.playerPedId(), false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    BankAccountManageFormOpened = false;
}

let closeATMCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(game.playerPedId(), false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    ATMcefOpened = false;
}

let closeShopCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(game.playerPedId(), false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    ShopCefOpened = false;
}

let closeBarberCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    BarberCefOpened = false;
}

let closeGarageCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    GarageCefOpened = false;
}

let closeVehicleShopCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    VehicleShopCefOpened = false;
}

let closeJobcenterCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    JobcenterCefOpened = false;
}

let closeFuelstationCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    FuelStationCefOpened = false;
}

let closeClothesShopCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    ClothesShopCefOpened = false;
}

let closeClothesStorageCEF = function() {
    alt.emitServer("Server:ClothesShop:RequestCurrentSkin");
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    ClothesStorageCefOpened = false;
}

let closeBankFactionATMCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    bankFactionATMCefOpened = false;
}

let closeGivePlayerBillCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    GivePlayerBillCefOpened = false;
}

let closeRecievePlayerBillCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    RecievePlayerBillCefOpened = false;
}

let closeFactionStorageCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    FactionStorageCefOpened = false;
}

let closeVehicleTrunkCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    VehicleTrunkCefOpened = false;
}

let closeVehicleLicensingCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    VehicleLicensingCefOpened = false;
}

let closePlayerSearchCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    PlayerSearchInventoryCefOpened = false;
}

let closeGivePlayerLicenseCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    GivePlayerLicenseCefOpened = false;
}

let closeMinijobPilotCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    MinijobPilotCefOpened = false;
}

let closeMinijobBusdriverCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    MinijobBusdriverCefOpened = false;
}

let closeHotelRentCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    HotelRentCefOpened = false;
}

let closeHouseEntranceCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    HouseEntranceCefOpened = false;
}

let closeHouseManageCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    HouseManageCefOpened = false;
}

let destroyTownHallHouseSelector = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    game.freezeEntityPosition(alt.Player.local.scriptID, false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    TownhallHouseSelectorCefOpened = false;
}

let destroyAnimationMenu = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    AnimationMenuCefOpened = false;
}

let destroyClothesRadialMenu = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    alt.showCursor(false);
    alt.toggleGameControls(true);
    hudBrowser.unfocus();
    ClothesRadialCefOpened = false;
}


let closeTuningCEF = function() {
    alt.emitServer("Server:CEF:setCefStatus", false);
    alt.toggleGameControls(true);
    alt.showCursor(false);
    hudBrowser.unfocus();
    TuningMenuCefOpened = false;
    alt.emitServer("Server:Tuning:resetToNormal", curTuningVeh);
    curTuningVeh = null;
}

let closeAllCEFs = function() {
    if (hudBrowser == null) return;
    if (identityCardApplyCEFopened || TuningMenuCefOpened || BankAccountManageFormOpened || ATMcefOpened || ShopCefOpened || BarberCefOpened || GarageCefOpened || VehicleShopCefOpened || JobcenterCefOpened || FuelStationCefOpened || ClothesShopCefOpened || bankFactionATMCefOpened || GivePlayerBillCefOpened || FactionStorageCefOpened || RecievePlayerBillCefOpened || VehicleTrunkCefOpened || VehicleLicensingCefOpened || PlayerSearchInventoryCefOpened || GivePlayerLicenseCefOpened || MinijobPilotCefOpened || MinijobBusdriverCefOpened || HotelRentCefOpened || HouseEntranceCefOpened || HouseManageCefOpened || TownhallHouseSelectorCefOpened || AnimationMenuCefOpened) {
        hudBrowser.emit("CEF:General:hideAllCEFs");
        identityCardApplyCEFopened = false,
            BankAccountManageFormOpened = false,
            ATMcefOpened = false,
            ShopCefOpened = false,
            BarberCefOpened = false,
            GarageCefOpened = false,
            VehicleShopCefOpened = false,
            JobcenterCefOpened = false,
            FuelStationCefOpened = false,
            ClothesShopCefOpened = false,
            bankFactionATMCefOpened = false,
            GivePlayerBillCefOpened = false,
            FactionStorageCefOpened = false,
            RecievePlayerBillCefOpened = false,
            VehicleTrunkCefOpened = false,
            VehicleLicensingCefOpened = false,
            PlayerSearchInventoryCefOpened = false,
            GivePlayerLicenseCefOpened = false,
            MinijobPilotCefOpened = false,
            MinijobBusdriverCefOpened = false,
            HotelRentCefOpened = false,
            HouseEntranceCefOpened = false,
            HouseManageCefOpened = false,
            TownhallHouseSelectorCefOpened = false,
            TuningMenuCefOpened = false,
            AnimationMenuCefOpened = false,
            ClothesStorageCefOpened = false;
        alt.emitServer("Server:CEF:setCefStatus", false);
        alt.showCursor(false);
        alt.toggleGameControls(true);
        hudBrowser.unfocus();
    }
    closeInventoryCEF();
    closeTabletCEF();
}

function GetVehicleSpeed() {
    let vehicle = alt.Player.local.vehicle;
    let speed = game.getEntitySpeed(vehicle.scriptID);
    curSpeed = speed * 3.6;
}

/* */
function initializeFavouriteAnims() {
    if (hudBrowser != null && alt.Player.local.getSyncedMeta("IsCefOpen") == false && AnimationMenuCefOpened == false) {
        if (alt.Player.local.getSyncedMeta("HasHandcuffs") == true || alt.Player.local.getSyncedMeta("HasRopeCuffs") == true) return;
        var animStuff = {
            'Num1': alt.LocalStorage.get('Num1Hotkey'),
            'Num2': alt.LocalStorage.get('Num2Hotkey'),
            'Num3': alt.LocalStorage.get('Num3Hotkey'),
            'Num4': alt.LocalStorage.get('Num4Hotkey'),
            'Num5': alt.LocalStorage.get('Num5Hotkey'),
            'Num6': alt.LocalStorage.get('Num6Hotkey'),
            'Num7': alt.LocalStorage.get('Num7Hotkey'),
            'Num8': alt.LocalStorage.get('Num8Hotkey'),
            'Num9': alt.LocalStorage.get('Num9Hotkey')
        };
        hudBrowser.emit("CEF:Animations:setupAnimationMenu", animStuff);
        alt.emitServer("Server:CEF:setCefStatus", true);
        alt.showCursor(true);
        alt.toggleGameControls(false);
        hudBrowser.focus();
        AnimationMenuCefOpened = true;
    }
}

function playAnimation(animDict, animName, animFlag, animDuration) {
    if (animDict == undefined || animName == undefined || animFlag == undefined || animDuration == undefined) return;
    game.requestAnimDict(animDict);
    let interval = alt.setInterval(() => {
        if (game.hasAnimDictLoaded(animDict)) {
            alt.clearInterval(interval);
            game.taskPlayAnim(alt.Player.local.scriptID, animDict, animName, 8.0, 1, animDuration, animFlag, 1, false, false, false);
        }
    }, 10);
}

function playWalking(anim) {
    if (anim == undefined) return;
    if (anim == "normal") {
        game.resetPedMovementClipset(alt.Player.local.scriptID);
        return;
    }
    game.requestAnimSet(anim);
    let interval = alt.setInterval(() => {
        if (game.hasAnimDictLoaded(anim)) {
            alt.clearInterval(interval);
            game.setPedMovementClipset(alt.Player.local.scriptID, anim, 0.2);
        }
    }, 10);
}

//Tattoo Shop
alt.onServer("Client:TattooShop:openShop", (gender, shopId, ownTattoosJSON) => {
    if (hudBrowser == null || isTattooShopOpened) return;
    isTattooShopOpened = true;
    hudBrowser.emit("CEF:TattooShop:openShop", shopId, ownTattoosJSON);
    alt.showCursor(true);
    alt.toggleGameControls(false);
    hudBrowser.focus();
    if (gender == 0) {
        setClothes(alt.Player.local.scriptID, 11, 15, 0);
        setClothes(alt.Player.local.scriptID, 8, 15, 0);
        setClothes(alt.Player.local.scriptID, 3, 15, 0);
        setClothes(alt.Player.local.scriptID, 4, 21, 0);
        setClothes(alt.Player.local.scriptID, 6, 34, 0);
    } else {
        //ToDo
    }
});

alt.onServer("Client:TattooShop:sendItemsToClient", (items) => {
    if (hudBrowser == null) return;
    hudBrowser.emit("CEF:TattooShop:sendItemsToClient", items);
});
