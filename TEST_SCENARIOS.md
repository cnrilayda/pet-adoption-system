# Pet Adoption Platform API - Test Senaryoları

## API Base URL
- Development: `https://localhost:7xxx` veya `http://localhost:5xxx`
- Swagger UI: `https://localhost:7xxx/swagger` veya `http://localhost:5xxx/swagger`

## Test Senaryoları

### 1. Authentication - Kullanıcı Kaydı ve Girişi

#### 1.1. Kullanıcı Kaydı (Normal User)
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "Password123",
  "confirmPassword": "Password123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+905551234567",
  "city": "Istanbul",
  "isShelter": false
}
```

**Beklenen Sonuç:** 
- Status: 201 Created
- Response: Token ve kullanıcı bilgileri

#### 1.2. Shelter Kaydı
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "shelter@example.com",
  "password": "Password123",
  "confirmPassword": "Password123",
  "firstName": "Happy",
  "lastName": "Shelter",
  "phoneNumber": "+905559876543",
  "city": "Ankara",
  "isShelter": true
}
```

#### 1.3. Giriş Yap
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@example.com",
  "password": "Password123"
}
```

**Beklenen Sonuç:**
- Status: 200 OK
- Response: Token (Authorization header'da kullanılacak)

---

### 2. Pet Listings - İlan Yönetimi

#### 2.1. Yeni İlan Oluştur (Adoption)
```http
POST /api/petlistings
Authorization: Bearer {token}
Content-Type: application/json

{
  "type": 1,
  "title": "Loving Golden Retriever Looking for Home",
  "description": "A friendly 2-year-old golden retriever looking for a loving family. Very playful and good with children.",
  "species": "Dog",
  "breed": "Golden Retriever",
  "age": 24,
  "gender": "Male",
  "size": "Large",
  "color": "Golden",
  "isVaccinated": true,
  "isNeutered": true,
  "city": "Istanbul",
  "district": "Kadıköy",
  "photoUrls": ["https://example.com/photo1.jpg"]
}
```

**Not:** İlan oluşturulur ama `isApproved: false` olur. Admin onayı bekler.

#### 2.2. İlanları Listele (Filtreleme ile)
```http
GET /api/petlistings?Type=1&Species=Dog&City=Istanbul&Page=1&PageSize=20
```

#### 2.3. İlan Detayı
```http
GET /api/petlistings/{listingId}
```

#### 2.4. Kendi İlanlarımı Listele
```http
GET /api/petlistings/my-listings
Authorization: Bearer {token}
```

---

### 3. Adoption Eligibility Form

#### 3.1. Form Oluştur
```http
POST /api/eligibilityforms
Authorization: Bearer {token}
Content-Type: application/json

{
  "livingType": "Apartment",
  "hasGarden": false,
  "hasBalcony": true,
  "squareMeters": 80,
  "hasPreviousPetExperience": true,
  "previousPetTypes": "Cat, Dog",
  "yearsOfExperience": 5,
  "householdMembers": 2,
  "hasChildren": false,
  "allMembersAgree": true,
  "workSchedule": "Full-time",
  "hoursAwayFromHome": 8,
  "canSpendTimeWithPet": true,
  "canAffordPetExpenses": true,
  "monthlyBudgetForPet": 500,
  "additionalNotes": "I love animals and have experience with pets."
}
```

#### 3.2. Formumu Görüntüle
```http
GET /api/eligibilityforms/my-form
Authorization: Bearer {token}
```

---

### 4. Adoption Applications

#### 4.1. Başvuru Yap
```http
POST /api/applications
Authorization: Bearer {token}
Content-Type: application/json

{
  "listingId": "{listingId}",
  "message": "I would love to adopt this pet. I have experience with dogs and a suitable home."
}
```

**Not:** Eligibility form doldurulmuş olmalı!

#### 4.2. Kendi Başvurularımı Listele
```http
GET /api/applications/my-applications
Authorization: Bearer {token}
```

#### 4.3. İlan Başvurularını Görüntüle (PetOwner)
```http
GET /api/applications/listing/{listingId}
Authorization: Bearer {petOwner-token}
```

#### 4.4. Başvuru Status Güncelle (PetOwner)
```http
PUT /api/applications/{applicationId}/status
Authorization: Bearer {petOwner-token}
Content-Type: application/json

{
  "status": 3,
  "adminNotes": "Great candidate, approved!"
}
```

**Status Değerleri:**
- 1: Pending
- 2: UnderReview
- 3: Accepted
- 4: Rejected
- 5: Completed
- 6: Cancelled

---

### 5. Messaging

#### 5.1. Mesaj Gönder
```http
POST /api/messages
Authorization: Bearer {token}
Content-Type: application/json

