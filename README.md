# 🎬 Streaming Watch Party App

A real-time application to host and join watch parties with synced video playback, chat, and room management — built as a microservices architecture for scalability and flexibility.

---

## Overview

This app enables users to create and join virtual watch parties where video playback is synchronized across participants in real time. It features user authentication, room management, chat, and video proxying, all coordinated via microservices with robust backend infrastructure.

---

## Services

| Service       | Description                                           | Technology / DB      |
|---------------|-------------------------------------------------------|---------------------|
| **UserService** | Handles authentication with JWT, user profiles        | ASP.NET Core, MSSQL  |
| **RoomService** | Creates and manages watch rooms, hosts/guests logic   | ASP.NET Core, MongoDB|
| **SyncService** | Keeps video playback synchronized using SignalR or gRPC streaming | ASP.NET Core |
| **ChatService** | Real-time in-room chat using pub/sub                   | ASP.NET Core, MongoDB|
| **VideoService** | Proxy or integration with YouTube/Twitch APIs          | ASP.NET Core, MongoDB|
| **ApiGateway**  | YARP-based reverse proxy handling routing and rate limiting | ASP.NET Core, YARP  |

---

## Frontend

- Built with **Blazor WASM** for rich real-time UI and video player integration
- Uses **SignalR** for real-time syncing and communication
- Embedded video player using iframe APIs

---

## Architecture

- Microservices backend deployed on **Azure Container Apps** for easy scaling and management
- Frontend hosted as an Azure **Static Web App** or on Azure Container Apps
- Docker used for environment consistency across development and deployment
- YARP (Yet Another Reverse Proxy) provides lightweight routing, auth control, and rate limiting
- Mix of SQL (MSSQL) and NoSQL (MongoDB) databases depending on service needs
- gRPC streaming used for high-performance sync in SyncService

---

## Deployment & Infrastructure

| Service         | Azure Resource           | Free Tier Available? |
|-----------------|-------------------------|---------------------|
| Microservices   | Azure Container Apps     | ✅ Yes              |
| Frontend        | Azure Static Web App / ACA| ✅ Yes              |
| CI/CD           | GitHub Actions          | ✅ Yes              |


---

## Features & Responsibilities by Service

| Feature            | Service          | Notes                              |
|--------------------|------------------|----------------------------------|
| User Auth & Profile | `UserService`    | Microsoft Identity with JWT auth, Blazor login UI, user data storage |
| Room Creation/Join  | `RoomService`    | Host/guest logic, room lifecycle  |
| Video Sync          | `SyncService`    | SignalR hub, supports gRPC streaming for efficient sync |
| In-Room Chat       | `ChatService`     | Real-time chat using pub/sub      |
| Video Playback      | `VideoService`   | YouTube/Twitch API proxying       |
| Real-Time Frontend  | `Blazor WASM` | SignalR client, UI controls       |

---

## Development

- Clone repo and build using Visual Studio or `dotnet` CLI
- Shared protobuf definitions for microservice communication in `/sharedprotos`
- Environment configs managed via `.Template.json` files to avoid leaking secrets

---

## Notes on gRPC and SignalR Usage

The **SyncService** supports both SignalR and gRPC streaming protocols to keep video playback synchronized across all participants in a room. SignalR provides easy integration with Blazor and web clients, while gRPC streaming can be used for more performant backend-to-backend or server-to-server synchronization.

---


