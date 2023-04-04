# Replicated log - iteration 1
# Running the Replicated Log application using Docker Compose

## Prerequisites
- Docker
- Docker Compose

## Setup
1. Clone the repository: `git clone https://github.com/ipavliuk/distributed-systems-course-2022.git`
2. Navigate to the project root directory: `cd ReplicatedLog-Iteration1\Docker`
3. Build the Docker images using Docker Compose: `docker-compose build`

## Running the application
1. Start the containers using Docker Compose: `docker-compose up`
2. Wait for the application to start and for the containers to be ready. You should see output similar to the following:

```secondary1_1 | Hosting environment: Development
secondary1_1 | Content root path: /app
secondary1_1 | Now listening on: http://[::]:80
secondary1_1 | Application started. Press Ctrl+C to shut down.
secondary2_1 | Hosting environment: Development
secondary2_1 | Content root path: /app
secondary2_1 | Now listening on: http://[::]:80
secondary2_1 | Application started. Press Ctrl+C to shut down.
master_1     | info: Microsoft.Hosting.Lifetime[0]
master_1     |       Now listening on: http://[::]:80
master_1     | info: Microsoft.Hosting.Lifetime[0]
master_1     |       Application started. Press Ctrl+C to shut down.

```



3. Open Postman and import the provided collection, `replicated-log.postman_collection.json`
4. Run the "Master Get messages" request to ensure that the master container is running and accessible.
5. Run the "Secondary1 Get messages" and "Secondary2 Get messages" requests to ensure that the secondary containers are running and accessible.
6. Run the "Master Append message" request multiple times to add messages to the replicated log. Or use Runnur to create iteration loop. By default each message would be with id generated `"test-message{{id}}"`


## Shutting down the application
1. Stop the containers using Docker Compose: `docker-compose down`

