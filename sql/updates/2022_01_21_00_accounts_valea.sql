ALTER TABLE `accounts`
	CHANGE COLUMN `whitelisted` `whitelisted` TINYINT(1) NOT NULL DEFAULT '1' AFTER `online`,
	CHANGE COLUMN `ban` `ban` TINYINT(1) NOT NULL DEFAULT '0' AFTER `whitelisted`,
	CHANGE COLUMN `banReason` `banReason` VARCHAR(128) NOT NULL DEFAULT '' COLLATE 'utf8_general_ci' AFTER `ban`;
