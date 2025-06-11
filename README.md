# db-query-v1.0.0.1

This repository contains an ASP.NET Core 8.0 web application that integrates OpenAI services with OCR and SQL Server data access. It provides a conversational interface where users can upload images, perform web searches, and query data using natural language.

## Features

- Chat interface powered by **OpenAI ChatGPT**.
- OCR processing via **Tesseract** (and Azure Computer Vision support) for extracting text from uploaded images.
- Optional web search results from **DuckDuckGo**.
- Ability to run ad-hoc SQL queries against a SQL Server database and display the results.
- Image generation using OpenAI DALL·E.
- User authentication and profile management via **ASP.NET Core Identity**.
- Entity Framework Core migrations for database schema management.
- GitHub Actions workflow for Azure App Service deployment.

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download).
- [Node.js](https://nodejs.org/) for client-side assets.
- SQL Server instance for the application database.

## Setup

1. Clone the repository and install the Node.js dependencies:

   ```bash
   npm install
   ```

2. Ensure the following environment variables are available (for local development you can place them in a `.env` file):

   - `OPENAI_API_KEY` – API key for OpenAI services.
   - `AZURE_COMPUTER_VISION_API_KEY` – key for Azure Computer Vision (optional if only using Tesseract).
   - `AZURE_COMPUTER_VISION_ENDPOINT` – endpoint URL for Azure Computer Vision.
   - Connection string `DefaultConnection` in `appsettings.json` or via environment variables for SQL Server.

   The application reads these variables when configuring services in `Program.cs`【F:Program.cs†L30-L70】.

3. Apply database migrations and run the application:

   ```bash
   dotnet ef database update
   dotnet run
   ```

4. Navigate to `https://localhost:5001` (or the port indicated in the output) to access the chat interface.

## Deployment

A GitHub Actions workflow is included for deploying the app to Azure App Service. Configure the `AZURE_WEBAPP_NAME` variable and add an `AZURE_WEBAPP_PUBLISH_PROFILE` secret to your repository to use it【F:.github/workflows/azure-webapps-dotnet-core.yml†L23-L88】.

## License

This project currently does not include a specific license file. Please add an appropriate license if you plan to publish or distribute the code.
