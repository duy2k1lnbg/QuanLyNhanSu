🏢 Enterprise Human Resource Management System

An enterprise-grade Human Resource Management System (HRMS) built on the Microsoft .NET platform with Oracle Database integration and on-premise AI capabilities.

This project demonstrates enterprise application architecture, database integration, role-based authorization, and local AI assistant integration.

📌 Overview

This is a Windows desktop HR system developed using .NET Framework 4.7.2 and DevExpress WinForms.
It connects to Oracle Database 19c using Entity Framework 6 (EF6).

The system is designed for enterprise environments requiring:

Structured employee management

Department and role management

Permission-based access control

Secure database connectivity

On-premise AI support (no cloud dependency)

🛠 Technologies Used

.NET Framework 4.7.2

Windows Forms + DevExpress

Oracle Database 19c

Entity Framework 6 (EF6)

Oracle.ManagedDataAccess.EntityFramework

Ollama (Local AI Runtime)

Llama 3 8B Instruct (q4_1 quantized)

🏗 Architecture

The system follows a layered enterprise structure:

Presentation Layer – WinForms + DevExpress UI

Business Logic Layer – Application rules and processing

Data Access Layer – Entity Framework 6 with Oracle provider

Database Layer – Oracle 19c

AI integration runs locally via Ollama and communicates through HTTP API.

🔐 Key Features

Employee Management (CRUD operations)

Department Management

Role & Permission System

Oracle-based data persistence

Entity Framework ORM mapping

On-premise AI assistant integration

Secure local deployment

🤖 AI Integration (On-Premise)

This system integrates a local Large Language Model using:

Ollama

Llama 3 8B Instruct (q4_1)

Benefits:

Runs completely offline

No external API cost

Data privacy maintained

Suitable for enterprise environments

The AI layer can be used for:

HR data analysis

Intelligent reporting

Internal assistant support

Development productivity enhancement

⚙️ Installation & Setup
1. Requirements

Visual Studio (.NET Desktop Development workload)

Oracle Data Access Components (ODAC) 19c

DevExpress WinForms

Ollama (for AI integration)

2. Clone Repository
git clone https://github.com/your-username/QuanLyNhanSu.git
3. Configure Database

Update the connection string inside:

App.config

Example:

<connectionStrings>
  <add name="HRDbContext"
       connectionString="User Id=your_user;Password=your_password;Data Source=your_datasource"
       providerName="Oracle.ManagedDataAccess.Client" />
</connectionStrings>
4. Run Ollama (Optional – AI Feature)

Install Ollama and pull the model:

ollama pull llama3:8b-instruct-q4_1

Start the Ollama service before running the application.

🧠 Enterprise Focus

This project is structured to simulate a real-world enterprise HR system:

Database-first architecture

Structured permission tables

Layered design

Local AI deployment for secure environments

📄 License

This project is developed for educational and enterprise demonstration purposes.
