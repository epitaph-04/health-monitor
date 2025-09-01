# Alerts Management Feature

This document describes the comprehensive alerts management system implemented for the Health Monitor application.

## Overview

The alerts management system provides a complete solution for configuring, monitoring, and analyzing alert rules and their effectiveness. It consists of two main pages:

1. **Alert Rules Management** (`/alerts`) - Configure and manage alert rules
2. **Alert History** (`/alerts/history`) - View past alerts and effectiveness analytics

## Features

### Alert Rules Management (`/alerts`)

#### Key Features:
- **Overview Dashboard**: Shows total rules, active rules, and critical rules counts
- **Rule Management**: Create, edit, enable/disable, and delete alert rules
- **Filtering**: Filter rules by alert level and status
- **Service Integration**: Select from available services for alert configuration
- **Notification Channels**: Configure email and webhook notification channels
- **Real-time Updates**: Immediate feedback when rules are modified

#### Alert Rule Configuration:
- **Service Selection**: Choose which service to monitor
- **Alert Level**: Info, Warning, Critical, or Emergency
- **Consecutive Failures**: Number of consecutive failures before triggering
- **Time Window**: Duration to consider for consecutive failures (in minutes)
- **Notification Channels**: Email and/or webhook notifications
- **Status**: Enable or disable the rule

### Alert History (`/alerts/history`)

#### Key Features:
- **Effectiveness Analytics**: Response rate, average response time, false positive rate
- **Timeline Visualization**: Chart showing alerts over time by level
- **Historical Data**: Complete list of past alerts with status and response times
- **Filtering Options**: Filter by time period, alert level, and service
- **Data Export**: Export alert history data in various formats
- **Recommendations**: AI-powered suggestions for improving alert effectiveness

#### Analytics Metrics:
- **Total Alerts**: Number of alerts triggered in the selected period
- **Response Rate**: Percentage of alerts that received a response
- **Average Response Time**: Mean time to respond to alerts
- **False Positive Rate**: Percentage of alerts that were false positives

## Technical Implementation

### Backend API Endpoints

#### Alert Rules Management:
```http
GET /api/HealthCheckApi/alerts/rules
POST /api/HealthCheckApi/alerts/rules
```

#### Alert Effectiveness Analytics:
```http
GET /api/AnalyticsApi/alerts/effectiveness?days={period}
POST /api/AnalyticsApi/export
```

### Data Models

#### AlertRule
```csharp
public class AlertRule
{
    public string Id { get; set; }
    public string ServiceId { get; set; }
    public AlertLevel Level { get; set; }
    public int ConsecutiveFailures { get; set; }
    public TimeSpan Duration { get; set; }
    public string[] NotificationChannels { get; set; }
    public bool IsEnabled { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}
```

#### AlertLevel Enum
```csharp
public enum AlertLevel
{
    Info,
    Warning,
    Critical,
    Emergency
}
```

#### AlertEffectivenessReport
```csharp
public class AlertEffectivenessReport
{
    public AlertMetrics Overall { get; set; }
    public Dictionary<AlertLevel, AlertMetrics> ByLevel { get; set; }
    public List<string> Recommendations { get; set; }
}
```

### Frontend Components

#### Pages:
- `Alerts.razor` - Main alerts management page
- `AlertsHistory.razor` - Alert history and analytics page
- `AlertsNavigation.razor` - Navigation component for alerts section

#### Key Features:
- **Responsive Design**: Works on desktop and mobile devices
- **Modern UI**: Clean, intuitive interface using Tailwind CSS
- **Interactive Charts**: Visual analytics using Chart.js
- **Real-time Updates**: Immediate feedback for user actions
- **Data Export**: Download alert data in various formats

### JavaScript Functions

#### Chart Functions:
- `createHealthTrendChart()` - Health score trend visualization
- `createAlertsTimelineChart()` - Alerts timeline by level
- `downloadFile()` - File download utility

