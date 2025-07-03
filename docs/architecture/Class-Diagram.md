# Class Diagram - Health Monitor System

## Domain Model and Class Relationships

This diagram shows the key classes and their relationships within the Health Monitor system.

```mermaid
classDiagram
    %% Core Domain Models
    class Service {
        +string Id
        +string Name
        +string Url
        +ServiceType ServiceType
        +StatusInfo LastCheckStatus
        +Queue~StatusInfo~ HistoricStatus
        +string[] Tag
        +List~Service~ DependentServices
    }

    class StatusInfo {
        +Status Status
        +string Message
        +TimeOnly Time
        +int ResponseTimeMs
        +StatusInfo(Status, string, TimeOnly, int)
    }

    class ApplicationConfiguration {
        +string Id
        +string Name
        +ServiceType Type
        +string Target
        +int ExpectedResponseCode
        +string Method
        +string RequestBody
        +Dictionary~string,string~ Headers
        +string Query
        +int TimeoutSeconds
        +string[] Tag
    }

    class HealthCheckResult {
        +Status Status
        +TimeSpan ResponseTime
        +DateTime LastCheckedUtc
        +string Message
    }

    %% Enumerations
    class ServiceType {
        <<enumeration>>
        +Http
        +Db
        +Sns
        +Sqs
        +Rabbitmq
    }

    class Status {
        <<enumeration>>
        +Unknown
        +Healthy
        +Degraded
        +Unhealthy
    }

    class CheckStatus {
        <<enumeration>>
        +Pending
        +Running
        +Completed
        +Failed
    }

    %% Service Interfaces and Implementations
    class IHealthCheckService {
        <<interface>>
        +string Id
        +string Name
        +ServiceType Type
        +string Target
        +HealthCheckResult LastCheckedResult
        +string[] Tag
        +Task~HealthCheckResult~ CheckHealthAsync()
        +IEnumerable~HealthCheckResult~ GetHistoricalHealthCheckResults()
    }

    class HttpHealthCheckService {
        -HttpClient httpClient
        -ApplicationConfiguration config
        -Queue~HealthCheckResult~ historicalResults
        +HttpHealthCheckService(HttpClient, ApplicationConfiguration)
        +Task~HealthCheckResult~ CheckHealthAsync()
        +IEnumerable~HealthCheckResult~ GetHistoricalHealthCheckResults()
        -Task~HealthCheckResult~ PerformHttpCheck()
        -bool IsSuccessStatusCode(int)
    }

    class DatabaseHealthCheckService {
        -string connectionString
        -ApplicationConfiguration config
        +DatabaseHealthCheckService(ApplicationConfiguration)
        +Task~HealthCheckResult~ CheckHealthAsync()
        -Task~bool~ TestConnection()
    }

    class SqsHealthCheckService {
        -IAmazonSQS sqsClient
        -ApplicationConfiguration config
        +SqsHealthCheckService(IAmazonSQS, ApplicationConfiguration)
        +Task~HealthCheckResult~ CheckHealthAsync()
        -Task~bool~ CheckQueueAccess()
    }

    class SnsHealthCheckService {
        -IAmazonSimpleNotificationService snsClient
        -ApplicationConfiguration config
        +SnsHealthCheckService(IAmazonSimpleNotificationService, ApplicationConfiguration)
        +Task~HealthCheckResult~ CheckHealthAsync()
        -Task~bool~ CheckTopicAccess()
    }

    class RabbitmqHealthCheckService {
        -IConnection connection
        -ApplicationConfiguration config
        +RabbitmqHealthCheckService(ApplicationConfiguration)
        +Task~HealthCheckResult~ CheckHealthAsync()
        -Task~bool~ CheckConnection()
    }

    %% Core Services
    class StatusService {
        -IEnumerable~IHealthCheckService~ services
        +StatusService(IEnumerable~IHealthCheckService~)
        +Service[] GetServices()
        -Service MapToService(IHealthCheckService)
    }

    class ConfigurationService {
        -IConfiguration configuration
        -ILogger logger
        +ConfigurationService(IConfiguration, ILogger)
        +Task LoadHealthCheckConfiguration(string)
        +ApplicationConfiguration[] GetConfigurations()
        +Task~bool~ ValidateConfiguration(ApplicationConfiguration)
    }

    class HealthCheckServiceOrchestrator {
        <<BackgroundService>>
        -IServiceProvider serviceProvider
        -ILogger logger
        -Timer timer
        +HealthCheckServiceOrchestrator(IServiceProvider, ILogger)
        #Task ExecuteAsync(CancellationToken)
        -Task PerformHealthChecks()
        -Task ScheduleNextCheck()
    }

    %% SignalR Hub
    class NotificationHub {
        <<Hub>>
        -StatusService statusService
        +NotificationHub(StatusService)
        +Task OnConnectedAsync()
        +Task OnDisconnectedAsync(Exception)
    }

    class INotificationClient {
        <<interface>>
        +Task ReceiveAllNotification(Service[])
        +Task ReceiveNotification(Service)
    }

    %% Blazor Components (Simplified)
    class DashboardPage {
        <<BlazorComponent>>
        -HubConnection hubConnection
        -Service[] services
        +Task OnInitializedAsync()
        +Task DisposeAsync()
        -Task UpdateServiceStatus(Service)
    }

    class StatusBoardPage {
        <<BlazorComponent>>
        -HubConnection hubConnection
        -Service[] services
        +Task OnInitializedAsync()
        -Task RefreshData()
    }

    %% Relationships
    Service ||--|| StatusInfo : "has current status"
    Service ||--o{ StatusInfo : "has historical status"
    Service ||--|| ServiceType : "of type"
    StatusInfo ||--|| Status : "has status"
    
    ApplicationConfiguration ||--|| ServiceType : "configured for type"
    HealthCheckResult ||--|| Status : "has status"
    
    IHealthCheckService ||--|| HealthCheckResult : "produces"
    IHealthCheckService ||--|| ServiceType : "handles type"
    IHealthCheckService ||--|| ApplicationConfiguration : "configured by"
    
    HttpHealthCheckService ..|> IHealthCheckService : "implements"
    DatabaseHealthCheckService ..|> IHealthCheckService : "implements"
    SqsHealthCheckService ..|> IHealthCheckService : "implements"
    SnsHealthCheckService ..|> IHealthCheckService : "implements"
    RabbitmqHealthCheckService ..|> IHealthCheckService : "implements"
    
    StatusService --> IHealthCheckService : "aggregates"
    StatusService --> Service : "produces"
    
    ConfigurationService --> ApplicationConfiguration : "loads"
    
    HealthCheckServiceOrchestrator --> IHealthCheckService : "orchestrates"
    HealthCheckServiceOrchestrator --> ConfigurationService : "uses"
    
    NotificationHub --> StatusService : "broadcasts from"
    NotificationHub ..|> INotificationClient : "implements client interface"
    
    DashboardPage --> NotificationHub : "connects to"
    StatusBoardPage --> NotificationHub : "connects to"
    DashboardPage --> Service : "displays"
    StatusBoardPage --> Service : "displays"

    %% Styling
    class Service {
        <<Domain Model>>
    }
    class StatusInfo {
        <<Value Object>>
    }
    class ApplicationConfiguration {
        <<Configuration>>
    }
    class HealthCheckResult {
        <<Result>>
    }
```

