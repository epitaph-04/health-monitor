# Health Monitor Application

## Overview

The Health Monitor is a real-time microservices health monitoring application built with .NET 10 Blazor. It provides a comprehensive dashboard for monitoring the health status of various microservices across different platforms including HTTP APIs, databases, message queues (SNS, SQS, RabbitMQ), and other services.

## 🏗️ Architecture

### Technology Stack

- **Frontend**: Blazor Server + WebAssembly (.NET 10)
- **Backend**: ASP.NET Core (.NET 10)
- **Real-time Communication**: SignalR
- **Containerization**: Docker
- **Orchestration**: Kubernetes with Helm Charts
- **UI Framework**: Blazor Components with Tailwind CSS

### Key Features

- ✅ **Real-time Health Monitoring**: Live status updates for all monitored services
- 📊 **Historical Data Tracking**: Maintains historical health check results
- 🔄 **Multiple Service Types**: Supports HTTP, Database, SNS, SQS, and RabbitMQ services
- 📡 **SignalR Integration**: Real-time push notifications to connected clients
- 🎯 **Tag-based Organization**: Services can be organized using tags
- ⚡ **Background Service Orchestration**: Automated health check scheduling
- 🚀 **Cloud-native Deployment**: Kubernetes-ready with Helm charts

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK (Preview)
- Docker (for containerization)
- Kubernetes cluster (for deployment)
- Helm (for chart deployment)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd health-monitor
   ```

2. **Install .NET workloads**
   ```bash
   dotnet workload install wasm-tools
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run the application**
   ```bash
   dotnet run --project health-monitor
   ```

5. **Access the application**
   - Local: `http://localhost:5000`
   - Health endpoint: `http://localhost:5000/health`

### Configuration

Health checks are configured in `healthcheckconfig.json`:

```json
[
  {
    "Id": "service-id",
    "Name": "Service Display Name",
    "Type": "Http",
    "Target": "http://service-url/health",
    "ExpectedResponseCode": 200,
    "Method": "GET",
    "TimeoutSeconds": 10,
    "Tag": ["API", "Production"]
  }
]
```

### Supported Service Types

| Type | Description | Configuration |
|------|-------------|---------------|
| **Http** | REST API health checks | URL, method, expected response code |
| **Db** | Database connectivity checks | Connection string, query |
| **Sns** | AWS SNS service checks | Topic ARN, credentials |
| **Sqs** | AWS SQS queue checks | Queue URL, credentials |
| **Rabbitmq** | RabbitMQ connection checks | Connection parameters |

## 🔧 Deployment

### Docker

```bash
# Build image
docker build -t health-monitor .

# Run container
docker run -p 8080:8080 health-monitor
```

### Kubernetes with Helm

```bash
# Deploy using Helm
cd chart
helm install cf-monitor ./cf-monitor
```

## 📁 Project Structure

```
health-monitor/
├── health-monitor/                 # Server-side Blazor application
│   ├── Components/                 # Blazor server components
│   ├── Hub/                       # SignalR hubs
│   ├── Models/                    # Data models
│   ├── Services/                  # Business logic services
│   │   ├── Http/                  # HTTP health check services
│   │   ├── Db/                    # Database health check services
│   │   ├── Sns/                   # SNS health check services
│   │   ├── Sqs/                   # SQS health check services
│   │   └── Rabbitmq/              # RabbitMQ health check services
│   └── Program.cs                 # Application entry point
├── health-monitor.Client/          # Client-side WebAssembly
│   ├── Model/                     # Shared models
│   ├── Pages/                     # Blazor pages/components
│   └── Layout/                    # Layout components
├── chart/                         # Helm charts for Kubernetes
│   └── cf-monitor/               # Health monitor chart
└── Dockerfile                    # Container configuration
```

## 🔄 Real-time Communication Flow

1. **Health Check Orchestrator** runs background health checks
2. **Status Service** aggregates health check results
3. **SignalR Hub** broadcasts updates to connected clients
4. **Blazor Client** receives real-time updates and updates UI

## 📊 Monitoring Features

### Dashboard Views

- **Status Board**: Real-time grid view of all services
- **Dashboard**: Detailed view with historical data and trends

### Status Indicators

- 🟢 **Healthy**: Service is responding normally
- 🟡 **Degraded**: Service is responding but with issues
- 🔴 **Unhealthy**: Service is not responding or failing
- ⚪ **Unknown**: Status not yet determined

### Historical Data

- Response time tracking
- Status change history
- Last check timestamp
- Error message details

## 🛠️ Development

### Adding New Service Types

1. Implement `IHealthCheckService` interface
2. Add service type to `ServiceType` enum
3. Register service in dependency injection
4. Configure in `healthcheckconfig.json`

### Customizing UI

- Modify Blazor components in `/Components` and `/Client/Pages`
- Update styling using Tailwind CSS classes
- Add new dashboard views as needed

## 🔒 Security Considerations

- Service endpoints should be secured appropriately
- Consider implementing authentication for the dashboard
- Use HTTPS in production environments
- Secure sensitive configuration data

## 📈 Performance Optimization

- AOT compilation enabled for WebAssembly
- SIMD and native WebAssembly compilation
- Optimized heap sizes for better performance
- Efficient SignalR connection management

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

[Add license information]

## 🆘 Support

For issues and questions:
- Create an issue in the repository
- Check existing documentation
- Review configuration examples