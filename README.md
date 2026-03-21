# 🏢 Enterprise Human Resource Management System (HRMS)

[![Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blueviolet)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Oracle-19c-red)](https://www.oracle.com/database/)
[![UI Library](https://img.shields.io/badge/UI%20Library-DevExpress-orange)](https://www.devexpress.com/)
[![AI-Engine](https://img.shields.io/badge/AI-Hybrid%20RAG-blue)](https://ollama.com/)

An enterprise-grade **Human Resource Management System (HRMS)** architected on the Microsoft .NET ecosystem. This solution integrates **Oracle Database 19c** for robust data persistence and features an **On-Premise AI Assistant** powered by Qwen 2.5 / Llama 3 to ensure data privacy and intelligent decision-making.

---

## 🌎 Language Versions
* [English Version](#english-overview)
* [日本語概要 (Japanese Overview)](#-日本語概要-japanese-overview)

---

## 🇺🇸 English Overview

### 📌 Project Overview
This system is engineered to handle complex organizational structures, providing a secure and scalable environment for managing corporate human capital. By leveraging **DevExpress WinForms**, it offers a high-performance desktop experience tailored for HR professionals.

* **Hybrid RAG Architecture:** Combines structured SQL querying with LLM reasoning for 100% data accuracy.
* **Data Sovereignty:** Local AI integration ensures sensitive HR data never leaves the corporate network.

### 🏗 System Architecture
1. **Presentation Layer:** DevExpress WinForms modern responsive interface.
2. **Business Logic Layer (BLL):** Core HR services and workflow orchestration.
3. **Intelligence Layer (Hybrid RAG):** NL2SQL engine with AiRouter, Cache, and History services.
4. **Data Access Layer (DAL):** Entity Framework 6 (EF6) with Oracle Managed Driver.

### 🤖 AI Intelligence & Security
* **Natural Language to SQL (NL2SQL):** Translates Vietnamese/English queries into optimized Oracle SQL.
* **Dedicated Read-Only Account:** The AI is assigned a specific database user with **Read-Only** privileges.
* **View-Restricted Access:** Access is granted ONLY to predefined **Views** (e.g., `V_AI_EMPLOYEE`, `V_AI_ATTENDANCE`), isolating the AI from core business tables.
* **SQL Injection Prevention:** Strict regex cleaning and keyword blacklisting (**DELETE, UPDATE, DROP, etc.**).

---

## 🇯🇵 日本語概要 (Japanese Overview)

### **エンタープライズ人事管理システム (HRMS) - AI統合型**

本システムは、Microsoft .NET エコシステムと言語モデル（LLM）を融合させた次世代の人事管理ソリューションです。**Oracle Database 19c** による堅牢なデータ管理と、**Ollama** を活用したオンプレミス AI アシスタントにより、機密データの外部流出を完全に防ぎつつ、インテリジェントな意思決定を支援します。

### **主な特徴 (Key Features)**

* **ハイブリッド RAG アーキテクチャ:** 自然言語による質問を正確な SQL クエリに変換し、100% 正確なデータ参照を実現します。
* **データ・ソブリンティ (データ主権):** ローカル環境で AI を実行するため、個人情報や給与データが企業ネットワーク外に出ることはありません。
* **高パフォーマンス UI:** DevExpress WinForms を採用し、大量の勤怠・給与データも高速に処理・表示します。

### **AI セキュリティとアクセス制御 (AI Security & Access Control)**

業務データの安全性を最優先し、AI には厳格な制限を設けています。

* **読み取り専用アカウント (Read-Only):** AI はデータの参照のみが許可された専用アカウントを使用します。
* **ビュー限定アクセス (View-Restricted):** 基幹テーブルへの直接アクセスは禁止されており、事前定義された「参照専用ビュー」のみを介してデータを取得します。
* **データ整合性の保護:** AI にはデータの追加・修正・削除（INSERT/UPDATE/DELETE）の権限が一切与えられていないため、ヒューマンエラーや AI の誤作動によるデータ破損のリスクがありません。

---

## 🛠 Technology Stack / 技術スタック

| Component | Technology / 技術仕様 |
| :--- | :--- |
| **Runtime** | .NET Framework 4.7.2 |
| **UI Library** | DevExpress WinForms |
| **Database** | Oracle Database 19c |
| **ORM** | Entity Framework 6.4 |
| **AI Runtime** | Ollama (Local LLM) |
| **Primary Model** | **Qwen 2.5** / **Llama 3** |

---

## ⚙️ Configuration (App.config)

```xml
<configuration>
  <connectionStrings>
    <add name="AiEntities" connectionString="provider=Oracle.ManagedDataAccess.Client;..." />
  </connectionStrings>
  <appSettings>
    <add key="OllamaUrl" value="http://localhost:11434/api/generate" />
    <add key="DefaultModel" value="qwen2.5:latest" />
  </appSettings>
</configuration>
