# 🤖 AI Customer Complaint Agent

An intelligent Agentic AI system built on **Azure OpenAI + ASP.NET Core** that autonomously receives, analyzes, and acts on customer complaints — without human intervention in routine cases.

> Built as a practical demonstration of the UAE's vision to power 50% of government services with AI Agents within two years.

---

## 🎯 Problem Statement

Organizations receive hundreds of complaints daily. Manual triage causes:
- Slow response times
- Critical cases getting lost
- No clear prioritization
- Weak reporting for management

---

## ⚡ How It Works

### 1. Customer Submits Complaint
Via web form — typed or **voice input** (Arabic & English supported)

### 2. Content Safety Check
Azure Content Safety filters harmful or abusive content before processing

### 3. AI Agent Analyzes & Decides
The agent autonomously determines:
- **Type**: Delivery / Payment / Refund / Technical / Other
- **Sentiment**: Positive / Negative / Neutral
- **Priority**: High / Medium / Low
- **Action**: What to do next — no if-else rules

### 4. Agent Executes Action
| Priority | Action |
|---|---|
| High | Notifies support team via email immediately |
| Medium | Saves complaint + awaits follow-up |
| Low | Auto-resolved — no human intervention needed |

### 5. Customer Gets Response
Personalized response in the **same language** as the complaint

### 6. Follow-Up Agent (Every 6 Hours)
A second autonomous agent wakes up, reviews pending complaints, and decides whether to send follow-up emails — based on context, not fixed rules.

---

## 🏗️ Architecture

```
User (Web / Voice)
        ↓
ASP.NET Core MVC
        ↓
Azure Content Safety ──── (filters harmful content)
        ↓
Semantic Kernel Agent (ComplaintAgent)
        ↓
Azure OpenAI (gpt-4o)
        ↓
Tools Layer:
  ├── send_team_alert      → Gmail SMTP
  └── send_customer_followup → Gmail SMTP
        ↓
Azure SQL Database
        ↓
Dashboard (Support Team)

Background:
FollowUpAgent (every 6h) → reviews pending → decides action
```

> **Production Architecture Note:**
> Designed with Private Endpoint + VNet Integration for enterprise-grade security.
> Current deployment uses Free tier for demonstration purposes.

---

## 🔐 Security

| Feature | Implementation |
|---|---|
| Secrets Management | Azure Key Vault |
| Authentication | Managed Identity (no passwords) |
| Speech Token | Temporary token (10 min expiry) |
| Content Filtering | Azure AI Content Safety |
| HTTPS | Enforced on Azure App Service |

---

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 9 MVC
- **AI Orchestration**: Microsoft Semantic Kernel
- **LLM**: Azure OpenAI (gpt-4o)
- **Database**: Azure SQL Database
- **Speech**: Azure Speech Services (client-side token auth)
- **Content Safety**: Azure AI Content Safety
- **Email**: Gmail SMTP via MailKit
- **Monitoring**: Azure Application Insights
- **CI/CD**: GitHub Actions
- **Secrets**: Azure Key Vault + Managed Identity

---

## 🚀 Getting Started

### Prerequisites
- Azure Subscription
- .NET 9 SDK
- Azure CLI

### Configuration

Add the following secrets to **Azure Key Vault** or **User Secrets**:

| Secret Name | Example Value |
|---|---|
| AzureOpenAI--Endpoint | `https://YOUR_RESOURCE.openai.azure.com/` |
| AzureOpenAI--ApiKey | `YOUR_API_KEY` |
| AzureOpenAI--DeploymentName | `gpt-4o` |
| ConnectionStrings--DefaultConnection | `Server=tcp:YOUR_SERVER.database.windows.net...` |
| Email--Username | `your@gmail.com` |
| Email--Password | `YOUR_APP_PASSWORD` |
| Email--ReceiverEmail | `your@gmail.com` |
| AzureSpeech--Key | `YOUR_SPEECH_KEY` |
| AzureSpeech--Region | `eastus` |
| ContentSafety--Endpoint | `https://YOUR_RESOURCE.cognitiveservices.azure.com/` |
| ContentSafety--ApiKey | `YOUR_KEY` |
| ApplicationInsights--ConnectionString | `InstrumentationKey=YOUR_KEY...` |

### Run Locally

```bash
git clone https://github.com/MoeenKorbaj/ai-complaint-agent
cd AIComplaintAgent
dotnet restore
dotnet run
```

---

## 📊 Features

- ✅ Agentic AI — agent decides actions autonomously
- ✅ Multi-language support (Arabic & English)
- ✅ Voice input with Azure Speech Services
- ✅ Content Safety filtering
- ✅ Automated email notifications
- ✅ Follow-up Agent (runs every 6 hours)
- ✅ Human-in-the-Loop for critical cases
- ✅ Real-time Dashboard
- ✅ Auto-resolve Low priority complaints
- ✅ Enterprise-grade security with Key Vault

---

## 👨‍💼 Author

**Moeen Korbaj**
Senior Software Engineer | Azure Solutions Architect | AI Solutions | AZ-305

[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-blue)](https://www.linkedin.com/in/moeenkorbaj/)
[![GitHub](https://img.shields.io/badge/GitHub-Follow-black)](https://github.com/MoeenKorbaj/ai-complaint-agent)

---

## 📄 License

MIT License