{
  "applicationId": "{applicationId}",
  "content": "Hello, I'm very interested in adopting your pet. Can we schedule a meeting?"
}
```

#### 5.2. Konuşma Geçmişi
```http
GET /api/messages/conversation/{applicationId}
Authorization: Bearer {token}
```

#### 5.3. Okunmamış Mesajlar
```http
GET /api/messages/unread
Authorization: Bearer {token}
```

---

### 6. Favorites

#### 6.1. Favorilere Ekle
```http
POST /api/favorites/{listingId}
Authorization: Bearer {token}
```

#### 6.2. Favorilerimi Listele
```http
GET /api/favorites
Authorization: Bearer {token}
```

#### 6.3. Favorilerden Çıkar
```http
DELETE /api/favorites/{listingId}
Authorization: Bearer {token}
```

---

### 7. Donations

#### 7.1. Bağış Yap (İlan için)
```http
POST /api/donations
Authorization: Bearer {token}
Content-Type: application/json

{
  "listingId": "{helpRequestListingId}",
  "amount": 500.00,
  "message": "Hope this helps!",
  "isAnonymous": false
}
```

#### 7.2. Genel Barınak Bağışı
```http
POST /api/donations
Authorization: Bearer {token}
Content-Type: application/json

{
  "amount": 200.00,
  "message": "General support",
  "isAnonymous": true
}
```

#### 7.3. Bağış Özeti
```http
GET /api/donations/summary?listingId={listingId}
```

---

### 8. Admin Features

#### 8.1. İlan Onayla
```http
POST /api/admin/listings/{listingId}/approve
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "isApproved": true,
  "adminNotes": "Looks good, approved!"
}
```

#### 8.2. Onay Bekleyen İlanlar
```http
GET /api/admin/listings/pending
Authorization: Bearer {admin-token}
```

#### 8.3. Kullanıcıları Listele
```http
GET /api/admin/users?IsShelter=true&IsActive=true&Page=1&PageSize=20
Authorization: Bearer {admin-token}
```

#### 8.4. Kullanıcı Durumu Güncelle
```http
PUT /api/admin/users/{userId}/status
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "isBanned": false,
  "isActive": true,
  "isShelterVerified": true,
  "adminNotes": "Shelter verified successfully"
}
```

#### 8.5. Sistem Raporları
```http
GET /api/admin/reports
Authorization: Bearer {admin-token}
```

---

### 9. Stories - Başarı Hikayeleri

#### 9.1. Hikaye Oluştur (Tamamlanan Adoption için)
```http
POST /api/stories
Authorization: Bearer {token}
Content-Type: application/json

{
  "applicationId": "{completedApplicationId}",
  "title": "Max ile Mutlu Bir Hayat",
  "content": "Max'i evlat edindikten sonra hayatımız çok değişti. Çok mutlu ve sağlıklı bir köpek. Herkese tavsiye ederim!",
  "photoUrl": "https://example.com/max-story.jpg"
}
```

**Not:** ApplicationId opsiyonel. Tamamlanmış bir adoption'a bağlı olabilir.

#### 9.2. Onaylanmış Hikayeleri Listele
```http
GET /api/stories
```

#### 9.3. Hikaye Detayı
```http
GET /api/stories/{storyId}
```

#### 9.4. Kendi Hikayelerimi Listele
```http
GET /api/stories/my-stories
Authorization: Bearer {token}
```

#### 9.5. Admin Hikaye Onayla
```http
POST /api/stories/{storyId}/approve
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "status": 1,
  "adminNotes": "Güzel bir hikaye, onaylandı!"
}
```

**Status Değerleri:**
- 0: Pending
- 1: Approved
- 2: Rejected

---

### 10. Complaints - Şikayetler

#### 10.1. Kullanıcı Hakkında Şikayet
```http
POST /api/complaints
Authorization: Bearer {token}
Content-Type: application/json

{
  "targetUserId": "{targetUserId}",
  "reason": "Uygunsuz davranış",
  "description": "Kullanıcı mesajlaşma sırasında uygunsuz dil kullandı ve tehdit etti."
}
```

#### 10.2. İlan Hakkında Şikayet
```http
POST /api/complaints
Authorization: Bearer {token}
Content-Type: application/json

{
  "targetListingId": "{targetListingId}",
  "reason": "Yanlış bilgi",
  "description": "İlanda belirtilen bilgiler gerçekle uyuşmuyor. Pet'in yaşı ve sağlık durumu yanlış."
}
```

#### 10.3. Kendi Şikayetlerimi Listele
```http
GET /api/complaints/my-complaints
Authorization: Bearer {token}
```

#### 10.4. Şikayet Detayı
```http
GET /api/complaints/{complaintId}
Authorization: Bearer {token}
```

**Not:** Sadece şikayet sahibi, hedef kullanıcı veya admin görebilir.

#### 10.5. Admin Tüm Şikayetleri Listele
```http
GET /api/complaints?complainantId={userId}&targetUserId={targetUserId}&targetListingId={listingId}
Authorization: Bearer {admin-token}
```

#### 10.6. Admin Şikayet Durumu Güncelle
```http
PUT /api/complaints/{complaintId}/status
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "status": 2,
  "adminNotes": "İncelendi, gerekli işlemler yapıldı",
  "resolutionNotes": "Kullanıcı uyarıldı ve ilan güncellendi"
}
```

**Status Değerleri:**
- 0: Open
- 1: InProgress
- 2: Resolved
- 3: Rejected

---

### 11. Ratings - Değerlendirmeler

#### 11.1. Değerlendirme Yap (Tamamlanan Adoption için)
```http
POST /api/ratings
Authorization: Bearer {token}
Content-Type: application/json

