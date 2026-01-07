# 🐳 Docker ile Hızlı Başlangıç

Bu proje Docker ile SQL Server kullanmaktadır.

## Hızlı Kurulum

1. **Docker Desktop'ı başlatın**

2. **SQL Server container'ını başlatın**
   ```bash
   docker-compose up -d
   ```

3. **appsettings.Development.json oluşturun**
   
   Windows (PowerShell):
   ```powershell
   Copy-Item PetAdoptionPlatform.API\appsettings.Development.example.json PetAdoptionPlatform.API\appsettings.Development.json
   ```
   
   Windows (CMD):
   ```cmd
   copy PetAdoptionPlatform.API\appsettings.Development.example.json PetAdoptionPlatform.API\appsettings.Development.json
   ```
   
   Linux/Mac:
   ```bash
   cp PetAdoptionPlatform.API/appsettings.Development.example.json PetAdoptionPlatform.API/appsettings.Development.json
   ```

4. **Migration'ları çalıştırın**
   ```bash
   cd PetAdoptionPlatform.API
   dotnet ef database update
   ```

5. **API'yi çalıştırın**
   ```bash
   dotnet run
   ```

## Önemli Notlar

- Container'ın hazır olması birkaç saniye sürebilir
- İlk başlatmada migration'lar otomatik çalışmaz, manuel çalıştırmanız gerekir
- Container durdurmak için: `docker-compose down`
- Verileri silmek için: `docker-compose down -v`

Detaylı bilgi için `SETUP_DATABASE.md` dosyasına bakın.

