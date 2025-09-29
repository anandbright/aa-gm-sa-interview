## Solution Options Pros and Cons

### Option 1 : Process the messages using an Azure Stream Analytics job
    1. Store the data in a database and join to another table that is a cache of the aircraft registration numbers by flight
    2. Create the flight lookup web service as an Azure Function app that queries the database

### Pros:
    1. Quick to begin and start with as most of the componets are avlready available as managed service and you have to configure them to your needs. It's also low-code otpion.
    2. Being fully managed scaling for ASA job and fucntion is fully managed my Azure. We may need to choose plans (expensive one).
    3. Resiliency i.e. high availability is also guaranteed by Azure including integration with different components like Cosmos DB and Event Hub.
    4. Out of order arrival of msgs for aggregation of flight until culmination of ARRIVE msg is also managed and supported.
    5. Monitoring, alerting and security is also easier with some out of box solutions and deep integration with AAD, Key Vault and Log analytics.

### Cons:
    1. It has been observed that in case of high throughput events ASA might not be performant considering it is designed for stream ingesstion and simple transformation or filtering not for handling multiple parallel REST calls.
    2. Directly quering the database is not a good solution for high volume throughput and multiple consumers requesting data.
    3. When providing a service to retrieve the model I am assuming a REST endpoint. REST calls introduce network latency per event. ASA queries are optimized for low-latency event processing.
    4. Error handling : In case of transient failures like timeouts, it may result in drop event if retries fails.
    5. REST calls per event could be expensive when high volumes are present.
    6. Troubleshooting REST output errors in combination with ASA is harder compared to other options.


### Option 2 : Create a .Net Core app for message processing and hosting the flight lookup service
    1. Use Azure Event Hubs SDK to read the messages
    1. Message consumer
        1. Calls the service to lookup the aircraft registration
        1. Queries the flight from the data store
        1. Updates the flight based on the message
        1. Stores the flight
    1. Flight lookup service
        1. Controller in the same application
    1. Deployment
        1. Azure App Services

### Pros - I am more inclined for better control and flexibility
    1. Since it's a custom app we have full control to do complex transformations and business rules.
    2. Message consumer and look up service can be seperate components but in same app to be managed.
    3. Build and debug could be easier with local setup
    4. In this domain driven design (DDD) where there is an aggregation of flight lifecycle events it fits better to needs.

### Cons:
    1. With custom app we will need to configure for availability and scalability of app
    2. This will also result in higher operations overheads with build, storage of compiled files like in a cloudsmith and deployment pipelines.
    3. Would need careful design for message reprocessing.
    4. Controller is in same application which can result in few problems if consumption of msgs and servicing the requests are not handled well.

### Option 3 : A combination of 1 and 2
    1. Message processing with Azure Stream Analytics
    1. .Net Core flight lookup service

### Pros:
    1. ASA handling msg consumption and custom app handling service/API requests is better choice.
    2. Custom handling of msg ingestion and ordering is pushed to ASA job which has built in custom solutions, we don't have to worry about it so less overhead in code maintenance.
    3. API service management is with us so better to contain and solution out based on needs.

## Cons:
    1. Debugging and cross service error handling could be tricky.
    2. Communication between ASA job and our core app can have network latency in high volume environment.
    3. Cost could be higher since running ASA job and also a custome .Net core look up service hosted on either app service and preferably on AKS adds up.
    4. For monitoring we will need to monitor and correlate both systems and it's flow and dependency.


## Thoughts

    1. Buffer first as I want to decouple the stream ingestion from API availability to be consumed by clients
    2. Use a AKS/K8s consumer to call REST API with:
        a. Circuit breakers
        b. Retry logic where possible
        c. Throttling for overwhelimg requests or DDOS attacks
        d. Failures dead letter queue handling for investigation
    3. Clients should have a semaless experince with a single endpoint whether being served from one region or another.
    4. Since different teams from whole enterprise is consuming this service want a secure and control management using Apigee Microgateway which is proven and matured API management having a cert and OAUTH 2.0
    5. The infrastructure should be managed by IaC such as Hasicorp Terraform for maintenance and reproducebaility across multiple environments.

## Proposed solution
    I would like to explore more and discuss the option 2 with some modification.
    The controller should be seperated from msg processing.
    A layer between database and update happening to domain model as the core of the model elements like:
        a. Flight No.
        b. Origin
        c. Destination
        d. Departure Date
        e. Scheduled Departure Time
        f. Estimated Departure Time
        g. Aircraft Registration No.

    remain unchanged and known in advance.
    The dynamic part of msg update happens for ETA, ETD, Update Dept Gate, Depart, Update Arrival Gate and Arrive.

    An in memory layer like Redis cache will provide the in montion flight data to multiple requesters since they would need at that time and avoid costly reads from DB.

    I want to emphasize the role of Arzure Traffic Manager (ATM) here since it holds significance for golbal availability, performance based automatic routing and diaster recovery as whole.

    We set it up with two Azure regions e.g. EAST US and WEST US.We can choose any non US location or add more regions too.

    ATM advantages:
    1. Single endpoint for clients like flightlookup.trafficmanger.aa
    2. Multi region deployment with each hosting an independednt lookup and update service, each having 10 pods
    3. Automatic failover based on dynamic health of servcie endpoints
    4. Health check monitoring for each region
    5. Geographically closest region routing of request for low latency.

    Health check configuration and alerting to team for faster response.
    /health/flightlookupService - to check if API is responding
    /health/database - to check the DB connectivity
    /health/externalDependency - if any needed

    If management of AKS is difficult and need is not too much we can switch back to app service for that too.

    ** High Availability **
    Global Load Balance with Azure Traffic Manager
    Multi region deployment
    Geo-replicated Cosmos DB
    High performance premium tier Redis cache
    Comprehensive monitoring with health checks and app insight

    ** Security **
    Private endpoints for cosmos DB with no public access. (Data always remains in Azure backbone)
    APIGEE onboarding of clients with certs and enterprise level OAuth 2.0 security
    Managed identity to access internal components (System managed)
    Enforce TLS 2.0 for transport layer security
    Have a firewall too but external with Private Endpoint it may not be used
    VNET with seperate subnets and an extensive NSG rule list to control access

    ** Scaling parameters **
    Auto scaling of Azure app services. Select initial tier based on estimated load
    Horizontal scaling of AKS pods starting with minimum 3 as it gives best results and scale up to 10
        consider Memory utilization
        CPU utilization
        backlog size for requests
    Redis cache hit ratio
    Cosmos DB RU (Read units) used and if there is any hot partitioning (unusual high volume of requests wrt other partitions)
        use GUID as partition key for better order of msgs to be processed one after another until ARRIVE
