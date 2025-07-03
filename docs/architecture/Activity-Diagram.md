# Activity Diagram - Health Monitor System

## Health Monitoring Process Flow

This diagram shows the activities and process flow within the Health Monitor system, including background health checking, real-time updates, and user interactions.

```mermaid
flowchart TD
    %% System Startup
    A[System Startup] --> B[Load Configuration]
    B --> C[Initialize Health Check Services]
    C --> D[Register Background Services]
    D --> E[Start Health Check Orchestrator]
    
    %% Background Health Checking Process
    E --> F{Timer Triggers?}
    F -->|Yes| G[Get All Health Check Services]
    F -->|No| F
    
    G --> H[For Each Service]
    H --> I[Execute Health Check]
    
    %% Health Check Execution
    I --> J{Service Type?}
    J -->|HTTP| K[HTTP Health Check]
    J -->|Database| L[Database Health Check]
    J -->|SQS| M[SQS Health Check]
    J -->|SNS| N[SNS Health Check]
    J -->|RabbitMQ| O[RabbitMQ Health Check]
    
    %% HTTP Health Check Flow
    K --> K1[Create HTTP Request]
    K1 --> K2[Send Request to Target]
    K2 --> K3{Response Received?}
    K3 -->|Yes| K4[Check Status Code]
    K3 -->|No| K5[Timeout/Error]
    K4 --> K6{Expected Status?}
    K6 -->|Yes| K7[Mark as Healthy]
    K6 -->|No| K8[Mark as Unhealthy]
    K5 --> K9[Mark as Unhealthy]
    K7 --> P[Record Response Time]
    K8 --> P
    K9 --> P
    
    %% Database Health Check Flow
    L --> L1[Create Database Connection]
    L1 --> L2{Connection Successful?}
    L2 -->|Yes| L3[Execute Test Query]
    L2 -->|No| L4[Mark as Unhealthy]
    L3 --> L5{Query Successful?}
    L5 -->|Yes| L6[Mark as Healthy]
    L5 -->|No| L7[Mark as Degraded]
    L4 --> P
    L6 --> P
    L7 --> P
    
    %% Message Queue Health Checks (Simplified)
    M --> M1[Test SQS Queue Access]
    N --> N1[Test SNS Topic Access]
    O --> O1[Test RabbitMQ Connection]
    M1 --> P
    N1 --> P
    O1 --> P
    
    %% Result Processing
    P --> Q[Create Health Check Result]
    Q --> R[Update Historical Data]
    R --> S[Update Status Service]
    S --> T{Status Changed?}
    T -->|Yes| U[Notify SignalR Hub]
    T -->|No| V[Continue to Next Service]
    
    U --> W[Broadcast to Connected Clients]
    W --> V
    V --> X{More Services?}
    X -->|Yes| H
    X -->|No| Y[Schedule Next Check]
    Y --> F
    
    %% User Interaction Flow
    Z[User Opens Dashboard] --> AA[Load Blazor Application]
    AA --> BB[Initialize SignalR Connection]
    BB --> CC[Request Current Status]
    CC --> DD[Display Service Dashboard]
    
    %% Real-time Updates
    W --> EE[Client Receives Update]
    EE --> FF[Update UI Component]
    FF --> GG[Animate Status Change]
    GG --> HH[Update Historical Chart]
    
    %% User Actions
    DD --> II{User Action?}
    II -->|View Details| JJ[Navigate to Service Details]
    II -->|Filter Services| KK[Apply Tag Filters]
    II -->|Refresh| LL[Request Manual Refresh]
    II -->|Switch View| MM[Toggle Dashboard/Status Board]
    
    JJ --> NN[Display Detailed Service Info]
    KK --> OO[Update Filtered View]
    LL --> CC
    MM --> PP[Load Alternative View]
    
    %% Error Handling
    I --> QQ{Health Check Failed?}
    QQ -->|Network Error| RR[Log Network Error]
    QQ -->|Timeout| SS[Log Timeout Error]
    QQ -->|Authentication| TT[Log Auth Error]
    QQ -->|Other| UU[Log General Error]
    
    RR --> VV[Set Error Status]
    SS --> VV
    TT --> VV
    UU --> VV
    VV --> S
    
    %% Configuration Management
    WW[Configuration Change] --> XX[Hot Reload Config]
    XX --> YY[Validate New Config]
    YY --> ZZ{Valid Config?}
    ZZ -->|Yes| AAA[Update Services]
    ZZ -->|No| BBB[Log Config Error]
    AAA --> CCC[Restart Health Checks]
    BBB --> DDD[Keep Existing Config]
    
    %% Styling
    classDef startEnd fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef process fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef decision fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef healthCheck fill:#e8f5e8,stroke:#2e7d32,stroke-width:2px
    classDef userAction fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    classDef error fill:#ffebee,stroke:#c62828,stroke-width:2px
    classDef realTime fill:#e0f2f1,stroke:#00695c,stroke-width:2px
    
    class A,Z,WW startEnd
    class B,C,D,E,G,H,I,P,Q,R,S,W,AA,BB,CC,DD,XX,YY,AAA,CCC process
    class F,J,K3,K6,L2,L5,T,X,II,QQ,ZZ decision
    class K,L,M,N,O,K1,K2,K4,K5,K7,K8,K9,L1,L3,L4,L6,L7,M1,N1,O1 healthCheck
    class JJ,KK,LL,MM,NN,OO,PP userAction
    class RR,SS,TT,UU,VV,BBB,DDD error
    class U,EE,FF,GG,HH realTime
```

