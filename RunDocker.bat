docker build -t book-store-api .
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development book-store-api