#!/bin/sh
docker exec -it power-lines-data-service dotnet run --project ./PowerLinesDataService $@
