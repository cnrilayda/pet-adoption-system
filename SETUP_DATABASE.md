# Veritabanı Kurulum Rehberi

Bu proje Docker ile SQL Server kullanmaktadır. Tüm ekip üyeleri aynı Docker container'ını kullanarak geliştirme yapabilir.

## 🐳 Docker ile Kurulum (Önerilen - Tüm Ekip İçin)

### Gereksinimler
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) yüklü olmalı

### Kurulum Adımları

1. **Docker Desktop'ı başlatın**

2. **SQL Server container'ını başlatın**
   ```bash
   docker-compose up -d
   ```
   Bu komut SQL Server container'ını arka planda başlatacaktır.

3. **Container'ın hazır olmasını bekleyin**
   ```bash
   docker-compose ps
   ```
   Container'ın `healthy` durumuna gelmesi birkaç saniye sürebilir.

4. **appsettings.Development.json dosyasını oluşturun**
   
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

5. **Migration'ları çalıştırın**
   ```bash
   cd PetAdoptionPlatform.API
   dotnet ef database update
   ```

6. **API'yi çalıştırın**
   ```bash
   dotnet run
   ```
   Veritabanı otomatik olarak oluşturulacak ve seed edilecektir.

### Docker Komutları

- **Container'ı başlat**: `docker-compose up -d`
- **Container'ı durdur**: `docker-compose down`
- **Container'ı durdur ve verileri sil**: `docker-compose down -v`
- **Container durumunu kontrol et**: `docker-compose ps`
- **Container loglarını görüntüle**: `docker-compose logs sqlserver`

### Şifre Değiştirme (Opsiyonel)

Varsayılan şifre: `YourStrong@Passw0rd123`

Şifreyi değiştirmek için:
1. `docker-compose.override.yml.example` dosyasını `docker-compose.override.yml` olarak kopyalayın
2. Şifreyi değiştirin
3. Container'ı yeniden başlatın: `docker-compose down && docker-compose up -d`
4. `appsettings.Development.json` dosyasındaki connection string'i de güncelleyin

---

## Diğer Seçenekler

### Seçenek 1: LocalDB Kullanımı

Her ekip üyesi kendi bilgisayarında LocalDB kullanabilir:

1. **SQL Server LocalDB'nin yüklü olduğundan emin olun**
   - Visual Studio ile birlikte gelir
   - Veya [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) indirilebilir

2. **appsettings.Development.json oluşturun ve LocalDB connection string kullanın**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PetAdoptionPlatformDb;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

3. **Migration'ları çalıştırın**
   ```bash
   cd PetAdoptionPlatform.API
   dotnet ef database update
   ```

### Seçenek 2: Bulut Veritabanı Kullanımı

Tüm ekip aynı veritabanını paylaşabilir. Ücretsiz seçenekler:

### Azure SQL Database (Ücretsiz Tier)
1. [Azure Portal](https://portal.azure.com) üzerinden Azure SQL Database oluşturun
2. Connection string'i alın
3. `appsettings.Development.json` dosyasına ekleyin:

```json
{
  "DatabaseOptions": {
    "UseCloudDatabase": true,
    "CloudConnectionString": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=PetAdoptionPlatformDb;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### PostgreSQL (Supabase - Ücretsiz)
1. [Supabase](https://supabase.com) üzerinden ücretsiz hesap oluşturun
2. Yeni proje oluşturun
3. Connection string'i alın
4. `appsettings.Development.json` dosyasına ekleyin

**Not:** PostgreSQL kullanmak için `Program.cs`'de `UseSqlServer` yerine `UseNpgsql` kullanmanız gerekir.

### MySQL (PlanetScale - Ücretsiz)
1. [PlanetScale](https://planetscale.com) üzerinden ücretsiz hesap oluşturun
2. Yeni veritabanı oluşturun
3. Connection string'i alın ve `appsettings.Development.json`'a ekleyin

### Seçenek 3: Environment Variable Kullanımı

Connection string'i environment variable olarak da ayarlayabilirsiniz:

**Windows (PowerShell):**
```powershell
$env:DATABASE_CONNECTION_STRING = "Server=your-server;Database=PetAdoptionPlatformDb;..."
```

**Windows (CMD):**
```cmd
set DATABASE_CONNECTION_STRING=Server=your-server;Database=PetAdoptionPlatformDb;...
```

**Linux/Mac:**
```bash
export DATABASE_CONNECTION_STRING="Server=your-server;Database=PetAdoptionPlatformDb;..."
```


## Önemli Notlar

- `appsettings.Development.json` dosyası git'e commit edilmemelidir (zaten .gitignore'da olmalı)
- Herkes kendi `appsettings.Development.json` dosyasını oluşturmalıdır
- Production ortamında kesinlikle bulut veritabanı kullanılmalıdır
- Connection string'lerde şifreler asla git'e commit edilmemelidir

