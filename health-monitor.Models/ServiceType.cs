namespace health_monitor.Models;

public enum ServiceType
{
    Http,
    Db,
    Sns,
    Sqs,
    Rabbitmq,
    Certificate,
    Resource,
    Network,
    CustomScript
}