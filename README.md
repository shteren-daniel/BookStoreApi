# BookStore API

REST API לניהול חנות ספרים, בנויה עם ASP.NET Core 9.0.

---

## הרצה עם Docker

### דרישות מקדימות
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) מותקן ורץ

### הרצה מהירה

הרץ את הקובץ `RunDocker.bat`:

```bat
RunDocker.bat
```

הקובץ מבצע אוטומטית:
1. בניית ה-Image — `docker build -t book-store-api .`
2. הרצת הקונטיינר — `docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development book-store-api`

### גישה ל-API

לאחר ההרצה, פתח בדפדפן:

```
http://localhost:8080/swagger
```

---

## הרצה ידנית

### בניית Image
```bash
docker build -t book-store-api .
```

### הרצת קונטיינר
```bash
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development book-store-api
```

---

## פקודות שימושיות

### הצגת קונטיינרים פעילים
```bash
docker ps
```

### עצירת קונטיינר
```bash
docker stop <CONTAINER_ID>
```

### צפייה בלוגים
```bash
docker logs <CONTAINER_ID>
```

---

## טכנולוגיות

- **ASP.NET Core 9.0**
- **FluentValidation** — ולידציה של קלט
- **Swashbuckle** — תיעוד Swagger
- **Docker** — containerization
