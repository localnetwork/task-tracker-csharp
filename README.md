# Task Tracker C#

A simple **Task Tracker** application built with **.NET** and **MySQL**. This project allows you to manage tasks efficiently with a structured database backend.

## Features

- Add, update, and delete tasks.
- User management with basic validation.
- Database migrations for easy setup.
- Fully configurable via `.env` file by copying .env.example file.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [MySQL](https://www.mysql.com/downloads/)
- Git

## Setup

1. Clone the repository:

```bash
git clone https://github.com/localnetwork/task-tracker-csharp.git
cd task-tracker-csharp
```

2. Configure the .env file with your database credentials:

```bash
DB_SERVER=localhost
DB_PORT=3306
DB_NAME=
DB_USER=
DB_PASS=
JWT_SECRET=
APP_NAME=
```

3. Run the migration to create the necessary database tables:

```bash
dotnet run migrate
```

4. Start the application:

```bash
dotnet run
```

# Project Structure

```bash
src/
├── config/ # Database connection and configuration
├── schema/ # Database migration scripts
├── models/ # Entity models
├── validators/ # FluentValidation validators
├── routes/ # FluentValidation validators
└── Program.cs # Entry point
```
