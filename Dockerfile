# Use the .NET SDK to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy only the project file to restore dependencies
COPY *.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy the rest of the files
COPY . .

# Build the application in Release mode
RUN dotnet publish -c Release -o /app

# Use the runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the built application from the build stage
COPY --from=build /app .

# Expose port 3000 for the application
EXPOSE 3000

# Run the application
CMD ["dotnet", "OnlineLearnHub.dll"]