## Class Descriptions

### Core Domain Models

#### Service
- **Purpose**: Represents a monitored service with its current and historical status
- **Key Properties**:
  - `Id`: Unique identifier for the service
  - `Name`: Display name for the service
  - `LastCheckStatus`: Current status information
  - `HistoricStatus`: Queue of recent status history (limited size)
  - `Tag`: Tags for categorization and filtering

#### StatusInfo
- **Purpose**: Value object representing a point-in-time status
- **Immutable**: Created with constructor, no setters
- **Contains**: Status, message, timestamp, and response time

#### ApplicationConfiguration
- **Purpose**: Configuration model for health check services
- **Features**: Supports various service types with flexible configuration options
- **Validation**: Used by ConfigurationService for validation

#### HealthCheckResult
- **Purpose**: Result object from health check execution
- **Contains**: Status, response time, timestamp, and detailed message

### Service Interfaces and Implementations

#### IHealthCheckService
- **Purpose**: Common interface for all health check implementations
- **Contract**: Defines standard methods for health checking
- **Abstraction**: Allows polymorphic treatment of different service types

#### Health Check Service Implementations
Each implementation handles a specific service type:

- **HttpHealthCheckService**: REST API health checks with HTTP client
- **DatabaseHealthCheckService**: Database connectivity testing
- **SqsHealthCheckService**: AWS SQS queue access verification
- **SnsHealthCheckService**: AWS SNS topic access verification
- **RabbitmqHealthCheckService**: RabbitMQ connection testing

### Core Application Services

#### StatusService
- **Purpose**: Central service for status aggregation
- **Responsibility**: Transforms health check services into UI-friendly Service objects
- **Singleton**: Registered as singleton for consistent state

#### ConfigurationService
- **Purpose**: Manages health check configurations
- **Features**: Loads, validates, and provides access to configurations
- **Extensible**: Supports multiple configuration sources

#### HealthCheckServiceOrchestrator
- **Purpose**: Background service that coordinates health checks
- **Inheritance**: Extends BackgroundService for hosted service functionality
- **Scheduling**: Manages timing and execution of health checks

### Real-time Communication

#### NotificationHub
- **Purpose**: SignalR hub for real-time status broadcasting
- **Events**: Handles connection/disconnection events
- **Integration**: Works with StatusService to push updates

#### INotificationClient
- **Purpose**: Client interface for SignalR communication
- **Methods**: Defines client-side methods for receiving notifications

### UI Components

#### Dashboard and Status Board Pages
- **Purpose**: Blazor components for UI presentation
- **Real-time**: Connected to SignalR for live updates
- **Lifecycle**: Proper initialization and disposal of connections

## Design Patterns Used

### Repository Pattern
- Health check services act as repositories for status data
- Each service encapsulates data access for its specific type

### Strategy Pattern
- Different health check implementations using common interface
- Runtime selection based on service type

### Observer Pattern
- SignalR hub notifies connected clients of status changes
- Loose coupling between status updates and UI

### Factory Pattern
- Service provider creates health check services based on configuration
- Dependency injection container acts as factory

### Background Service Pattern
- HealthCheckServiceOrchestrator runs as hosted background service
- Scheduled execution with proper cancellation support

### Value Object Pattern
- StatusInfo is immutable value object
- Encapsulates related status data

## Key Design Principles

### Single Responsibility
- Each class has a focused, single responsibility
- Clear separation between concerns

### Open/Closed Principle
- Easy to add new health check types without modifying existing code
- Interface-based extensibility

### Dependency Inversion
- High-level modules depend on abstractions
- Dependency injection throughout the application

### Interface Segregation
- Small, focused interfaces like IHealthCheckService
- Clients depend only on what they use