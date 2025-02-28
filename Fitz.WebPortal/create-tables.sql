-- Create lotteries table if it doesn't exist
CREATE TABLE IF NOT EXISTS `lotteries` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `PrizePool` int NOT NULL,
    `StartDate` datetime(6) NOT NULL,
    `DrawDate` datetime(6) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `WinnerId` bigint unsigned NULL,
    CONSTRAINT `PK_lotteries` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

-- Create lottery_entries table if it doesn't exist
CREATE TABLE IF NOT EXISTS `lottery_entries` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `LotteryId` int NOT NULL,
    `AccountId` bigint unsigned NOT NULL,
    `EntryDate` datetime(6) NOT NULL,
    CONSTRAINT `PK_lottery_entries` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_lottery_entries_lotteries_LotteryId` FOREIGN KEY (`LotteryId`) REFERENCES `lotteries` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

-- Create transactions table if it doesn't exist
CREATE TABLE IF NOT EXISTS `transactions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `SenderId` bigint unsigned NOT NULL,
    `RecipientId` bigint unsigned NOT NULL,
    `Amount` int NOT NULL,
    `Reason` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Timestamp` datetime(6) NOT NULL,
    CONSTRAINT `PK_transactions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

-- Create EF migrations history table if it doesn't exist
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

-- Insert the migration record to prevent future migration attempts
INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250228010252_InitialCreate', '8.0.0'); 