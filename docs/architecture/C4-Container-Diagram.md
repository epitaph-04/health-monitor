# C4 Container Diagram - Health Monitor System

## Container Architecture

The Health Monitor system is composed of several containers that work together to provide real-time health monitoring capabilities.

```mermaid
C4Container
    title Container Diagram - Health Monitor System

    Person(users, "Users", "DevOps, SRE, Developers")

    Container_Boundary(healthMonitorSystem, "Health Monitor System") {
        Container(webApp, "Blazor Web Application", ".NET 10 Blazor Server + WebAssembly", "Serves the web UI and handles user interactions")
        Container(signalRHub, "SignalR Hub", ".NET 10 SignalR", "Real-time communication hub for pushing updates to clients")
        Container(healthCheckOrchestrator, "Health Check Orchestrator", ".NET 10 Background Service", "Coordinates and schedules health checks")
        Container(statusService, "Status Service", ".NET 10 Service", "Aggregates and manages health check results")
        Container(healthCheckServices, "Health Check Services", ".NET 10 Services", "Performs actual health checks (HTTP, DB, SNS, SQS, RabbitMQ)")
        ContainerDb(configStore, "Configuration Store", "JSON File", "Stores health check configurations")
        ContainerDb(statusCache, "Status Cache", "In-Memory", "Caches current and historical status data")
    }

    System_Ext(monitoredServices, "Monitored Services", "CF Backend, CF Scheduler, Audit Logger, MAF Notifier")
    System_Ext(externalSystems, "External Systems", "Databases, Message Queues")
    System_Ext(kubernetes, "Kubernetes", "Container orchestration platform")

    Rel(users, webApp, "Views dashboard, receives real-time updates", "HTTPS/WebSocket")
    
    Rel(webApp, signalRHub, "Establishes SignalR connections", "WebSocket")
    Rel(webApp, statusService, "Requests service status data", "HTTP API")
    
    Rel(signalRHub, statusService, "Gets status updates for broadcasting", "In-Process")
    
    Rel(healthCheckOrchestrator, healthCheckServices, "Triggers health checks", "In-Process")
    Rel(healthCheckOrchestrator, configStore, "Reads health check configurations", "File I/O")
    
    Rel(healthCheckServices, statusService, "Reports health check results", "In-Process")
    Rel(healthCheckServices, monitoredServices, "Performs health checks", "HTTP/TCP")
    Rel(healthCheckServices, externalSystems, "Performs connectivity checks", "TCP/AMQP/HTTP")
    
    Rel(statusService, statusCache, "Stores/retrieves status data", "In-Memory")
    Rel(statusService, signalRHub, "Triggers real-time updates", "In-Process")
    
    Rel(kubernetes, webApp, "Hosts and manages", "Container Runtime")

    UpdateElementStyle(webApp, $fontColor="white", $bgColor="blue", $borderColor="navy")
    UpdateElementStyle(signalRHub, $fontColor="white", $bgColor="orange", $borderColor="darkorange")
    UpdateElementStyle(healthCheckOrchestrator, $fontColor="white", $bgColor="green", $borderColor="darkgreen")
    UpdateElementStyle(statusService, $fontColor="white", $bgColor="purple", $borderColor="darkpurple")
    UpdateElementStyle(healthCheckServices, $fontColor="white", $bgColor="red", $borderColor="darkred")
```

## Container Responsibilities

### Web Application (Blazor)
- **Technology**: .NET 10 Blazor Server + WebAssembly
- **Purpose**: User interface and presentation layer
- **Features**:
  - Dashboard and status board views
  - Real-time UI updates via SignalR
  - Responsive design with Tailwind CSS
  - Both server-side and client-side rendering

### SignalR Hub
- **Technology**: .NET 10 SignalR
- **Purpose**: Real-time communication between server and clients
- **Features**:
  - WebSocket connections for low-latency updates
  - Broadcasting status changes to all connected clients
  - Connection management and scaling

### Health Check Orchestrator
- **Technology**: .NET 10 Background Service
- **Purpose**: Coordinates and schedules health checks
- **Features**:
  - Configurable check intervals
  - Service discovery and registration
  - Error handling and retry logic
  - Performance monitoring

### Status Service
- **Technology**: .NET 10 Service Layer
- **Purpose**: Central status aggregation and management
- **Features**:
  - Current status tracking
  - Historical data management
  - Status change detection
  - Event publishing

### Health Check Services
- **Technology**: .NET 10 Service implementations
- **Purpose**: Actual health check execution
- **Types**:
  - **HTTP Service**: REST API health checks
  - **Database Service**: Database connectivity checks
  - **SNS Service**: AWS SNS health checks
  - **SQS Service**: AWS SQS health checks
  - **RabbitMQ Service**: RabbitMQ connectivity checks

### Configuration Store
- **Technology**: JSON File Storage
- **Purpose**: Health check configuration management
- **Features**:
  - Service definitions
  - Check parameters
  - Timeout settings
  - Tagging system

### Status Cache
- **Technology**: In-Memory Storage
- **Purpose**: Fast access to current and historical status data
- **Features**:
  - Current status cache
  - Historical data buffer
  - Performance optimization
  - Memory-efficient storage

## Communication Patterns

### Real-time Updates
1. Health check services perform checks
2. Results are sent to Status Service
3. Status Service detects changes
4. SignalR Hub broadcasts updates
5. Web clients receive real-time updates

### Data Flow
1. Configuration loaded from JSON file
2. Orchestrator schedules health checks
3. Health check services execute checks
4. Results aggregated in Status Service
5. UI displays current and historical data