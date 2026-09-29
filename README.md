# SmartPrep Modern

SmartPrep Modern is the Windows desktop client for **NovaStudy**, a source-grounded assessment and learning platform. It provides role-specific workflows for instructors, learners, and administrators.

> **Project context:** The platform was developed as an end-to-end desktop and backend system combining assessment workflows, analytics, document processing, background jobs, and source-grounded LLM analysis.

## Project evolution

The original client version was built around local Ollama inference with a
Qwen model. Limited local hardware led to an AWS G6 GPU deployment, but its
roughly PHP 50 hourly operating cost meant the inference server could not
remain online continuously. The backend consequently queued pending analysis
until the GPU server was available, while a heuristic questionnaire extractor
was introduced to meet the client's requirement for near-instant uploads.

The current NovaStudy version is a broader technical and product rebuild. Its
backend now uses Django REST Framework and Django migrations, OpenRouter
replaces the dedicated GPU inference deployment, and Redis/Celery provides
event-triggered rather than periodically polled analysis. The desktop was
redesigned and generalized from its original subject-specific presentation
into a source-grounded assessment platform with dynamic answer choices.

## Key features

- **Source-based exam generation** — organize review material by category/topic, upload questionnaire PDFs, and generate exams from selected content.
- **Exam delivery** — reviewees can take configured examinations and submit answers through the desktop client.
- **Performance analytics** — dashboards for exam results, comparative performance, growth trends, and leaderboards.
- **Question forensics** — per-question distributions and deeper attempt/item analysis for identifying strengths and problem areas.
- **AI-assisted analysis** — displays structured performance summaries and recommendations produced by the backend.
- **Role-based workflows** — dedicated interfaces for administrators, review directors, and reviewees.

## Screenshots

> Screenshots will be added after the local environment is restored.

<!-- Suggested screenshots:
1. Main dashboard
2. Topic/source management
3. Exam generation or exam session
4. Performance analytics / growth trends
5. Question forensics / AI analysis
-->

## Technology stack

- WPF / VB.NET / .NET 9
- Material Design
- LiveCharts
- Django REST API
- MySQL
- Redis + Celery
- OpenRouter-backed LLM workflows

The desktop client keeps API communication separate from the UI through domain repositories, shared HTTP services, and request/response models.

## Run SmartPrep

SmartPrep Modern requires the separate **SmartPrepPython** backend:

https://github.com/SaintRelion/SmartPrepPython

### 1. Start the backend

Follow the SmartPrepPython setup instructions. The quickest setup uses Docker Compose:

```powershell
docker compose up -d --build
```

### 2. Download SmartPrep Modern

Download the latest **Windows x64** build from this repository's **Releases** page.

Extract the downloaded archive and run:

```text
SmartPrepModern.exe
```

The release is self-contained, so the .NET runtime does not need to be installed separately.

The desktop client must be able to connect to the SmartPrepPython backend.

## First administrator

On a fresh installation, register the first administrator through SmartPrep Modern. New accounts are initially created in a locked state.

After registration, connect to the SmartPrep MySQL database, locate the new administrator in the `users` table, and change `status` from `locked` to `active`. When using the provided backend Docker setup, MySQL is exposed to the host on port `3307` by default.

Do not create the first administrator directly in the database; use the application's registration flow first, then perform only the initial activation.

## Questionnaire documents

Questionnaires and other source documents are runtime/local data and are intentionally not included in the repository.

The preserved SmartPrep backend expects questionnaires in a structured format because its ingestion pipeline uses deterministic parsing. See the **SmartPrepPython** README for the supported format, the reasoning behind the original design, and the planned modernization toward LLM-assisted document extraction.

## Local development

This section is only needed when building or modifying the desktop client.

Requirements:

- Windows
- .NET 9 SDK
- Visual Studio with the .NET desktop development workload, or the .NET CLI
- A running SmartPrepPython backend

From the repository root:

```powershell
dotnet restore
dotnet run
```

To produce a build without running it:

```powershell
dotnet build
```

Before building, make sure `ApiService.BaseUrl` points to the backend instance you want the compiled client to use.

## Author

**June Aurelius Jacinto**  
Full-Stack Software Developer

GitHub: https://github.com/SaintRelion
