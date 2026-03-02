# 🚀 SysGuard.Monitor

SysGuard is a distributed, high-precision system monitoring solution designed for Linux environments. Built with **.NET 8**, it features a decoupled architecture consisting of a data-collection agent, a centralized REST API, and a real-time web dashboard.

## 🏗️ Architecture
The project is professionally divided into three main components to ensure scalability and maintainability:
* **SysGuard.Monitor.Console (Agent):** A lightweight background service that calculates real-time CPU load by parsing the Linux kernel (`/proc/stat`) and monitors RAM/Disk metrics.
* **SysGuard.Monitor.API (Server):** An ASP.NET Core Minimal API that acts as a central hub, receiving data from agents via HTTP POST and serving it to the frontend.
* **SysGuard.Monitor.Models (Shared):** A common class library that ensures type safety and consistent data structures across the entire ecosystem.

## ✨ Key Features
* **Real-Time Dashboard:** A modern, dark-themed web interface that updates every second without requiring page refreshes.
* **High-Precision Monitoring:** Directly calculates CPU usage by sampling kernel ticks for professional-grade accuracy.
* **Distributed Design:** Multiple agents can push data to a single central API.
* **Linux Native:** Specifically optimized for Linux distributions like **Fedora** and Arch.

## 🚀 Getting Started

### Prerequisites
* .NET 8 SDK
* Linux Environment (Tested on Fedora)

### Installation & Run
1.  **Clone the repository:**
    ```bash
    git clone https://github.com/lezgintekay/SysGuard.Monitor.git
    cd SysGuard.Monitor
    ```
2.  **Start the API Server:**
    ```bash
    dotnet run --project SysGuard.Monitor.API
    ```
3.  **Start the Monitoring Agent:**
    ```bash
    dotnet run --project SysGuard.Monitor.Console
    ```
4.  **View Results:** Open your browser and navigate to `http://localhost:5005`.

## 🛠️ Tech Stack
* **Backend:** C#, .NET 8, ASP.NET Core
* **Frontend:** HTML5, CSS3, JavaScript (Fetch API & Intervals)
* **DevOps:** Git (Feature Branch Workflow), Docker-ready
