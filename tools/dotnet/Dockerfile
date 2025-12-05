FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env

WORKDIR /app

RUN dotnet nuget locals all --clear \
	&& dotnet tool install --global dotnet-ef --version 8.0.0

ENV PATH="$PATH:/root/.dotnet/tools"

COPY . /app
