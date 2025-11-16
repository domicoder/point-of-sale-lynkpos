# dotnet --info | grep "SDK"
# restore and build (from root)
# dotnet restore ./API/PuntoVenta_API.sln
# dotnet build   ./API/PuntoVenta_API.sln
# Next steps:
# make up
# make db-sync
# make run

# Start using docker
up:
	docker compose up -d

db-sync:
	dotnet ef database update --project Data --startup-project API

# start Project API from Root
run:
	dotnet run --project API

# Down the project
down:
	docker compose down -v

stop:
	docker compose stop