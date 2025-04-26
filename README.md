# ChatSupport Backend System

An ASP.NET Core Web API that manages support chat queues, agent assignments, overflow logic, and session monitoring.  
Built with clean layered architecture: Domain, Application, Infrastructure, and API projects.

---

## Features

- Queue user chat requests (FIFO order)
- Assign agents based on seniority, shift, and concurrency
- Activate overflow agents during office hours if needed
- Background service to monitor missed polls
- Seed database with agents, shifts, and sample sessions
- Dashboard endpoint to view current queue and agent states
- xUnit test coverage for critical business rules

---

## Solution Structure

CC.ChatSupport.sln
- CC.ChatSupport.API/ (Presentation Layer - Web API)
- CC.ChatSupport.Application/ (Application Layer - Services & Logic)
- CC.ChatSupport.Domain/ (Domain Layer - Entities)
- CC.ChatSupport.Infrastructure/ (Infrastructure Layer - EF Core)
- CC.ChatSupport.Tests/ (Unit Tests - xUnit)

---

## Requirements

- .NET 8 SDK
- SQL Server
- Visual Studio 2022+ 

---

## Getting Started

1. **Clone or download** the repository.
2. Open the solution in **Visual Studio**.
3. Set `CC.ChatSupport.API` as the **Startup Project**.
4. Ensure SQL Server is running locally.

## Database Setup

1. Set the `CC.ChatSupport.Api` as the Startup Project
2. Inside Visual Studio, open **Tools → NuGet Package Manager → Package Manager Console**.
3. Set Default Project to `CC.ChatSupport.Infrastructure`, then run:

4. Add-Migration InitialCreate -p CC.ChatSupport.Infrastructure -s CC.ChatSupport.API
5. Update-Database