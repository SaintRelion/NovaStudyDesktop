# NovaStudy Desktop

NovaStudy is a Windows assessment workspace for educators preparing exams from supplied material and for learners taking those exams and reviewing their progress. The desktop client brings the learning library, assessment setup, timed sessions, and analysis into one role-specific interface. I developed the original end-to-end system and later reengineered this portfolio edition.

**Client project · Reengineered portfolio edition.** The client permitted a public portfolio presentation. NovaStudy uses generalized branding and fictional demonstration records; no client documents or records are included here. The portfolio edition connects to infrastructure separate from the client's installation. Changes described below were made later and do not imply they were delivered to the client. There is no standalone browser deployment of the WPF app; the HTML previews below are offline representations, not a live product demo.

## Original delivery and constraints

The original desktop-and-backend system was developed in under two months for a subject-specific assessment workflow. Local inference with Ollama and Qwen met the initial hardware requirement, but limited capacity led to an AWS G6 GPU. Keeping that server available was costly, so queued analysis could run when it was online. A heuristic questionnaire extractor prioritized quick uploads when model-backed extraction was too slow. Those choices supported the original delivery but added operational and extraction complexity.

## Selected workflows

<!-- portfolio:showcase:start -->

<!-- portfolio:feature learning-library -->
## Learning library

An educator organizes source material and a human-authored questionnaire into topic slots. The client shows upload and processing state, previews extracted questions, warns before replacing questions, and blocks replacement when an active assessment depends on the slot. PDF is recommended when Word layout could affect extraction. The model extracts supplied questions; it does not invent the questionnaire.

<!-- portfolio:preview showcase_html/learning-library.html -->

<!-- portfolio:feature assessment-builder -->
## Assessment builder

An educator stages ready topic slots, chooses how many existing questions to draw from each, and can randomize their order. The current builder checks each slot's available count and requires exactly 100 questions before generating an assessment. The smaller offline preview demonstrates that validation without creating a real exam.

<!-- portfolio:preview showcase_html/assessment-builder.html -->

<!-- portfolio:feature timed-assessment -->
## Timed assessment

A learner answers one question at a time against a per-question timer, then enters a separate review phase to revisit responses before submission. Choices come from the uploaded questionnaire rather than a fixed A–D layout. Unanswered questions are treated as incorrect when the attempt is submitted or times out. The preview uses three fictional questions for a short interaction; the desktop uses the configured assessment and timing rules.

<!-- portfolio:preview showcase_html/timed-assessment.html -->

<!-- portfolio:feature progress-analysis -->
## Progress and item analysis

Educators and learners can select an attempt on the progress chart to inspect question-level answer distributions and available AI analysis. The desktop opens item analysis in the main content area, supports sorting and expanded question details, and keeps deeper attempt forensics as a separate view. Analysis depends on submitted results and background processing; it may not be available immediately.

<!-- portfolio:preview showcase_html/progress-analysis.html -->

<!-- portfolio:showcase:end -->

## Engineering changes in this edition

The interface was generalized and redesigned as a desktop workspace with top navigation and a new visual identity. Exam loading now validates question data before starting the timer; upload states, question previews, and item-analysis navigation were revised to provide clearer feedback. The backend moved from FastAPI and raw SQL to Django REST Framework and migrations. OpenRouter replaces the dedicated GPU inference path, while Redis and Celery remain for asynchronous document and analysis jobs.

These are changes in the portfolio edition, not claims about the original client installation. The release and deployment process is structured, but the presence of Docker or Kubernetes alone is not a production-readiness guarantee.

## Architecture and tradeoffs

The WPF/VB.NET client targets .NET 9 and calls a separately hosted Django REST API through shared HTTP services, repositories, and request/response models. PostgreSQL stores application data; Redis and Celery handle work that should not block the API; OpenRouter supplies model-assisted extraction and analysis. Material Design styles and LiveCharts support the desktop UI.

A desktop app fits the original Windows workflow, but limits access to Windows and requires separate client releases. Hosted inference removes dedicated GPU maintenance but adds network and provider dependency. The exactly-100-item builder rule is still a product constraint worth revisiting for more general assessments. The desktop's configured API endpoint is compiled into `APISync/Services/ApiService.vb`, so switching environments currently requires changing the client configuration or build.

## Release lifecycle

This repository builds the Windows client; the Django backend has its own repository and release process. There is no frontend website image or database migration in the desktop build. Backend migrations and deployment are coordinated separately. Local desktop development can point the API service at a compatible local backend; the current client source points to the portfolio backend. No staging environment is documented for this desktop project.

## Known limitations

The app requires Windows and a reachable compatible API. Document extraction and AI analysis depend on backend jobs and may finish after the initial request. The HTML demos are isolated illustrations: they make no API calls, do not authenticate, and do not save data. They are not evidence of backup, recovery, high availability, or security certification.

## Run and develop

For a distributed build, use this repository's Windows x64 release archive if available, extract it, and launch `SmartPrepModern.exe`. The current source builds with the .NET 9 SDK on Windows:

```powershell
dotnet restore
dotnet run
```

To publish a self-contained Windows build:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

Set `ApiService.BaseUrl` in `APISync/Services/ApiService.vb` for the backend you intend to use. Running the desktop client does not start Django, PostgreSQL, Redis, or Celery.

## Author

June Aurelius Jacinto · Full-Stack Software Developer · [GitHub](https://github.com/SaintRelion)
