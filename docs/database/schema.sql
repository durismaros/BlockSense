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
-- Table structure for table `activity_logs`
--

DROP TABLE IF EXISTS `activity_logs`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `activity_logs` (
  `log_id` bigint unsigned NOT NULL AUTO_INCREMENT COMMENT 'Unique log entry identifier, auto-incrementing primary key (supports billions of entries)',
  `actor_type` enum('user','system','cron') NOT NULL COMMENT 'Entity that performed action: user=authenticated user, system=internal process, cron=scheduled task',
  `actor_id` int unsigned DEFAULT NULL COMMENT 'User ID of actor if actor_type=user; NULL for system/api/cron actions',
  `action` varchar(255) NOT NULL COMMENT 'Stable action code in namespace format (e.g., "auth.success", "user.2fa.enabled", "device.revoked")',
  `context` json DEFAULT NULL COMMENT 'Flexible JSON metadata: IP address, user agent, device info, changed fields, error messages, etc.',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp when activity occurred, microsecond precision for precise ordering',
  PRIMARY KEY (`log_id`),
  KEY `idx_activity_logs_actor` (`actor_type`,`actor_id`) COMMENT 'Optimize queries for all actions by specific actor',
  KEY `idx_activity_logs_created_at` (`created_at`) COMMENT 'Optimize time-range queries and chronological sorting',
  KEY `idx_activity_logs_action` (`action`) COMMENT 'Optimize queries filtering by specific action type',
  KEY `fk_activity_logs_actor_user` (`actor_id`),
  CONSTRAINT `fk_activity_logs_actor_user` FOREIGN KEY (`actor_id`) REFERENCES `users` (`user_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Comprehensive activity audit log for user actions, system events, and security monitoring with flexible JSON metadata';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invitation_codes`
--

DROP TABLE IF EXISTS `invitation_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invitation_codes` (
  `invitation_id` int unsigned NOT NULL AUTO_INCREMENT COMMENT 'Unique invitation identifier, auto-incrementing primary key',
  `invitation_code` char(32) NOT NULL COMMENT 'Unique 32-character invitation code (alphanumeric, cryptographically random)',
  `generated_by` int unsigned NOT NULL COMMENT 'User ID of the account that generated this invitation code',
  `used_by` int unsigned DEFAULT NULL COMMENT 'User ID of the account that redeemed this code; NULL=unused',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp when invitation was generated, microsecond precision',
  `expires_at` datetime(6) NOT NULL COMMENT 'UTC timestamp when invitation expires and becomes invalid',
  `is_revoked` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Revocation flag: 0=active, 1=manually revoked (cannot be used)',
  PRIMARY KEY (`invitation_id`),
  UNIQUE KEY `uq_invitation_codes_code` (`invitation_code`),
  UNIQUE KEY `uq_invitation_codes_used_by` (`used_by`) COMMENT 'Ensures each user can only use one invitation code',
  KEY `idx_invitation_codes_generated_by` (`generated_by`) COMMENT 'Optimize queries for invitations created by specific user',
  KEY `idx_invitation_codes_expires_at` (`expires_at`) COMMENT 'Optimize queries for expired/valid invitation cleanup',
  KEY `idx_invitation_codes_is_revoked` (`is_revoked`) COMMENT 'Optimize queries filtering active vs revoked invitations',
  CONSTRAINT `fk_invitation_codes_generated_by` FOREIGN KEY (`generated_by`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_invitation_codes_used_by` FOREIGN KEY (`used_by`) REFERENCES `users` (`user_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1000 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Invitation-based registration system with expiration tracking, one-time use enforcement, and revocation support';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `refresh_tokens`
--

DROP TABLE IF EXISTS `refresh_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refresh_tokens` (
  `token_hash` varchar(255) NOT NULL COMMENT 'SHA-256 hash of refresh token value (never store plaintext tokens)',
  `user_id` int unsigned NOT NULL COMMENT 'User ID associated with this refresh token session',
  `ip_address` varchar(45) NOT NULL COMMENT 'IP address of login device (IPv4: 15 chars max, IPv6: 45 chars max)',
  `device_identifier` varchar(255) NOT NULL COMMENT 'Human-readable device label (e.g., "Chrome on Windows 11", "iPhone 15 Pro")',
  `device_os` varchar(255) NOT NULL COMMENT 'Operating system and version (e.g., "Windows 11 Pro 23H2", "macOS 14.2 Sonoma")',
  `hardware_fingerprint` char(44) NOT NULL COMMENT 'Unique hardware fingerprint, Base64-encoded (32-byte hash = 44 chars)',
  `network_fingerprint` char(17) NOT NULL COMMENT 'MAC address of network interface (format: XX:XX:XX:XX:XX:XX)',
  `issued_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp when refresh token was issued, microsecond precision',
  `expires_at` datetime(6) NOT NULL COMMENT 'UTC timestamp when token expires (typically 30-90 days from issuance)',
  `last_used_at` datetime(6) DEFAULT NULL COMMENT 'UTC timestamp of last token refresh/usage for activity tracking',
  `is_revoked` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Revocation flag: 0=active token, 1=manually revoked (logout, security event)',
  PRIMARY KEY (`token_hash`),
  UNIQUE KEY `uq_refresh_tokens_hardware_fingerprint` (`hardware_fingerprint`) COMMENT 'Enforce one active token per hardware device (prevents token proliferation)',
  KEY `idx_refresh_tokens_user_id` (`user_id`) COMMENT 'Optimize queries for all sessions belonging to a user',
  KEY `idx_refresh_tokens_expires_at` (`expires_at`) COMMENT 'Optimize queries for expired token cleanup jobs',
  KEY `idx_refresh_tokens_is_revoked` (`is_revoked`) COMMENT 'Optimize queries filtering active vs revoked tokens',
  KEY `idx_refresh_tokens_last_used_at` (`last_used_at`) COMMENT 'Optimize queries for detecting inactive sessions',
  CONSTRAINT `fk_refresh_tokens_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Secure refresh token registry with device fingerprinting and session management. One active token per hardware device.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `two_factor_auth`
--

DROP TABLE IF EXISTS `two_factor_auth`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `two_factor_auth` (
  `user_id` int unsigned NOT NULL COMMENT 'User ID for this 2FA configuration (one-to-one relationship)',
  `encrypted_totp_secret` varbinary(48) NOT NULL COMMENT '160-bit TOTP secret encrypted with AES-GCM-256 (12-byte nonce + 20-byte ciphertext + 16-byte auth tag)',
  `backup_codes` json DEFAULT NULL COMMENT 'Array of hashed backup codes for 2FA recovery (Argon2 or bcrypt hashed)',
  `backup_codes_generated_at` datetime(6) DEFAULT NULL COMMENT 'UTC timestamp when backup codes were last generated',
  `enabled_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp when 2FA was first enabled for this user',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `uq_two_factor_auth_encrypted_secret` (`encrypted_totp_secret`) COMMENT 'Ensure TOTP secrets are unique across all users',
  KEY `idx_two_factor_auth_enabled_at` (`enabled_at`) COMMENT 'Optimize queries for 2FA adoption analytics',
  CONSTRAINT `fk_two_factor_auth_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Two-factor authentication configuration with AES-GCM-256 encrypted TOTP secrets and hashed backup recovery codes';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `user_id` int unsigned NOT NULL AUTO_INCREMENT COMMENT 'Unique user identifier, auto-incrementing primary key',
  `username` varchar(32) NOT NULL COMMENT 'Unique username for login, 3-32 alphanumeric characters',
  `email` varchar(256) NOT NULL COMMENT 'Unique email address, validated format, used for notifications',
  `user_type` enum('standard','administrator','founder','banned') NOT NULL DEFAULT 'standard' COMMENT 'User role: standard=regular user, administrator=elevated privileges, founder=system owner, banned=access revoked',
  `password_hash` binary(32) NOT NULL COMMENT 'Cryptographic password hash (Argon2id recommended, SHA-256 compatible)',
  `password_salt` binary(16) NOT NULL COMMENT 'Random 128-bit cryptographic salt for password hashing',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp of account creation, microsecond precision',
  `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp of last account modification, auto-updated on changes',
  `deleted_at` datetime(6) DEFAULT NULL COMMENT 'Soft delete timestamp; NULL=active account, non-NULL=deleted account (enables data recovery)',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `uq_users_username` (`username`),
  UNIQUE KEY `uq_users_email` (`email`),
  KEY `idx_users_user_type` (`user_type`) COMMENT 'Optimize queries filtering by user role',
  KEY `idx_users_created_at` (`created_at`) COMMENT 'Optimize queries sorting/filtering by registration date',
  KEY `idx_users_deleted_at` (`deleted_at`) COMMENT 'Optimize soft-delete queries (active users have NULL)'
) ENGINE=InnoDB AUTO_INCREMENT=1000 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Core user account registry with authentication credentials, role management, and soft delete support';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-03 21:17:41
