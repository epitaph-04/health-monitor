# Deployment Architecture - Health Monitor System

## Kubernetes Deployment Architecture

This diagram shows how the Health Monitor system is deployed on Kubernetes with supporting infrastructure components.

```mermaid
C4Deployment
    title Deployment Diagram - Health Monitor on Kubernetes

    Deployment_Node(k8sCluster, "Kubernetes Cluster", "Container Orchestration Platform") {
        
        Deployment_Node(namespace, "health-monitor Namespace", "Kubernetes Namespace") {
            
            Container(healthMonitorPod, "Health Monitor Pod", "Docker Container", "Main application pod running Blazor app") {
                Component(blazorApp, "Blazor Application", ".NET 10", "Health monitoring web application")
                Component(signalRHub, "SignalR Hub", "Real-time communication", "WebSocket connections")
                Component(healthChecks, "Health Check Services", "Background services", "HTTP, DB, SQS, SNS, RabbitMQ checks")
            }
            
            ContainerDb(configMap, "Configuration ConfigMap", "Kubernetes ConfigMap", "health check configurations and app settings")
            ContainerDb(secrets, "Secrets", "Kubernetes Secret", "API keys, connection strings, credentials")
            
            Container(service, "Kubernetes Service", "ClusterIP", "Internal service discovery and load balancing")
            Container(ingress, "Ingress Controller", "NGINX/Traefik", "External traffic routing and SSL termination")
        }
        
        Deployment_Node(systemNamespace, "kube-system Namespace", "System Components") {
            Container(dnsService, "CoreDNS", "DNS Service", "Service discovery and name resolution")
            Container(kubeProxy, "kube-proxy", "Network Proxy", "Network rules and load balancing")
        }
        
        Deployment_Node(monitoringNamespace, "monitoring Namespace", "Observability Stack") {
            Container(prometheus, "Prometheus", "Metrics Collection", "Collects application and cluster metrics")
            Container(grafana, "Grafana", "Dashboards", "Visualization and alerting")
            Container(alertManager, "AlertManager", "Alert Routing", "Handles alerts from Prometheus")
        }
    }
    
    Deployment_Node(loadBalancer, "Cloud Load Balancer", "External Load Balancer") {
        Container(cloudLB, "Application Load Balancer", "AWS ALB/Azure LB", "Distributes traffic across cluster nodes")
    }
    
    Deployment_Node(externalServices, "External Services", "Monitored Infrastructure") {
        System_Ext(cfBackend, "CF Backend", "Campaign workflow backend service")
        System_Ext(cfScheduler, "CF Scheduler", "Campaign flow scheduler service")
        System_Ext(auditLogger, "Audit Logger", "Audit logging service")
        System_Ext(mafNotifier, "MAF Notifier", "Message and notification service")
        
        SystemDb_Ext(databases, "Databases", "PostgreSQL, MySQL, SQL Server")
        SystemQueue_Ext(messageQueues, "Message Queues", "SNS, SQS, RabbitMQ")
    }
    
    Deployment_Node(cloudServices, "Cloud Services", "AWS/Azure Services") {
        SystemDb_Ext(secretsManager, "Secrets Manager", "AWS Secrets Manager / Azure Key Vault")
        System_Ext(cloudWatch, "CloudWatch / Monitor", "Cloud monitoring and logging")
        System_Ext(registry, "Container Registry", "Docker image storage")
    }
    
    Person(users, "Users", "DevOps, SRE, Developers")
    Person(admin, "Administrators", "System administrators")
    
    %% External connections
    Rel(users, cloudLB, "Access dashboard", "HTTPS")
    Rel(cloudLB, ingress, "Route traffic", "HTTPS")
    
    %% Internal cluster connections
    Rel(ingress, service, "Forward requests", "HTTP")
    Rel(service, healthMonitorPod, "Load balance", "HTTP")
    
    %% Configuration and secrets
    Rel(healthMonitorPod, configMap, "Read configuration", "Volume Mount")
    Rel(healthMonitorPod, secrets, "Read secrets", "Volume Mount")
    Rel(secrets, secretsManager, "Sync secrets", "API")
    
    %% Health check connections
    Rel(healthChecks, cfBackend, "HTTP health checks", "HTTP")
    Rel(healthChecks, cfScheduler, "HTTP health checks", "HTTP")
    Rel(healthChecks, auditLogger, "HTTP health checks", "HTTP")
    Rel(healthChecks, mafNotifier, "HTTP health checks", "HTTP")
    Rel(healthChecks, databases, "Database connectivity", "TCP/SQL")
    Rel(healthChecks, messageQueues, "Queue health checks", "AMQP/HTTPS")
    
    %% Service discovery
    Rel(healthMonitorPod, dnsService, "Service discovery", "DNS")
    
    %% Monitoring
    Rel(prometheus, healthMonitorPod, "Scrape metrics", "HTTP")
    Rel(grafana, prometheus, "Query metrics", "HTTP")
    Rel(alertManager, prometheus, "Receive alerts", "HTTP")
    
    %% Administration
    Rel(admin, k8sCluster, "Manage cluster", "kubectl/API")
    Rel(admin, cloudWatch, "View logs", "HTTPS")
    
    %% CI/CD (implied)
    Rel(registry, healthMonitorPod, "Pull images", "Docker Registry API")

    UpdateElementStyle(healthMonitorPod, $fontColor="white", $bgColor="blue", $borderColor="navy")
    UpdateElementStyle(configMap, $fontColor="white", $bgColor="green", $borderColor="darkgreen")
    UpdateElementStyle(secrets, $fontColor="white", $bgColor="red", $borderColor="darkred")
    UpdateElementStyle(ingress, $fontColor="white", $bgColor="orange", $borderColor="darkorange")
```

