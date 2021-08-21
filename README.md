[![Build Status](https://johnwatson484.visualstudio.com/John%20D%20Watson/_apis/build/status/Power%20Lines%20Data%20Service?branchName=master)](https://johnwatson484.visualstudio.com/John%20D%20Watson/_build/latest?definitionId=32&branchName=master)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=johnwatson484_power-lines-data-service&metric=alert_status)](https://sonarcloud.io/dashboard?id=johnwatson484_power-lines-data-service)

# Power Lines Data Service
Import base data for analysis

# Prerequisites
- Docker
- Docker Compose

# Running the application
The application is intended to be run and developed within a container.  A set of docker-compose files exist to support this.

## Run production application in container

```
docker-compose -f docker-compose.yaml build
docker-compose -f docker-compose.yaml up
```

## Develop application in container
The service is dependent on a message broker. For development a RabbitMQ container is provided.

```
docker network create power-lines
docker-compose build
docker-compose up
```

## Debug application in container
Running the above development container configuration will include a remote debugger that can be connected to using the example VS Code debug configuration within `./vscode`.

The `${command:pickRemoteProcess}` will prompt for which process to connect to within the container.  

Local changes to code files will automatically trigger a rebuild and restart of the application within the container.

### Running service with arguments
A script, `./scripts/run-service.sh`, is provided to run the containerised service with arguments.

Accepted arguements:  
`--fixtures` import all available fixtures.  
`--results` import available results.  
`--all` import result data from all seasons.  If not supplied then only current season results are imported.

## Run tests
Unit tests are written in NUnit.

```
docker-compose -f docker-compose.test.yaml build
docker-compose -f docker-compose.test.yaml up
```
