import alt from "alt"
import native from "natives"
import { Menu, UIMenuItem, UIMenuListItem, UIMenuCheckboxItem, UIMenuSliderItem, BadgeStyle, Point, ItemsCollection, Color, ListItem } from "./nativeui/nativeui.min.js"

const configFile = "/client/config.json"
const keysForBindingDisplay = ["-"]
const keysForBinding = [0]
const menus = {}
let keyBindings = {}
let config = null
let selectedIndex = 0
let selectedKey = 0
let lastAnimation = null

const loadConfig = () => {
    const exists = alt.File.exists(configFile)

    if (!exists) {
        alt.logError("[Animations Menü] Datei config.json existiert im client Order nicht!")
        return false
    }
    
    try {
        config = JSON.parse(alt.File.read(configFile))
    } catch (error) {
        alt.logError("[Animations Menü] Fehler beim laden der config.json! (Format richtig?)")
        return false
    }

    return true
}

const createMenu = () => {
    const mainMenu = new Menu("Animationen", "Wähle eine Kategorie", new Point(50, 50))
    mainMenu.AddItem(new UIMenuItem("Animation beenden", "Beendet die Animation", 0))

    for (const index in config.animations) {
        const animation = config.animations[index]
        let savedIndex = 0

        for (const key in keyBindings) {
            if (index != keyBindings[key]) continue
                
            for (const i in keysForBinding) {
                if (keysForBinding[i] != key) continue

                savedIndex = i
                break
            }
            break
        }

        if (animation[5] == undefined)
            continue

        let animationMenu = menus[animation[5]]
        if (animationMenu == undefined) {
            animationMenu = new Menu("Animationen", "Spiele eine Animation ab", new Point(50, 50))
            animationMenu.AddItem(new UIMenuListItem(animation[0], animation[1], new ItemsCollection(keysForBindingDisplay), savedIndex, index))
            menus[animation[5]] = animationMenu

            const categorieItem = new UIMenuItem(animation[5])
            mainMenu.AddItem(categorieItem)
            mainMenu.AddSubMenu(animationMenu, categorieItem)

            animationMenu.ItemSelect.on(async (item, index) => {
                animationMenu.Close()
                const animation = config.animations[item.Data]
                await playAnimation(animation)
            })
        
            animationMenu.ListChange.on((item, index) => {
                selectedIndex = item.Data
                selectedKey = keysForBinding[index]
            })

            mainMenu.ItemSelect.on((item, index) => {
                if (item.Data == 0) {
                    mainMenu.Close()
                    playAnimation(null)    
                }
            })
        } else {
            animationMenu.AddItem(new UIMenuListItem(animation[0], animation[1], new ItemsCollection(keysForBindingDisplay), savedIndex, index))
        }
    }

    alt.on("keydown", async key => {
        if (key == config.openKey) {
            if (mainMenu.Visible) {
                mainMenu.Close() //wenn submenu auf und f5 dann geht main menu auf
            }
            else {
                mainMenu.Open()
                for (const k in menus) {
                    menus[k].Close()
                }
            }
        } else if (key == config.saveKey) {
            saveAnimation(selectedKey, selectedIndex)
            notify("Animation ~b~gespeichert")
        } else {
            const index = keyBindings[key]
            if (!index) {
                return
            }

            const animation = config.animations[index]
            if (animation) {
                await playAnimation(animation)
            }
        }
    })
}

const notify = message => {
    native.beginTextCommandThefeedPost("STRING")
    native.addTextComponentSubstringPlayerName(message)
    native.endTextCommandThefeedPostTicker(false, false)
}

const saveAnimation = (key, index) => {
    for (const k in keyBindings) {
        if (keyBindings[k] == index) {
            delete keyBindings[k]
        }
    }

    keyBindings[key] = index
    alt.LocalStorage.get().set("keyBindings", keyBindings)
    alt.LocalStorage.get().save()
}

const loadKeyBindings = () => {
    let bindings = alt.LocalStorage.get().get("keyBindings")

    if (bindings != null) {
        keyBindings = bindings
    }
    
    for (let i = 0; i < 10; i++) {
        keysForBinding.push([0x60 + i])
        keysForBindingDisplay.push(i)
    }
}

const initDebugCommands = () => {
    alt.on("consoleCommand", async (name, ...args) => {
        if (name == "anim") {
            await playAnimation(["", "", args[0], args[1], args[2]])
            notify("Animation ~g~gestartet")
        } else if (name == "stop") {
            native.clearPedTasks(alt.Player.local.scriptID)
            notify("Animation ~r~gestoppt")
        }
    })
}

const playAnimation = async animation => {
    if (lastAnimation) {
        native.stopAnimTask(alt.Player.local.scriptID, lastAnimation[2], lastAnimation[3], lastAnimation[4])
    }

    if (animation == null) {
        notify("Animation ~r~gestoppt")
        return
    }

    await loadAnimation(animation[2])
    native.taskPlayAnim(alt.Player.local.scriptID, animation[2], animation[3], 8, -4, -1, animation[4], 0, false, false, false)
    native.removeAnimDict(animation[2])
    lastAnimation = animation
    notify("Animation ~g~gestartet")
}

const loadAnimation = async (animDict) => {
    return new Promise((resolve, reject) => {
        native.requestAnimDict(animDict)

        const interval = alt.setInterval(() => {
            if (native.hasAnimDictLoaded(animDict)) {
                alt.clearInterval(interval)
                return resolve(true)
            }
        }, 0)
    })
}

const loaded = loadConfig()

if (loaded) {
    loadKeyBindings()
    createMenu()

    if (config.debug) {
        alt.logWarning("[Animations Menü] Debug Modus ist an!")
        initDebugCommands()
    }
}