## Deployment Components

### Kubernetes Resources

#### Pod Configuration
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: health-monitor
  namespace: health-monitor
spec:
  replicas: 2
  selector:
    matchLabels:
      app: health-monitor
  template:
    metadata:
      labels:
        app: health-monitor
    spec:
      containers:
      - name: health-monitor
        image: health-monitor:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        volumeMounts:
        - name: config-volume
          mountPath: /app/config
        - name: secrets-volume
          mountPath: /app/secrets
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
      volumes:
      - name: config-volume
        configMap:
          name: health-monitor-config
      - name: secrets-volume
        secret:
          secretName: health-monitor-secrets
```

#### Service Configuration
```yaml
apiVersion: v1
kind: Service
metadata:
  name: health-monitor-service
  namespace: health-monitor
spec:
  selector:
    app: health-monitor
  ports:
  - name: http
    port: 80
    targetPort: 8080
    protocol: TCP
  type: ClusterIP
```

#### Ingress Configuration
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: health-monitor-ingress
  namespace: health-monitor
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - health-monitor.company.com
    secretName: health-monitor-tls
  rules:
  - host: health-monitor.company.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: health-monitor-service
            port:
              number: 80
```

### Configuration Management

#### ConfigMap Structure
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: health-monitor-config
  namespace: health-monitor
data:
  appsettings.json: |
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*"
    }
  healthcheckconfig.json: |
    [
      {
        "Id": "cf-backend",
        "Name": "CF Backend",
        "Type": "Http",
        "Target": "http://campaign-workflow-backend/health",
        "ExpectedResponseCode": 200,
        "Method": "GET",
        "TimeoutSeconds": 10,
        "Tag": ["API", "Production"]
      }
    ]
```

#### Secrets Management
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: health-monitor-secrets
  namespace: health-monitor
type: Opaque
data:
  database-connection: <base64-encoded-connection-string>
  aws-access-key: <base64-encoded-access-key>
  aws-secret-key: <base64-encoded-secret-key>
```

## Infrastructure Components

### Load Balancing and Traffic Management
- **Cloud Load Balancer**: Distributes external traffic across cluster nodes
- **Ingress Controller**: Handles SSL termination, routing, and rate limiting
- **Service Mesh** (Optional): Istio or Linkerd for advanced traffic management

### Service Discovery and Networking
- **CoreDNS**: Provides DNS-based service discovery within the cluster
- **kube-proxy**: Manages network rules and load balancing
- **CNI Plugin**: Container Network Interface for pod networking

### Storage and Configuration
- **ConfigMaps**: Non-sensitive configuration data
- **Secrets**: Sensitive configuration like API keys and connection strings
- **Persistent Volumes** (if needed): For storing historical data beyond memory cache

### Monitoring and Observability
- **Prometheus**: Metrics collection from application and cluster
- **Grafana**: Dashboards and visualization
- **AlertManager**: Alert routing and notification management
- **Jaeger/Zipkin** (Optional): Distributed tracing

## Deployment Strategies

### Rolling Updates
```yaml
spec:
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1
      maxSurge: 1
```

### Blue-Green Deployment
- Maintain two identical production environments
- Switch traffic between environments for zero-downtime deployments
- Immediate rollback capability

### Canary Deployment
- Gradual rollout to a subset of users
- Traffic splitting using ingress controller or service mesh
- Automated rollback based on health metrics

## High Availability and Scaling

### Horizontal Pod Autoscaler
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: health-monitor-hpa
  namespace: health-monitor
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: health-monitor
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Pod Disruption Budget
```yaml
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: health-monitor-pdb
  namespace: health-monitor
spec:
  minAvailable: 1
  selector:
    matchLabels:
      app: health-monitor
```

## Security Considerations

### Network Security
- **Network Policies**: Restrict pod-to-pod communication
- **Service Mesh Security**: mTLS between services
- **Ingress Security**: WAF protection and DDoS mitigation

### Pod Security
- **Security Contexts**: Run containers as non-root user
- **Pod Security Standards**: Enforce security policies
- **Resource Limits**: Prevent resource exhaustion attacks

### Secrets Management
- **External Secrets Operator**: Sync secrets from cloud providers
- **RBAC**: Role-based access control for Kubernetes resources
- **Service Accounts**: Minimal permissions for pod operations

## Monitoring and Alerting

### Application Metrics
- Health check success/failure rates
- Response times for monitored services
- SignalR connection counts
- Memory and CPU usage

### Infrastructure Metrics
- Pod restart counts
- Node resource utilization
- Network traffic patterns
- Storage usage

### Alerting Rules
- Service health check failures
- High response times
- Pod crash loops
- Resource exhaustion

## Backup and Disaster Recovery

### Configuration Backup
- GitOps approach for configuration management
- Version-controlled Kubernetes manifests
- Automated backup of ConfigMaps and Secrets

### Disaster Recovery
- Multi-region deployment capability
- Database backups and point-in-time recovery
- Infrastructure as Code for rapid environment recreation

## Cost Optimization

### Resource Management
- Appropriate CPU and memory requests/limits
- Vertical Pod Autoscaler for right-sizing
- Cluster autoscaler for node optimization

### Efficiency Measures
- Use of spot instances where appropriate
- Scheduled scaling for predictable workloads
- Resource sharing and multi-tenancy considerations