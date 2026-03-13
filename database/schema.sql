CREATE DATABASE  IF NOT EXISTS `blocksense` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `blocksense`;
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
  `id` bigint unsigned NOT NULL AUTO_INCREMENT COMMENT 'Primary key. BIGINT UNSIGNED supports ~1.8×10¹⁹ rows, sufficient for high-volume deployments across long-lived systems without overflow.',
  `type` enum('user','system','cron') NOT NULL COMMENT 'Category of the actor that triggered the event. user = authenticated human; system = autonomous internal process; cron = scheduled background job.',
  `user_id` int unsigned NOT NULL COMMENT 'FK -> users.id. ON DELETE CASCADE preserves log integrity; rows are retained when the referenced account is deleted to maintain the complete audit history for compliance and forensic purposes.',
  `action` varchar(255) NOT NULL COMMENT 'Stable dot-namespaced event code identifying what occurred, e.g. "profile.picture.changed". Must not be renamed or reused after deployment; downstream alerting rules and compliance exports depend on exact string matches.',
  `context` json DEFAULT NULL COMMENT 'Flexible JSON object containing action-specific metadata. Schema varies by action type; common fields include ip, user_agent, device, old_role, new_role, and initiated_by. NULL when no additional context is relevant. Must never contain sensitive material such as plaintext passwords, raw tokens, or cryptographic secrets.',
  `occurred_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp of the event, microsecond precision. The combination of AUTO_INCREMENT id and microsecond resolution provides unambiguous ordering even for rapid-fire entries within the same second.',
  PRIMARY KEY (`id`),
  KEY `idx_activity_logs_actor` (`type`,`user_id`) COMMENT 'Composite index on (type, user_id). Satisfies the most common audit query pattern: all actions performed by a specific user (type = "user" AND user_id = ?). The leading type column also efficiently covers queries that filter by actor category alone.' /*!80000 INVISIBLE */,
  KEY `idx_activity_logs_action` (`action`) COMMENT 'Supports filtering and aggregating by specific event code, e.g. counting failed login attempts within an alerting window. Also used by security monitoring pipelines that subscribe to specific action namespaces.',
  KEY `idx_activity_logs_occurred_at` (`occurred_at`) COMMENT 'Supports chronological retrieval, time-range filtering, and range-based archival. Because occurred_at is monotonically increasing in an append-only table, InnoDB satisfies ORDER BY occurred_at DESC with minimal sort overhead.' /*!80000 INVISIBLE */,
  KEY `fk_activity_logs_user` (`user_id`),
  CONSTRAINT `fk_activity_logs_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=86 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Append-only audit log for all user, system, and cron events. Rows must never be updated or deleted in production. Supports security auditing, compliance reporting, and incident investigation.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `invitation_codes`
--

