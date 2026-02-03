# 💼 Desktop Cryptocurrency Wallet

<div align="center">

![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)
![Avalonia](https://img.shields.io/badge/Avalonia-11.0-8B44AC)

**A secure and reliable desktop cryptocurrency wallet application for managing digital assets.**

Built with modern technologies and cryptography best practices for safe handling of sensitive data.

[Features](#-features) • [Architecture](#-architecture--technologies) • [Security](#-security) • [Installation](#-setup--installation) • [Contributing](#-contributing)

</div>

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
  - [Functional Features](#functional-features)
  - [Non-Functional Requirements](#non-functional-requirements)
- [Architecture & Technologies](#-architecture--technologies)
- [Security](#-security)
- [User Interface](#-user-interface)
- [Setup & Installation](#-setup--installation)
- [Screenshots](#-screenshots)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🛠 Overview

The **Desktop Cryptocurrency Wallet** enables users to securely manage digital currencies and accounts with enterprise-grade security and an intuitive interface.

### Components

- **Desktop Client:** Built with [Avalonia UI Framework](https://avaloniaui.net/) for a responsive and cross-platform experience
- **Backend API:** Powered by **ASP.NET Core Web API** with **MySQL** for secure authentication and wallet operations

### Project Goals

- Implement secure authentication & authorization with 2FA support
- Enable wallet creation, import, and management
- Provide reliable transaction processing and balance tracking
- Maintain an intuitive, user-friendly interface
- Apply cryptography best practices to protect user data

---

## ⚡ Features

### Functional Features

#### 👤 User Management

- **Registration** via email, password, and valid invitation code
- **Invitation validation** before account creation
- **Login/Logout** with optional 2FA support
- **Password reset** via email recovery

#### 🔐 Authentication & Authorization

- **TOTP (Time-based One-Time Password)** support with Google Authenticator
- **JWT authentication** for secure API communication
- **Refresh tokens** for persistent, secure sessions

#### 💰 Wallet Management

- **Create new wallets** or **import** using 12-word seed phrases
- **Local encryption** of seed phrases and private keys
- **Single active wallet** per user for simplified management

#### 📤 Transactions

- **Send and receive** cryptocurrencies: **Bitcoin, Ethereum, Litecoin**
- **Transaction confirmation** with amount and recipient verification
- **Transaction history** with real-time balance updates

#### 🛡️ Data Security

- All sensitive data encrypted locally
- Zero transmission of sensitive information to servers

### Non-Functional Requirements

- **Encryption:** AES-256 GCM/CBC for data, Argon2Id for password hashing
- **Brute-force protection:** 30-second login timeout
- **Performance:** API latency < 500ms
- **Platform:** Windows-only support
- **Logging:** Comprehensive user activity logging (local and remote)
- **UX:** Intuitive, clear, and beginner-friendly interface

---

## 🏗 Architecture & Technologies

### Technology Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Avalonia UI (C#) |
| **Backend** | ASP.NET Core Web API |
| **Database** | MySQL |
| **Local Storage** | LevelDB (encrypted wallets) |
| **Authentication** | JWT & TOTP (Google Authenticator) |
| **Cryptography** | AES-256, Argon2Id |
| **Version Control** | Git |

### Architecture

The application follows a **client-server architecture** that cleanly separates the UI layer from backend logic, prioritizing:

- 🔒 **Security** - Zero-knowledge architecture with client-side encryption
- 🎯 **Reliability** - Robust error handling and transaction verification
- 🔧 **Maintainability** - Modular design with clear separation of concerns

---

## 🔐 Security

Security is the cornerstone of this application:

- 🔑 **Local Encryption:** All sensitive data (seed phrases, private keys) encrypted locally before storage
- 🔐 **Password Security:** Argon2Id hashing for password protection
- 🎫 **JWT Authentication:** Secure token-based API authentication
- 📱 **2FA Support:** TOTP-based two-factor authentication
- 🛡️ **Brute-Force Protection:** Rate limiting and login timeouts
- 🚫 **Zero Knowledge:** No sensitive information transmitted over the network

---

## 🎨 User Interface

The application provides a **clean, modern, and accessible interface**:

- 📝 Easy account registration and login flow
- 💼 Clear wallet management controls
- 📊 Transaction history with detailed balance display
- 📚 Step-by-step guidance for new users
- 🎯 Intuitive navigation and workflow

---

## ⚡ Setup & Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MySQL Server](https://dev.mysql.com/downloads/mysql/)
- Windows OS

### 1. Clone the Repository

```bash
git clone https://github.com/durismaros/BlockSense.git
cd BlockSense
```

### 2. Configure Backend

#### Set up MySQL database

```bash
# Create the database
mysql -u root -p
CREATE DATABASE blocksense;
USE blocksense;
```

Run the database schema script located in the repository:

```bash
# Execute the schema setup script
mysql -u root -p blocksense < docs/database/schema.sql
```

> **Note:** The database schema includes tables for users, invitation codes, refresh tokens, and two-factor authentication. See `docs/database/schema.sql` for the complete structure.

#### Configure secrets using .NET User Secrets

Connection strings and sensitive configuration are managed via **dotnet user-secrets** (not `appsettings.json`).

Navigate to your backend project directory and configure the required secrets:

```bash
cd src/BlockSense.Backend

# Set database connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=blocksense;User=root;Password=yourpassword;"

# Set JWT signing key (use a strong random key)
dotnet user-secrets set "JwtTokenConfig:SigningKey" "your-secure-signing-key-here"

# Set 2FA master key (use a strong random key)
dotnet user-secrets set "TwoFactorAuthConfig:MasterKey" "your-secure-master-key-here"
```

**Required secrets:**
- `ConnectionStrings:DefaultConnection` - MySQL connection string
- `JwtTokenConfig:SigningKey` - JWT token signing key
- `TwoFactorAuthConfig:MasterKey` - Master key for 2FA encryption

### 3. Run Backend API

```bash
dotnet run --project src/BlockSense.Backend/BlockSense.Backend.csproj
```

The API will be available at `https://localhost:5001`

### 4. Run Desktop Client

```bash
dotnet run --project src/BlockSense.Desktop/BlockSense.Desktop.csproj
```

### 5. Access Application

Open the desktop client and follow the on-screen instructions to:

1. **Register** a new account with your invitation code
2. **Create** or **import** a cryptocurrency wallet
3. **Start managing** your digital assets and transactions

---

## 📸 Screenshots

### Dashboard & Wallet Overview

![Wallet UI](screenshots/wallet_ui.png)

### Transaction History and Balance Display

![Transaction History](screenshots/transaction_history.png)

> **Tip:** Screenshots showcase the clean, modern interface designed for ease of use.

---

## 🤝 Contributing

Contributions are welcome and appreciated! Here's how you can contribute:

### How to Contribute

1. **Fork the repository**
   ```bash
   git fork https://github.com/durismaros/BlockSense.git
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Commit your changes**
   ```bash
   git commit -m "Add feature: your feature description"
   ```

4. **Push to your branch**
   ```bash
   git push origin feature/your-feature-name
   ```

5. **Submit a pull request**

### Contribution Guidelines

- Write clear, descriptive commit messages
- Follow the existing code style and conventions
- Add tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting

---

## 📝 License

This project is licensed under the **MIT License**.

See the [LICENSE](LICENSE) file for complete details.

---

<div align="center">

**Built with ❤️ for secure cryptocurrency management**

[Report Bug](https://github.com/durismaros/BlockSense/issues) • [Request Feature](https://github.com/durismaros/BlockSense/issues)

</div>
