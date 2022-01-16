-- Added ShowBlip
ALTER TABLE `server_atm`
	ADD COLUMN `showBlip` INT(11) NOT NULL DEFAULT '1' AFTER `isrobbed`;
	
ALTER TABLE `server_storages`
	ADD COLUMN `showBlip` INT(11) NOT NULL DEFAULT '1' AFTER `factionid`;