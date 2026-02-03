-- BlockSense Database Schema
-- MySQL dump 10.13  Distrib 8.0.40, for Win64 (x86_64)
--
-- Host: localhost    Database: blocksense
-- ------------------------------------------------------
-- Server version	8.0.40

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Create database
--

CREATE DATABASE IF NOT EXISTS `blocksense` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `blocksense`;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `user_id` int unsigned NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  `user_type` enum('standard','administrator','founder','banned') NOT NULL DEFAULT 'standard',
  `password_hash` binary(32) NOT NULL COMMENT 'SHA-256 or Argon2 hash',
  `password_salt` binary(16) NOT NULL COMMENT 'Random 128-bit salt',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  `deleted_at` datetime(6) DEFAULT NULL COMMENT 'Soft delete timestamp; NULL = active account',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `uq_users_username` (`username`),
  UNIQUE KEY `uq_users_email` (`email`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Stores user account information and metadata.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invitation_codes`
--

DROP TABLE IF EXISTS `invitation_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invitation_codes` (
  `invitation_id` int unsigned NOT NULL AUTO_INCREMENT,
  `invitation_code` char(32) NOT NULL,
  `generated_by` int unsigned NOT NULL COMMENT 'User ID who generated this code',
  `used_by` int unsigned DEFAULT NULL,
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `expires_at` datetime(6) NOT NULL,
  `is_revoked` tinyint(1) NOT NULL DEFAULT '0' COMMENT '0 = active, 1 = revoked',
  PRIMARY KEY (`invitation_id`),
  UNIQUE KEY `uq_invitation_codes_code` (`invitation_code`),
  UNIQUE KEY `used_by_UNIQUE` (`used_by`),
  KEY `idx_invitation_codes_expires_at` (`expires_at`),
  KEY `fk_invitation_codes_generated_by_idx` (`generated_by`),
  CONSTRAINT `fk_invitation_codes_generated_by` FOREIGN KEY (`generated_by`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_invitation_codes_used_by` FOREIGN KEY (`used_by`) REFERENCES `users` (`user_id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Stores invitation codes for user registration or access control.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `refresh_tokens`
--

DROP TABLE IF EXISTS `refresh_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refresh_tokens` (
  `token_hash` varchar(255) NOT NULL COMMENT 'Hashed refresh token value (SHA-256)',
  `user_id` int unsigned NOT NULL,
  `ip_address` varchar(45) NOT NULL COMMENT 'IPv4 or IPv6 address of login device',
  `device_identifier` varchar(255) NOT NULL COMMENT 'User-friendly device label (e.g., "Windows 11 PC")',
  `device_os` varchar(150) NOT NULL COMMENT 'Operating system of the device (e.g., Windows 11, macOS 14)',
  `hardware_fingerprint` char(44) NOT NULL COMMENT 'Unique hardware fingerprint (Base64-encoded)',
  `network_fingerprint` char(17) NOT NULL COMMENT 'Hardware-level network identifier represented by the device MAC address.',
  `issued_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `expires_at` datetime(6) NOT NULL,
  `is_revoked` tinyint(1) NOT NULL DEFAULT '0' COMMENT '0 = active, 1 = revoked',
  PRIMARY KEY (`token_hash`),
  UNIQUE KEY `uq_refresh_token_hash` (`token_hash`),
  UNIQUE KEY `hardware_fingerprint_UNIQUE` (`hardware_fingerprint`),
  KEY `idx_refresh_tokens_user_id` (`user_id`),
  KEY `idx_refresh_tokens_expires_at` (`expires_at`),
  CONSTRAINT `fk_refresh_tokens_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Tracks refresh tokens and device sessions. Each hardware/network combination has one active token.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `two_factor_auth`
--

DROP TABLE IF EXISTS `two_factor_auth`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `two_factor_auth` (
  `user_id` int unsigned NOT NULL,
  `encrypted_totp_secret` varbinary(48) NOT NULL COMMENT '160-bit TOTP secret encrypted with AES-GCM-256 (nonce + ciphertext + tag)',
  `backup_codes` json DEFAULT NULL COMMENT 'Array of hashed backup codes for recovery',
  `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `encrypted_totp_secret_UNIQUE` (`encrypted_totp_secret`),
  CONSTRAINT `fk_two_factor_auth_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Stores AES-GCM-256 encrypted 160-bit TOTP secrets and 2FA backup codes per user.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Restore settings
--

/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed

