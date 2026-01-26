# 🦉 OwlLogs SDK

A comprehensive, production-ready logging solution for ASP.NET Core applications with support for multiple sinks, intelligent buffering, and sensitive data masking.

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
  - [Service Configuration](#️-service-configuration)
  - [Enable Middleware](#️-enable-the-middleware)
- [Configuration Guide](#-configuration-guide)
  - [Sinks Configuration](#sinks-configuration)
  - [Endpoint Filtering](#endpoint-filtering)
  - [HTTP Logging Options](#http-logging-options)
  - [Buffer & Batch Configuration](#buffer--batch-configuration)
  - [Data Masking](#data-masking)
  - [Exception Handling](#exception-handling)
- [Database Schema](#-sql-server-log-schema)
- [Custom Events](#-logging-custom-events)
- [Architecture](#-architecture-overview)
- [Security & Performance](#-security--performance)

## ✨ Features

- **Multi-Sink Support**: Log to ILogger, Console, RabbitMQ, and SQL Server simultaneously
- **Smart Buffering**: Configurable buffer and batch sizes with interval-based flushing
- **Data Masking**: Automatically mask sensitive fields and headers
- **Exception Tracking**: Fine-grained control over exception logging levels
- **HTTP Interception**: Capture request/response bodies, headers, and status codes
- **Correlation IDs**: Track requests across distributed systems
- **Async Processing**: Background service for non-blocking log operations
- **Auto Schema Creation**: Automatically creates SQL Server tables
- **Thread-Safe**: Production-ready with concurrent request handling

## 🛠️ Tech Stack

- **.NET 6+**
- **ASP.NET Core**
- **SQL Server**
- **RabbitMQ**
- **Background Services**
- **System.Text.Json**

## 📦 Installation

Install the OwlLogs NuGet package:

```bash
dotnet add package OwlLogs.Sdk
```

## 🚀 Quick Start

### 1️⃣ Service Configuration

In `Program.cs`, add the OwlLogs service configuration:

```csharp
using OwlLogs.Sdk.Models;
using System.ComponentModel.DataAnnotations;

builder.Services.AddOwlLogs(options =>
{
    options.Enabled = true;

    // Sinks
    options.ILoggerSink.Enabled = false;
    options.Console.Enabled = true;

    options.RabbitMq.Enabled = true;
    options.RabbitMq.HostName = "localhost";
    options.RabbitMq.QueueName = "api_logs_queue";

    options.SqlServer.Enabled = true;
    options.SqlServer.ConnectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");
    options.SqlServer.TableName = "api_logs";
    options.SqlServer.AutoCreateTable = true;

    // Endpoint filtering
    options.Endpoints.Allow(
        "/api/auth/login",
        "/api/auth/register",
        "/api/transporters/transporter",
        "/api/auth/verify",
        "/api/transporters",
        "/test"
    );

    options.Endpoints.Deny("/swagger");

    // HTTP logging
    options.LogRequestBody = true;
    options.LogResponseBody = true;
    options.LogRequestHeaders = true;
    options.LogResponseHeaders = true;
    options.MaxBodySize = 16_000;

    // Buffer & batch configuration
    options.BufferSize = 1000;
    options.BatchSize = 10;
    options.FlushIntervalMs = 500;

    // Data masking
    options.Mask.Enabled = true;
    options.Mask.Fields.Add("accessToken");
    options.Mask.Headers.Add("authorization");

    // Exception handling
    options.ExceptionOptions.SetLogLevel<ValidationException>(LogLevel.Warning);
    options.ExceptionOptions.SetLogLevel<InvalidOperationException>(LogLevel.Critical);
    options.ExceptionOptions.SetLogLevel<ArgumentException>(LogLevel.Error);
    options.ExceptionOptions.SetLogLevel<NullReferenceException>(LogLevel.Critical);
    options.ExceptionOptions.SetLogLevel<ArgumentNullException>(LogLevel.Warning);

    options.ExceptionOptions.LogExceptions = true;
    options.ExceptionOptions.LogInnerExceptions = true;
    options.ExceptionOptions.LogStackTrace = true;
    options.ExceptionOptions.LogMessage = true;
    options.ExceptionOptions.LogSource = false;
    options.ExceptionOptions.LogData = false;
});
```

### 2️⃣ Enable the Middleware

Still in `Program.cs`, add the middleware to the request pipeline:

```csharp
app.UseOwlLogs();
```

**That's it! Your API is now observable 🦉**

## ⚙️ Configuration Guide

### Sinks Configuration

OwlLogs supports multiple logging sinks that can be enabled/disabled independently:

#### ILogger Sink
```csharp
options.ILoggerSink.Enabled = true;
```

#### Console Sink
```csharp
options.Console.Enabled = true;
```

#### RabbitMQ Sink
```csharp
options.RabbitMq.Enabled = true;
options.RabbitMq.HostName = "localhost";
options.RabbitMq.Port = 5672;
options.RabbitMq.QueueName = "api_logs_queue";
options.RabbitMq.UserName = "guest";
options.RabbitMq.Password = "guest";
```

#### SQL Server Sink
```csharp
options.SqlServer.Enabled = true;
options.SqlServer.ConnectionString = "Server=localhost;Database=logs;";
options.SqlServer.TableName = "api_logs";
options.SqlServer.AutoCreateTable = true;
```

### Endpoint Filtering

Whitelist specific endpoints to log:

```csharp
options.Endpoints.Allow(
    "/api/auth/login",
    "/api/auth/register",
    "/api/users",
    "/api/products"
);
```

Or blacklist endpoints:

```csharp
options.Endpoints.Deny(
    "/swagger",
    "/health",
    "/metrics"
);
```

### HTTP Logging Options

Control what HTTP data gets logged:

```csharp
options.LogRequestBody = true;
options.LogResponseBody = true;
options.LogRequestHeaders = true;
options.LogResponseHeaders = true;
options.MaxBodySize = 16_000;  // Maximum body size in bytes
```

### Buffer & Batch Configuration

Fine-tune buffering behavior for performance:

```csharp
options.BufferSize = 1000;        // Maximum entries in buffer
options.BatchSize = 10;           // Entries per batch write
options.FlushIntervalMs = 500;    // Flush every 500ms
```

### Data Masking

Automatically mask sensitive information:

```csharp
options.Mask.Enabled = true;

// Mask specific fields in JSON bodies
options.Mask.Fields.Add("accessToken");
options.Mask.Fields.Add("password");
options.Mask.Fields.Add("apiKey");
options.Mask.Fields.Add("creditCard");

// Mask specific headers
options.Mask.Headers.Add("authorization");
options.Mask.Headers.Add("x-api-key");
options.Mask.Headers.Add("cookie");
```

### Exception Handling

Configure logging levels for specific exception types:

```csharp
options.ExceptionOptions.SetLogLevel<ValidationException>(LogLevel.Warning);
options.ExceptionOptions.SetLogLevel<InvalidOperationException>(LogLevel.Critical);
options.ExceptionOptions.SetLogLevel<ArgumentException>(LogLevel.Error);
options.ExceptionOptions.SetLogLevel<NullReferenceException>(LogLevel.Critical);
options.ExceptionOptions.SetLogLevel<ArgumentNullException>(LogLevel.Warning);

// Control what exception data gets logged
options.ExceptionOptions.LogExceptions = true;
options.ExceptionOptions.LogInnerExceptions = true;
options.ExceptionOptions.LogStackTrace = true;
options.ExceptionOptions.LogMessage = true;
options.ExceptionOptions.LogSource = false;
options.ExceptionOptions.LogData = false;
```

## 🧾 SQL Server Log Schema

When `AutoCreateTable = true`, OwlLogs automatically creates the following table:

```sql
CREATE TABLE [api_logs] (
    [id] UNIQUEIDENTIFIER NOT NULL,
    [level] INT NOT NULL,
    [method] NVARCHAR(10),
    [path] NVARCHAR(500),
    [status_code] INT,
    [message] NVARCHAR(MAX),
    [correlation_id] NVARCHAR(100),
    [created_at] DATETIME2 NOT NULL
);
```

### Schema Details

| Column | Type | Description |
|--------|------|-------------|
| `id` | UNIQUEIDENTIFIER | Unique log entry identifier |
| `level` | INT | Log level (Info=0, Warning=1, Error=2, Critical=3) |
| `method` | NVARCHAR(10) | HTTP method (GET, POST, PUT, DELETE, etc.) |
| `path` | NVARCHAR(500) | Request path |
| `status_code` | INT | HTTP response status code |
| `message` | NVARCHAR(MAX) | Log message |
| `correlation_id` | NVARCHAR(100) | Correlation ID for request tracking |
| `created_at` | DATETIME2 | Timestamp (UTC) |

## 🧪 Logging Custom Events

You can log events outside the HTTP pipeline using the `IOwlLogsRuntime` service:

```csharp
public class MyService
{
    private readonly IOwlLogsRuntime _owlLogsRuntime;

    public MyService(IOwlLogsRuntime owlLogsRuntime)
    {
        _owlLogsRuntime = owlLogsRuntime;
    }

    public void LogCustomEvent()
    {
        _owlLogsRuntime.Write(new ApiLogEntry
        {
            Method = "CUSTOM",
            Path = "/business-event",
            Level = LogLevel.Info,
            Message = "Business event executed successfully",
            OccurredAt = DateTime.UtcNow
        });
    }
}
```

### Inject in Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOwlLogsRuntime _owlLogsRuntime;

    public OrdersController(IOwlLogsRuntime owlLogsRuntime)
    {
        _owlLogsRuntime = owlLogsRuntime;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        // Your logic here
        
        _owlLogsRuntime.Write(new ApiLogEntry
        {
            Method = "CUSTOM",
            Path = "/orders/create",
            Level = LogLevel.Info,
            Message = $"Order created: {request.OrderId}",
            OccurredAt = DateTime.UtcNow
        });

        return Ok();
    }
}
```

## 🧩 Architecture Overview

OwlLogs follows a modular, extensible architecture:

- **`OwlLogsMiddleware`** – Intercepts HTTP requests, responses, and exceptions
- **`OwlLogsRuntime`** – Manages buffering, batching, and sink orchestration
- **`LogBuffer`** – Thread-safe in-memory queue for storing pending entries
- **`IOwlLogsSink`** – Abstract contract for custom log destinations
- **`OwlLogsBackgroundService`** – Asynchronous background service for periodic flushing

### Data Flow

```
HTTP Request
    ↓
OwlLogsMiddleware (capture)
    ↓
OwlLogsRuntime (buffer)
    ↓
LogBuffer (queue)
    ↓
OwlLogsBackgroundService (batch & flush)
    ↓
Multiple Sinks (ILogger, Console, RabbitMQ, SQL Server)
```

## 🔒 Security & Performance

### Security Features

- **Sensitive Data Masking**: Automatically masks passwords, tokens, and API keys
- **Header Security**: Redact authorization headers and cookies
- **Size Limits**: Prevent oversized payloads with `MaxBodySize`
- **GDPR Ready**: Supports data filtering and retention policies

### Performance Features

- **Asynchronous Processing**: Non-blocking background service
- **Buffering & Batching**: Reduce database round-trips
- **Configurable Flushing**: Balance between latency and throughput
- **Low Overhead**: Minimal impact on request pipeline (<100ms)
- **Thread-Safe**: Lock-free concurrent operations where possible

### Best Practices

1. Set appropriate `BufferSize` and `BatchSize` for your traffic volume
2. Enable masking for all sensitive fields and headers
3. Use endpoint filtering to avoid logging unnecessary requests
4. Configure exception levels based on your alerting strategy
5. Monitor background service health for log delivery issues
