# Requirements:

## Primary Goal is to reduce passenger queue in case of cancellation or delay at customer service and makea self serve process

## Thoughts:

1. System interacts with external services and sits in between an incoming and outgoing message updates components
2. Business logic is to be implemented so as to avoid ineligible voucher payments as it would have a monetary impact
3. System is internal without http requests coming directly from users
4. Security and networking has to work in tandem
5. Monitoring and giving out of failed requests needs to be proided to frontline facing agents for misses and discrepancies

### Architecture of components

    Each app service or function should have a seperation of concern and do a process
    Seperate out the processing from queuing and notifcation service
    Orchestration service has been in AKS to scale up based on need and need to configure business process
    No app gateway or front door used as it is msg push architecture without http requests
    We can use kafka streaming and do in process updates as an alternative if Azure is not being used for that
    Cross region deployment for resiliency
    Manage everything with IaC such as Hashicorp Terraform for maintenance and reproducability
    Store all infrastructure as state file in a Storage account


### Security 
    Private endpoints for cosmos DB with no public access. (Data always remains in Azure backbone)
    APIGEE for Sabre with OAuth 2.0 security
    Managed identity to access internal components (System managed)
    Enforce TLS 2.0 for transport layer security
    Have a firewall too but external with Private Endpoint it may not be used
    VNET with seperate subnets and an extensive NSG rule list to control access
    