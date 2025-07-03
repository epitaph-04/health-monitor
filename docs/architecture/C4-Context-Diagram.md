# C4 Context Diagram - Health Monitor System

## System Context

The Health Monitor System provides real-time monitoring and health checking capabilities for microservices infrastructure.

```mermaid
C4Context
    title System Context Diagram - Health Monitor

    Person(devops, "DevOps Engineers", "Monitor service health and respond to incidents")
    Person(sre, "SRE Team", "Ensure system reliability and performance")
    Person(developers, "Development Teams", "Monitor their services during development and production")

    System(healthMonitor, "Health Monitor System", "Real-time health monitoring dashboard for microservices")

    System_Ext(cfBackend, "CF Backend Service", "Campaign workflow backend API")
    System_Ext(cfScheduler, "CF Scheduler Service", "Campaign flow scheduler service")
    System_Ext(auditLogger, "Audit Logger Service", "Audit logging service")
    System_Ext(mafNotifier, "MAF Notifier Service", "Message and notification service")
    
    System_Ext(databases, "Database Systems", "Various database systems (PostgreSQL, MySQL, etc.)")
    System_Ext(messageQueues, "Message Queues", "SNS, SQS, RabbitMQ systems")
    System_Ext(kubernetes, "Kubernetes Cluster", "Container orchestration platform")

    Rel(devops, healthMonitor, "Views health status, receives alerts", "HTTPS/WebSocket")
    Rel(sre, healthMonitor, "Monitors system health, analyzes trends", "HTTPS/WebSocket")
    Rel(developers, healthMonitor, "Checks service status during development", "HTTPS/WebSocket")

    Rel(healthMonitor, cfBackend, "Performs health checks", "HTTP")
    Rel(healthMonitor, cfScheduler, "Performs health checks", "HTTP")
    Rel(healthMonitor, auditLogger, "Performs health checks", "HTTP")
    Rel(healthMonitor, mafNotifier, "Performs health checks", "HTTP")
    
    Rel(healthMonitor, databases, "Database connectivity checks", "TCP/SQL")
    Rel(healthMonitor, messageQueues, "Queue health checks", "AMQP/HTTP")
    
    Rel(kubernetes, healthMonitor, "Hosts and orchestrates", "Container Runtime")

    UpdateElementStyle(healthMonitor, $fontColor="white", $bgColor="blue", $borderColor="navy")
    UpdateElementStyle(devops, $fontColor="white", $bgColor="green", $borderColor="darkgreen")
    UpdateElementStyle(sre, $fontColor="white", $bgColor="green", $borderColor="darkgreen")
    UpdateElementStyle(developers, $fontColor="white", $bgColor="green", $borderColor="darkgreen")
```

## Key Relationships

### Primary Users
- **DevOps Engineers**: Use the system to monitor production services and respond to incidents
- **SRE Team**: Analyze health trends and ensure system reliability
- **Development Teams**: Monitor their services during development and production deployments

### External Systems
- **Microservices**: CF Backend, CF Scheduler, Audit Logger, MAF Notifier
- **Infrastructure**: Databases, Message Queues (SNS, SQS, RabbitMQ)
- **Platform**: Kubernetes cluster for deployment and orchestration

### Communication Patterns
- **Real-time monitoring**: WebSocket connections for live updates
- **Health checks**: HTTP requests to service health endpoints
- **Database checks**: Direct database connectivity validation
- **Message queue checks**: Queue availability and performance testing