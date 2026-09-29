# SmartPrep Modern

SmartPrep Modern is the Windows desktop client for **NovaStudy**, a
source-grounded assessment and learning platform. It provides
role-specific workflows for instructors, learners, and administrators.

> **Project context:** The platform was developed as an end-to-end
> desktop and backend system combining assessment workflows, analytics,
> document processing, background jobs, and source-grounded LLM
> analysis.

## Project evolution

The original version used local Ollama inference with a Qwen model.
Limited local hardware led to an AWS G6 GPU deployment, while queued
background processing allowed analytical work to continue without
keeping the inference server online continuously.

The current NovaStudy version is a broader technical and product
rebuild. The desktop was redesigned and generalized from its original
subject-specific presentation, while the backend moved to Django REST
Framework, OpenRouter, and event-triggered Redis/Celery processing.

## Key features

-   **Questionnaire-based assessments** --- organize source material by
    category and topic, upload human-prepared questionnaires, and
    configure examinations from the supplied questions.
-   **Exam delivery** --- learners can take configured examinations and
    submit answers through the desktop client.
-   **Performance analytics** --- dashboards for exam results,
    comparative performance, growth trends, and leaderboards.
-   **Question forensics** --- per-question distributions and deeper
    attempt/item analysis for identifying strengths and problem areas.
-   **AI-assisted analysis** --- structured performance summaries,
    source-grounded explanations, and recommendations produced from
    assessment data and supplied learning materials.
-   **Role-based workflows** --- dedicated interfaces for
    administrators, instructors, and learners.

## Screenshots

> Screenshots will be added from the redesigned portfolio version using
> non-confidential data.

```{=html}
<!-- Suggested screenshots:
1. Main dashboard
2. Topic/source management
3. Exam configuration or exam session
4. Performance analytics / growth trends
5. Question forensics / AI analysis
-->
```
## Architecture

SmartPrep is a client-server system. The Windows desktop application
communicates with a separately hosted Django REST backend responsible
for assessment workflows, document processing, persistence, background
analysis, and LLM-assisted features.

``` text
SmartPrep Modern
WPF / .NET 9
       |
       | REST API
       v
Demo Backend
Personal Infrastructure
       |
       +-- Django REST Framework
       +-- MySQL
       +-- Redis / Celery
       +-- OpenRouter
```

The desktop client keeps API communication separate from the UI through
domain repositories, shared HTTP services, and request/response models.

The backend source code and deployment infrastructure are maintained
privately. Their technologies are documented here to show the
architecture of the complete system.

## Technology stack

### Desktop client

-   WPF / VB.NET / .NET 9
-   Material Design
-   LiveCharts
-   REST API communication

### Backend and infrastructure

-   Django REST Framework
-   MySQL
-   Redis + Celery
-   OpenRouter-backed LLM workflows
-   Docker

## Run SmartPrep

SmartPrep Modern is distributed as a self-contained **Windows x64**
application.

1.  Download the latest build from this repository's **Releases** page.
2.  Extract `SmartPrepModern-v1.0.0-win-x64.zip`.
3.  Run `SmartPrepModern.exe`.

A separate .NET runtime installation is not required.

The released application connects to the hosted SmartPrep backend; no
local backend or Docker setup is required to run the distributed client.

## Local development

This section is only needed when building or modifying the desktop
client.

Requirements:

-   Windows
-   .NET 9 SDK
-   Visual Studio with the .NET desktop development workload, or the
    .NET CLI
-   Access to a compatible SmartPrep API endpoint

From the repository root:

``` powershell
dotnet restore
dotnet run
```

To produce a release build:

``` powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

The API endpoint used by the compiled client is configured through
`ApiService.BaseUrl`.

## Author

**June Aurelius Jacinto**\
Full-Stack Software Developer

GitHub: https://github.com/SaintRelion
