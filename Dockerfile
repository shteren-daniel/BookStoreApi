# שלב Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# העתקת קבצי פרויקט והחזרת dependencies
COPY *.csproj ./
RUN dotnet restore

# העתקת שאר הקוד ו-build
COPY . ./
RUN dotnet publish -c Release -o out

# שלב Runtime (תמונה קטנה יותר)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "BookStoreApi.dll"]