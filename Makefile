# dotnet --info | grep "SDK"
# restore and build (from root)
# dotnet restore ./API/PuntoVenta_API.sln
# dotnet build   ./API/PuntoVenta_API.sln
# Next steps:
# make up
# make db-sync
# make run

# Create .env file
make env:
	cp .env.example .env

# Start using docker
up:
	docker compose up -d

db-sync:
	dotnet ef database update --project Data --startup-project API

# start Project API from Root
run:
	dotnet run --project API

run-tests:
	dotnet test Business.Tests 2>&1

stop:
	docker compose stop

# Remove all containers
remove:
	docker compose down -v

# Remove all containers and volumes
remove-all:
	docker compose down -v --remove-orphans --rmi all