## Process Descriptions

### 1. System Initialization
- **Configuration Loading**: Read health check configurations from JSON file
- **Service Registration**: Register all health check services with dependency injection
- **Background Service**: Start the orchestrator as a hosted background service
- **Timer Setup**: Initialize periodic health check scheduling

### 2. Health Check Orchestration
- **Scheduled Execution**: Timer-based triggering of health check cycles
- **Service Discovery**: Enumerate all registered health check services
- **Parallel Execution**: Execute health checks concurrently where possible
- **Result Aggregation**: Collect and process all health check results

### 3. Service-Specific Health Checks

#### HTTP Health Checks
1. Construct HTTP request with configured parameters
2. Send request to target endpoint
3. Evaluate response status code and timing
4. Handle network errors and timeouts
5. Record success/failure with response time

#### Database Health Checks
1. Establish database connection
2. Execute test query or connection validation
3. Measure connection and query response times
4. Classify as healthy, degraded, or unhealthy
5. Properly dispose of connections

#### Message Queue Health Checks
1. **SQS**: Test queue access and permissions
2. **SNS**: Verify topic access and publishing capabilities
3. **RabbitMQ**: Test connection and exchange availability
4. Handle authentication and authorization errors

### 4. Status Management
- **Result Processing**: Transform health check results into status objects
- **Historical Tracking**: Maintain rolling window of historical status data
- **Change Detection**: Identify status transitions for notification
- **State Persistence**: Maintain current status in memory cache

### 5. Real-time Communication
- **SignalR Broadcasting**: Push status updates to connected clients
- **Connection Management**: Handle client connections and disconnections
- **Group Management**: Support for targeted notifications (if implemented)
- **Fallback Handling**: Graceful degradation when WebSocket is unavailable

### 6. User Interface Interactions

#### Dashboard Navigation
- **Initial Load**: Establish SignalR connection and load current status
- **Real-time Updates**: Receive and apply status updates without page refresh
- **Interactive Features**: Filtering, sorting, and detailed views
- **Responsive Design**: Adapt to different screen sizes and devices

#### User Actions
- **Service Details**: Drill down into individual service health data
- **Filtering**: Apply tag-based filters to focus on specific services
- **Manual Refresh**: Force immediate status update
- **View Switching**: Toggle between dashboard and status board layouts

### 7. Error Handling and Resilience

#### Health Check Errors
- **Network Failures**: Handle connectivity issues and DNS resolution
- **Timeouts**: Manage request timeouts and circuit breaking
- **Authentication**: Handle credential and permission issues
- **Service Unavailability**: Graceful handling of temporarily unavailable services

#### System Resilience
- **Configuration Validation**: Prevent invalid configurations from breaking the system
- **Hot Reloading**: Support configuration changes without system restart
- **Logging**: Comprehensive logging for debugging and monitoring
- **Graceful Degradation**: Continue monitoring available services when others fail

### 8. Configuration Management
- **Dynamic Updates**: Support for runtime configuration changes
- **Validation**: Ensure configuration integrity before applying changes
- **Rollback**: Ability to revert to previous working configuration
- **Environment-specific**: Support for different configurations per environment

## Key Activities and Decision Points

### Critical Decision Points
1. **Service Type Selection**: Route to appropriate health check implementation
2. **Status Change Detection**: Determine if notification is required
3. **Error Classification**: Categorize failures for appropriate handling
4. **Configuration Validation**: Ensure safe application of configuration changes

### Parallel Activities
- Multiple health checks execute concurrently
- Real-time updates occur independent of health check cycles
- User interactions happen asynchronously from background processes
- Configuration management operates independently

### Error Recovery
- Automatic retry mechanisms for transient failures
- Circuit breaker pattern for consistently failing services
- Fallback to cached status when real-time updates fail
- Graceful degradation of functionality during partial system failures

## Performance Considerations
- **Concurrent Execution**: Health checks run in parallel to minimize total execution time
- **Caching**: Status results cached to reduce load during high-traffic periods
- **Efficient Broadcasting**: SignalR optimizations for large numbers of connected clients
- **Resource Management**: Proper disposal of connections and resources