{
  "applicationId": "{completedApplicationId}",
  "score": 5,
  "comment": "Harika bir deneyim! Pet sahibi çok yardımcı ve profesyoneldi."
}
```

**Not:** 
- Score: 1-5 arası
- Sadece tamamlanmış adoption'lar için değerlendirme yapılabilir
- Adopter ve Owner birbirini değerlendirebilir
- Aynı application için tekrar gönderilirse güncellenir

#### 11.2. Değerlendirme Detayı
```http
GET /api/ratings/{ratingId}
```

#### 11.3. Kullanıcının Aldığı Değerlendirmeler
```http
GET /api/ratings/user/{userId}
```

#### 11.4. Kullanıcı Değerlendirme Özeti
```http
GET /api/ratings/user/{userId}/summary
```

**Response:**
```json
{
  "userId": "...",
  "averageRating": 4.5,
  "totalRatings": 10,
  "rating1": 0,
  "rating2": 1,
  "rating3": 2,
  "rating4": 3,
  "rating5": 4
}
```

#### 11.5. Application için Değerlendirme Kontrolü
```http
GET /api/ratings/application/{applicationId}
Authorization: Bearer {token}
```

**Not:** Sadece değerlendirme yapan kullanıcı görebilir.

---

## Test Senaryosu Akışı

### Senaryo 1: Tam Adoption Süreci

1. **Normal User Kaydı** → Token al
2. **Eligibility Form Doldur** → Form oluştur
3. **PetOwner Kaydı** → Token al
4. **PetOwner İlan Oluştur** → İlan oluştur (pending)
5. **Admin İlan Onayla** → İlanı onayla
6. **Normal User İlanları Görüntüle** → İlanları listele
7. **Normal User Başvuru Yap** → Başvuru oluştur
8. **PetOwner Başvuruları Görüntüle** → Başvuruları listele
9. **PetOwner Başvuruyu Kabul Et** → Status güncelle
10. **Mesajlaşma** → Her iki taraf mesaj göndersin
11. **Adoption Tamamla** → Status Completed yap

### Senaryo 2: Help Request ve Bağış

1. **PetOwner Kaydı** → Token al
2. **Help Request İlanı Oluştur** → Type: 3 (HelpRequest), RequiredAmount belirt
3. **Admin İlan Onayla** → İlanı onayla
4. **Donor Kaydı** → Token al
5. **Bağış Yap** → İlan için bağış yap
6. **Bağış Özeti Görüntüle** → CollectedAmount kontrol et

### Senaryo 3: Adoption Sonrası Hikaye ve Değerlendirme

1. **Adoption Tamamla** → Application status'u Completed yap
2. **Adopter Hikaye Oluştur** → Başarı hikayesi yaz
3. **Admin Hikaye Onayla** → Hikayeyi onayla
4. **Adopter Değerlendirme Yap** → Owner'ı değerlendir (5 yıldız)
5. **Owner Değerlendirme Yap** → Adopter'ı değerlendir (5 yıldız)
6. **Değerlendirme Özetleri Görüntüle** → Her iki kullanıcının da rating summary'sini kontrol et

### Senaryo 4: Şikayet Sistemi

1. **Kullanıcı Şikayet Oluştur** → Bir kullanıcı veya ilan hakkında şikayet
2. **Admin Şikayetleri Görüntüle** → Tüm şikayetleri listele
3. **Admin Şikayet Durumu Güncelle** → InProgress → Resolved
4. **Şikayet Sahibi Durumu Kontrol Et** → Kendi şikayetini görüntüle

---

## Önemli Notlar

1. **Admin Token:** İlk admin kullanıcıyı manuel olarak veritabanında oluşturmanız gerekebilir:
   ```sql
   UPDATE Users SET IsAdmin = 1 WHERE Email = 'admin@example.com'
   ```

2. **İlan Onayı:** Yeni oluşturulan ilanlar otomatik olarak `isApproved: false` olur. Admin onayı gerekir.

3. **Eligibility Form:** Adoption başvurusu yapmadan önce eligibility form doldurulmalı.

4. **Authorization:** Tüm protected endpoint'ler için `Authorization: Bearer {token}` header'ı gerekli.

5. **Swagger UI:** `https://localhost:7xxx/swagger` adresinden tüm endpoint'leri test edebilirsiniz.

6. **Stories:** Hikayeler admin onayı bekler. Pending → Approved/Rejected.

7. **Complaints:** Şikayetler sadece şikayet sahibi, hedef kullanıcı veya admin tarafından görülebilir.

8. **Ratings:** Sadece tamamlanmış adoption'lar için değerlendirme yapılabilir. Aynı application için tekrar gönderilirse güncellenir.

