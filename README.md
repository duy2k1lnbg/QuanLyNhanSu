# 🏢 Enterprise Human Resource Management System (HRMS)

[![Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blueviolet)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Oracle-19c-red)](https://www.oracle.com/database/)
[![UI Component](https://img.shields.io/badge/UI%20Library-DevExpress-orange)](https://www.devexpress.com/)
[![AI](https://img.shields.io/badge/AI-Ollama%20/%20Llama%203-blue)](https://ollama.com/)

An enterprise-grade **Human Resource Management System (HRMS)** architected on the Microsoft .NET ecosystem. This solution integrates **Oracle Database 19c** for robust data persistence and features an **On-Premise AI Assistant** powered by Llama 3 to ensure data privacy and intelligent decision-making.

---

## 📌 Project Overview

This system is engineered to handle complex organizational structures, providing a secure and scalable environment for managing corporate human capital. By leveraging **DevExpress WinForms**, it offers a high-performance desktop experience tailored for HR professionals.

### Key Value Propositions:
* **Data Sovereignty:** Local AI integration ensures sensitive HR data never leaves the corporate network.
* **High Availability:** Built for Oracle 19c, ensuring enterprise-level reliability and performance.
* **Rich UI/UX:** Advanced data grids and reporting tools via DevExpress components.

---

## 🏗 System Architecture

The application follows a rigorous **N-Tier Architecture** to ensure maintainability and scalability:

1.  **Presentation Layer:** WinForms integrated with DevExpress UI components for a modern, responsive interface.
2.  **Business Logic Layer (BLL):** Handles validation, workflow orchestration, and core HR business rules.
3.  **Data Access Layer (DAL):** Implemented via **Entity Framework 6 (EF6)** using the Oracle Managed Driver for seamless ORM mapping.
4.  **Intelligence Layer:** A local HTTP-based interface connecting to the **Ollama** runtime for private AI processing.

---

## 🛠 Technology Stack

| Component | Technology |
| :--- | :--- |
| **Runtime** | .NET Framework 4.7.2 |
| **UI Library** | DevExpress WinForms |
| **Database** | Oracle Database 19c (Enterprise Edition) |
| **ORM** | Entity Framework 6.4 |
| **AI Runtime** | Ollama (Local LLM Instance) |
| **LLM Model** | Llama 3 8B Instruct (Quantized q4_1) |

---

## 🔐 Core Features

* **Employee Lifecycle Management:** Comprehensive CRUD operations with detailed history tracking.
* **Organizational Mapping:** Dynamic department and role hierarchy management.
* **RBAC (Role-Based Access Control):** Granular permission system ensuring data security and compliance.
* **Advanced Reporting:** Integrated analytics and export capabilities (Excel, PDF, Word).
* **Private AI Assistant:** * Synthesize employee performance data.
    * Automate internal HR queries.
    * Draft HR communications and policy summaries.

---

## 🤖 AI Integration (On-Premise)

Unlike traditional HR systems that rely on cloud APIs (OpenAI/Azure), this system prioritizes **Privacy First**:
* **Deployment:** The model runs locally via **Ollama**.
* **Model:** `llama3:8b-instruct-q4_1` provides a perfect balance between inference speed and reasoning capabilities.
* **Security:** Zero data leakage risk as no external API calls are made.

---

## ⚙️ Installation & Configuration

### 1. Prerequisites
* Visual Studio 2022 (.NET Desktop Development workload).
* Oracle Data Access Components (ODAC) 19c.
* DevExpress WinForms Library.
* Ollama Runtime.

### 2. Database Setup
Update the `App.config` file with your Oracle connection string:
```xml
<connectionStrings>
  <add name="HRDbContext" 
       connectionString="User Id=your_user;Password=your_password;Data Source=your_oracle_datasource" 
       providerName="Oracle.ManagedDataAccess.Client" />
</connectionStrings>