## Usage Guide

### Creating an Alert Rule

1. Navigate to `/alerts`
2. Click "Add Alert Rule"
3. Select the service to monitor
4. Choose alert level (Info, Warning, Critical, Emergency)
5. Configure consecutive failures threshold
6. Set time window for monitoring
7. Select notification channels (email/webhook)
8. Enable the rule
9. Click "Create"

### Managing Existing Rules

1. View all rules in the table
2. Use filters to find specific rules
3. Click "Edit" to modify a rule
4. Click "Enable/Disable" to toggle rule status
5. Click "Delete" to remove a rule

### Viewing Alert History

1. Navigate to `/alerts/history`
2. Select time period (7, 30, 90, or 365 days)
3. Use filters to narrow down results
4. View effectiveness metrics
5. Analyze timeline chart
6. Export data if needed

### Analyzing Alert Effectiveness

1. Review the effectiveness overview cards
2. Check response rates and times
3. Identify false positive patterns
4. Read AI-generated recommendations
5. Adjust alert rules based on insights

## Configuration

### Notification Channels

#### Email Configuration:
- Configure SMTP settings in `appsettings.json`
- Set recipient email addresses
- Customize email templates

#### Webhook Configuration:
- Set webhook URL
- Configure authentication if required
- Customize payload format

### Alert Rules Defaults:
- Default consecutive failures: 1
- Default time window: 5 minutes
- Default notification channels: None (must be selected)

## Best Practices

### Alert Rule Configuration:
1. **Start Conservative**: Begin with higher thresholds and reduce gradually
2. **Use Appropriate Levels**: 
   - Info: General monitoring
   - Warning: Performance degradation
   - Critical: Service issues
   - Emergency: System-wide problems
3. **Set Realistic Time Windows**: Consider service characteristics
4. **Monitor False Positives**: Adjust rules based on effectiveness

### Alert Management:
1. **Regular Review**: Check alert effectiveness weekly
2. **Documentation**: Document rule purposes and expected behavior
3. **Testing**: Test new rules in staging environments
4. **Escalation**: Set up proper escalation procedures

## Troubleshooting

### Common Issues:

1. **Alerts Not Triggering**:
   - Check if rule is enabled
   - Verify service is being monitored
   - Confirm threshold settings

2. **Too Many False Positives**:
   - Increase consecutive failures threshold
   - Extend time window
   - Review service behavior patterns

3. **Notification Issues**:
   - Verify email/webhook configuration
   - Check network connectivity
   - Review authentication settings

### Performance Considerations:
- Limit number of active rules per service
- Use appropriate time windows
- Monitor system resources during high alert volumes

## Future Enhancements

### Planned Features:
1. **Advanced Alert Conditions**: Custom expressions and thresholds
2. **Alert Escalation**: Automatic escalation based on response time
3. **Alert Templates**: Predefined rule templates for common scenarios
4. **Machine Learning**: AI-powered alert optimization
5. **Mobile Notifications**: Push notifications for critical alerts
6. **Alert Correlation**: Group related alerts together
7. **Custom Dashboards**: Personalized alert monitoring views

### Integration Opportunities:
1. **Slack/Discord**: Chat platform notifications
2. **PagerDuty**: Incident management integration
3. **ServiceNow**: IT service management integration
4. **Grafana**: Advanced visualization integration

## Security Considerations

1. **Access Control**: Implement role-based access for alert management
2. **Audit Logging**: Log all alert rule changes
3. **Data Privacy**: Ensure alert data doesn't contain sensitive information
4. **Rate Limiting**: Prevent alert spam
5. **Encryption**: Encrypt sensitive alert data in transit and at rest

## Support

For issues or questions regarding the alerts management system:
1. Check the troubleshooting section above
2. Review application logs for error details
3. Contact the development team with specific error messages
4. Provide context about the alert rule configuration and observed behavior