DROP TABLE IF EXISTS `invitation_codes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `invitation_codes` (
  `id` int unsigned NOT NULL AUTO_INCREMENT COMMENT 'Primary key. Auto-incrementing unsigned integer.',
  `code` char(32) NOT NULL COMMENT 'Invitation code presented during registration. Fixed-length 32-character alphanumeric string generated via CSPRNG to ensure unpredictability.',
  `issued_to_id` int unsigned NOT NULL COMMENT 'FK -> users.id. The account this invitation was issued to. ON DELETE CASCADE removes outstanding codes if the holder account is deleted.',
  `redeemed_by_id` int unsigned DEFAULT NULL COMMENT 'FK -> users.id. The account that redeemed this code at registration. NULL = code is still available. UNIQUE constraint enforces one redemption per account. ON DELETE SET NULL retains the row for audit purposes.',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp of code generation, microsecond precision. Set once at INSERT; never subsequently modified.',
  `expires_at` datetime(6) NOT NULL COMMENT 'UTC timestamp after which the code must be rejected, microsecond precision. Checked by the application layer at redemption time.',
  `is_revoked` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Revocation flag. 0 = active; 1 = manually revoked. Revoked codes must be rejected regardless of expiry or redemption status.',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_invitation_codes_code` (`code`),
  UNIQUE KEY `uq_invitation_codes_redeemed_by_id` (`redeemed_by_id`),
  KEY `idx_invitation_codes_issued_to_id` (`issued_to_id`) COMMENT 'Supports queries that retrieve all invitations issued to a specific account.',
  KEY `idx_invitation_codes_expires_at` (`expires_at`) COMMENT 'Supports cleanup jobs and reporting on upcoming or past expirations.',
  KEY `idx_invitation_codes_is_revoked` (`is_revoked`) COMMENT 'Supports filtering active vs. revoked codes.',
  CONSTRAINT `fk_invitation_codes_issued_to_id` FOREIGN KEY (`issued_to_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_invitation_codes_redeemed_by_id` FOREIGN KEY (`redeemed_by_id`) REFERENCES `users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1027 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Invite-only registration codes. Each code is single-use, time-limited, and revocable. UNIQUE on redeemed_by_id enforces one redemption per account.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `refresh_tokens`
--

DROP TABLE IF EXISTS `refresh_tokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `refresh_tokens` (
  `token_hash` varchar(255) NOT NULL COMMENT 'SHA-256 hash of the raw refresh token value, base64-encoded. The plaintext token is issued to the client and never persisted. On validation, the presented token is hashed and compared against this column. VARCHAR(255) accommodates 64-character base64 SHA-256 output and leaves room for future algorithm migrations without a schema change.',
  `user_id` int unsigned NOT NULL COMMENT 'FK -> users.id. The account to which this session belongs. ON DELETE CASCADE removes all associated tokens when a user is deleted, terminating every active session for that account.',
  `ip_address` varchar(45) NOT NULL COMMENT 'IP address of the client at token issuance time. VARCHAR(45) accommodates IPv4 (max 15 chars, e.g. "203.0.113.42") and IPv6 (max 45 chars, e.g. "2001:0db8:85a3:0000:0000:8a2e:0370:7334"). Stored for security auditing and anomaly detection; not used for token validation.',
  `device_identifier` varchar(255) NOT NULL COMMENT 'Human-readable label identifying the client device, composed at the application layer from browser user-agent parsing or OS APIs. Examples: "User''s MacBook Air", "DESKTOP-LP0IJOQ". Displayed in the active-sessions UI to help users identify and revoke specific sessions.',
  `device_os` varchar(255) NOT NULL COMMENT 'Operating system name and version string at the time of login. Examples: "macOS 14.2 Sonoma", "Windows 11 Pro 23H2". Stored for security auditing and session display; not used for token validation.',
  `hardware_fingerprint` char(44) NOT NULL COMMENT 'Base64-encoded SHA-256 hash (32 raw bytes = 44 Base64 chars) derived from stable hardware attributes such as CPU identifier, primary disk serial number, and motherboard UUID. CHAR(44) because all valid values are exactly 44 characters. The UNIQUE constraint enforces the one-active-token-per-device policy. A changed fingerprint forces re-authentication on that device.',
  `network_fingerprint` char(17) NOT NULL COMMENT 'MAC address of the primary network interface at login time, in IEEE 802 colon-hexadecimal notation (e.g. "A1:B2:C3:D4:E5:F6"). CHAR(17) because all valid MAC addresses are exactly 17 characters. Stored for forensic and audit context only; not used for session validation or uniqueness enforcement, as MAC addresses may change with VPN usage or NIC replacement.',
  `issued_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp when this token was issued, microsecond precision. Set once at INSERT; never subsequently modified. Used to calculate token age and populate session history in the user interface.',
  `expires_at` datetime(6) NOT NULL COMMENT 'UTC timestamp after which this token must be rejected, microsecond precision. The application layer must refuse tokens presented after this time even if is_revoked = 0. A background cleanup job should periodically purge rows where expires_at < NOW() to prevent unbounded table growth.',
  `is_revoked` tinyint(1) NOT NULL DEFAULT '0' COMMENT 'Revocation flag. 0 = token is active and eligible for use; 1 = token has been explicitly invalidated via logout, administrator action, or a security event such as detected compromise. Revoked tokens must be rejected by validation logic regardless of expiry timestamp. Stored as TINYINT(1) for ORM boolean compatibility.',
  PRIMARY KEY (`token_hash`),
  UNIQUE KEY `uq_refresh_tokens_hardware_fingerprint` (`hardware_fingerprint`),
  KEY `idx_refresh_tokens_user_id` (`user_id`) COMMENT 'Supports retrieval of all sessions belonging to a specific user, which is the primary query for the active-sessions management UI. Also satisfies the InnoDB foreign key lookup during UPDATE and DELETE operations on the users parent table.',
  KEY `idx_refresh_tokens_expires_at` (`expires_at`) COMMENT 'Supports scheduled cleanup jobs that purge or archive expired tokens: DELETE FROM refresh_tokens WHERE expires_at < NOW(). Range scans on this column are efficient due to the monotonically increasing nature of expiry timestamps for tokens issued with a fixed TTL.',
  KEY `idx_refresh_tokens_is_revoked` (`is_revoked`) COMMENT 'Supports queries that distinguish active from revoked sessions, e.g. counting live sessions or bulk-revoking all tokens for a user. Most effective when combined with the user_id index in composite queries.',
  CONSTRAINT `fk_refresh_tokens_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Device-bound refresh token sessions. Token values are stored as SHA-256 hashes only; plaintext is never persisted. The UNIQUE constraint on hardware_fingerprint enforces exactly one active token per physical device.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `totp_credentials`
--

DROP TABLE IF EXISTS `totp_credentials`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `totp_credentials` (
  `user_id` int unsigned NOT NULL COMMENT 'FK -> users.id. Identifies the account for which TOTP 2FA is configured. Serves as both the primary key and the foreign key, enforcing a strict one-to-one relationship. Row presence implies 2FA is enabled; row absence implies 2FA is disabled.',
  `encrypted_secret` varbinary(48) NOT NULL COMMENT 'AES-256-GCM encrypted 160-bit TOTP secret (RFC 6238). Binary layout: bytes 0–11 = 96-bit GCM nonce (randomly generated per write); bytes 12–31 = 20-byte ciphertext of the raw TOTP seed; bytes 32–47 = 128-bit GCM authentication tag. The encryption key is stored outside the database. VARBINARY prevents charset conversion of raw binary data.',
  `backup_codes` json DEFAULT NULL COMMENT 'JSON array of SHA-256 hashed single-use recovery codes (base64-encoded), e.g. ["<sha256_base64>", ...]. Plaintext codes are issued to the user and never persisted. On successful use, the matching hash is removed from the array and the row is updated. NULL or an empty array indicates no remaining backup codes.',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp when 2FA was first enabled for this account, microsecond precision. Set once at INSERT; never subsequently modified.',
  `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp of the most recent modification to this row, microsecond precision. Updated when the TOTP secret is rotated or when backup codes are consumed or regenerated. Automatically maintained by InnoDB on every UPDATE.',
  PRIMARY KEY (`user_id`),
  UNIQUE KEY `uq_totp_credentials_encrypted_secret` (`encrypted_secret`),
  KEY `idx_totp_credentials_created_at` (`created_at`) COMMENT 'Supports 2FA adoption analytics and time-range queries over enablement date, e.g. counting accounts that enabled 2FA within a given month.',
  CONSTRAINT `fk_totp_credentials_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='TOTP two-factor authentication configuration. One row per user; row presence implies 2FA is enabled. Stores AES-256-GCM encrypted TOTP secrets and SHA-256 hashed single-use backup recovery codes.';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` int unsigned NOT NULL AUTO_INCREMENT COMMENT 'Primary key. Auto-incrementing unsigned integer.',
  `username` varchar(32) NOT NULL COMMENT 'Unique login handle. 3–32 characters, alphanumeric only, enforced at the application layer. Uniqueness is case-insensitive via the utf8mb4_0900_ai_ci collation.',
  `email` varchar(256) NOT NULL COMMENT 'Unique email address. 256 characters covers practical RFC 5321 limits. Format validation is enforced at the application layer.',
  `role` enum('standard','administrator','founder','banned') NOT NULL DEFAULT 'standard' COMMENT 'Access role. standard = no elevation; administrator = platform management; founder = highest privilege; banned = account prohibited, must be rejected at login before any token issuance.',
  `password_hash` binary(32) NOT NULL COMMENT 'Raw Argon2id (RFC 9106) hash bytes of the user password. 32-byte output, compatible with SHA-256 output length. BINARY type prevents charset or collation interference with raw byte values. Plaintext and hex-encoded strings must never be stored here.',
  `password_salt` binary(16) NOT NULL COMMENT 'Cryptographically random 128-bit (16-byte) salt, unique per account, generated via CSPRNG. Required for future password verification.',
  `created_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp of account creation, microsecond precision. Set once at INSERT; never subsequently modified.',
  `updated_at` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6) COMMENT 'UTC timestamp of the most recent modification to this row, microsecond precision. Automatically maintained by InnoDB on every UPDATE.',
  `deleted_at` datetime(6) DEFAULT NULL COMMENT 'Soft-delete marker. NULL = account is active. Non-NULL = UTC timestamp at which the account was logically deleted. Hard DELETE is intentionally avoided to preserve referential integrity across dependent tables and to allow account recovery.',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_username` (`username`),
  UNIQUE KEY `uq_users_email` (`email`),
  KEY `idx_users_role` (`role`) COMMENT 'Supports role-based filtering, e.g. fetching all banned or all administrator accounts.',
  KEY `idx_users_created_at` (`created_at`) COMMENT 'Supports paginated registration timelines and time-range queries over account creation date. Also used by scheduled jobs that process newly registered accounts within a given window.',
  KEY `idx_users_deleted_at` (`deleted_at`) COMMENT 'Supports soft-delete filtering. Active-user queries use WHERE deleted_at IS NULL; deletion audit queries use WHERE deleted_at IS NOT NULL. InnoDB stores NULL values in the index, making both forms efficient.'
) ENGINE=InnoDB AUTO_INCREMENT=1027 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Master user account registry. Stores authentication credentials, access roles, and soft-delete state. Every other table in this schema depends on this table either directly or transitively.';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-03-13 12:00:23
