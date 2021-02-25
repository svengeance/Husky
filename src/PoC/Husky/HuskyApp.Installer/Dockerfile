#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/runtime:5.0.3-windowsservercore-ltsc2019 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["HuskyApp.Installer/HuskyApp.Installer.csproj", "HuskyApp.Installer/"]
COPY ["Husky.Tasks/Husky.Tasks.csproj", "Husky.Tasks/"]
COPY ["Husky.Services/Husky.Services.csproj", "Husky.Services/"]
COPY ["Husky.Core/Husky.Core.csproj", "Husky.Core/"]
COPY ["Husky.Internal.Shared/Husky.Internal.Shared.csproj", "Husky.Internal.Shared/"]
COPY ["Husky.Installer/Husky.Installer.csproj", "Husky.Installer/"]
COPY ["Husky.Dependencies/Husky.Dependencies.csproj", "Husky.Dependencies/"]
RUN dotnet restore "HuskyApp.Installer/HuskyApp.Installer.csproj"
COPY . .
WORKDIR "/src/HuskyApp.Installer"
RUN dotnet build "HuskyApp.Installer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HuskyApp.Installer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HuskyApp.Installer.dll"]