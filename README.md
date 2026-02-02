# 💼 Desktop Cryptocurrency Wallet

![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)
![GitHub top language](https://img.shields.io/github/languages/top/yourusername/desktop-crypto-wallet)
![.NET](https://img.shields.io/badge/.NET-7.0-blue)
![Avalonia](https://img.shields.io/badge/Avalonia-UI-red)

A **secure and reliable desktop cryptocurrency wallet** application for managing digital assets.  
This project emphasizes **security, cryptography, and safe handling of sensitive data**, built with modern technologies and best practices.  

---

## 📌 Table of Contents

- [Overview](#overview)  
- [Features](#features)  
  - [Functional Features](#functional-features)  
  - [Non-Functional Requirements](#non-functional-requirements)  
- [Architecture & Technologies](#architecture--technologies)  
- [Security](#security)  
- [User Interface](#user-interface)  
- [Setup & Installation](#setup--installation)  
- [Screenshots](#screenshots)  
- [Contributing](#contributing)  
- [License](#license)  

---

## 🛠 Overview

The Desktop Cryptocurrency Wallet enables users to **securely manage digital currencies and accounts**. 

**Components:**

- **Desktop Client:** Built with [Avalonia UI Framework](https://avaloniaui.net/) for a **responsive and intuitive interface**.  
- **Backend API:** Built with **ASP.NET Core Web API** and **MySQL** for secure authentication, authorization, and wallet operations.  

**Project Goals:**

- Implement a **secure authentication & authorization system**.  
- Enable creation, import, and management of cryptocurrency wallets.  
- Provide reliable transaction management and balance tracking.  
- Maintain an **intuitive, user-friendly interface**.  
- Apply **cryptography best practices** to protect user data.  

---

## ⚙️ Features

### Functional Features

**User Management:**

- Registration via email, password, and valid invitation code.  
- Invitation validation before registration.  
- Login, logout, and optional 2FA support.  
- Password reset via email.  

**Authentication & Authorization:**

- TOTP (Time-based One-Time Password) support with Google Authenticator.  
- JWT authentication for sensitive API calls.  
- Refresh token support for persistent sessions.  

**Wallet Management:**

- Create new wallets or import using a 12-word seed phrase.  
- Local encryption and secure storage of seed phrases and private keys.  
- Only one active wallet per user at a time.  

**Transactions:**

- Send and receive supported cryptocurrencies: **Bitcoin, Ethereum, Litecoin**.  
- Transaction confirmation with sum and recipient address verification.  
- View transaction history and real-time balance.  

**Data Security:**

- All sensitive data encrypted locally.  
- No sensitive data transmitted to the server.  

### Non-Functional Requirements

- AES-256 GCM/CBC encryption for data, Argon2Id for password hashing.  
- Brute-force attack protection (30-second login timeout).  
- Low API latency (<500ms).  
- Windows-only support.  
- Logging of user activity (local and remote).  
- Intuitive, clear, and beginner-friendly interface.  

---

## 🏗 Architecture & Technologies

- **Frontend:** Avalonia UI (C#)  
- **Backend API:** ASP.NET Core Web API  
- **Database:** MySQL  
- **Local Data Storage:** LevelDB (encrypted wallets)  
- **Authentication:** JWT & TOTP (Google Authenticator)  
- **Cryptography:** AES-256, Argon2Id  
- **Version Control:** Git  

**Architecture:** Client-server model separating UI from backend logic, prioritizing **security, reliability, and maintainability**.

---

## 🔐 Security

- Local encryption of all sensitive data (seed phrases, private keys).  
- Password hashing with Argon2Id.  
- JWT-based authentication for API requests.  
- 2FA support for unauthorized access prevention.  
- Brute-force attack mitigation.  
- No sensitive information is transmitted over the network.  

---

## 🎨 User Interface

The application provides a **clean and modern interface**:

- Easy account registration and login.  
- Clear wallet management controls.  
- Transaction history and balance display with clarity.  
- Step-by-step guidance for new users.  

---

## ⚡ Setup & Installation

Configure Backend

Set up MySQL database.

Configure connection string in appsettings.json.

Run migrations:

dotnet ef database update

Run Backend API
dotnet run --project WalletBackend/WalletBackend.csproj

Run Desktop Client
dotnet run --project WalletClient/WalletClient.csproj

Access Application

Open the desktop client and follow the on-screen instructions for registration, wallet creation, and setup.

## 📸 Screenshots

Add screenshots of your application to show UI and features.

![Wallet UI Placeholder](screenshots/wallet_ui.png)
![Transaction History Placeholder](screenshots/transaction_history.png)

## 🤝 Contributing

Contributions are welcome!

Fork the repository.

Create a new feature branch.

Commit changes with descriptive messages.

Submit a pull request for review.

## 📝 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.