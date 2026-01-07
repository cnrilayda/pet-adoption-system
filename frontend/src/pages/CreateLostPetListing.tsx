import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AlertCircle, ArrowLeft, Upload, X } from 'lucide-react';
import api from '../lib/api';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';

const TURKISH_CITIES = [
  'Adana', 'Adıyaman', 'Afyonkarahisar', 'Ağrı', 'Amasya', 'Ankara', 'Antalya', 'Artvin',
  'Aydın', 'Balıkesir', 'Bilecik', 'Bingöl', 'Bitlis', 'Bolu', 'Burdur', 'Bursa',
  'Çanakkale', 'Çankırı', 'Çorum', 'Denizli', 'Diyarbakır', 'Edirne', 'Elazığ', 'Erzincan',
  'Erzurum', 'Eskişehir', 'Gaziantep', 'Giresun', 'Gümüşhane', 'Hakkari', 'Hatay', 'Isparta',
  'İçel (Mersin)', 'İstanbul', 'İzmir', 'Kars', 'Kastamonu', 'Kayseri', 'Kırklareli', 'Kırşehir',
  'Kocaeli', 'Konya', 'Kütahya', 'Malatya', 'Manisa', 'Kahramanmaraş', 'Mardin', 'Muğla',
  'Muş', 'Nevşehir', 'Niğde', 'Ordu', 'Rize', 'Sakarya', 'Samsun', 'Siirt',
  'Sinop', 'Sivas', 'Tekirdağ', 'Tokat', 'Trabzon', 'Tunceli', 'Şanlıurfa', 'Uşak',
  'Van', 'Yozgat', 'Zonguldak', 'Aksaray', 'Bayburt', 'Karaman', 'Kırıkkale', 'Batman',
  'Şırnak', 'Bartın', 'Ardahan', 'Iğdır', 'Yalova', 'Karabük', 'Kilis', 'Osmaniye', 'Düzce'
];

// Türkçe karakterleri normalize eden fonksiyon
const normalizeTurkish = (str: string): string => {
  return str
    .replace(/İ/g, 'i')
    .replace(/ı/g, 'i')
    .replace(/I/g, 'i')
    .replace(/Ş/g, 's')
    .replace(/ş/g, 's')
    .replace(/Ç/g, 'c')
    .replace(/ç/g, 'c')
    .replace(/Ö/g, 'o')
    .replace(/ö/g, 'o')
    .replace(/Ü/g, 'u')
    .replace(/ü/g, 'u')
    .replace(/Ğ/g, 'g')
    .replace(/ğ/g, 'g')
    .toLowerCase();
};

export default function CreateLostPetListing() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    species: '',
    breed: '',
    age: '',
    gender: '',
    size: '',
    color: '',
    city: '',
    district: '',
    photoUrls: [] as string[],
  });

  const [citySuggestions, setCitySuggestions] = useState<string[]>([]);
  const [showCitySuggestions, setShowCitySuggestions] = useState(false);
  const [uploadingPhotos, setUploadingPhotos] = useState(false);

  if (!user) {
    navigate('/login');
    return null;
  }

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    
    if (field === 'city') {
      if (value.length > 0) {
        const normalizedSearch = normalizeTurkish(value);
        const filtered = TURKISH_CITIES.filter(city =>
          normalizeTurkish(city).includes(normalizedSearch)
        );
        setCitySuggestions(filtered);
        setShowCitySuggestions(true);
      } else {
        setCitySuggestions([]);
        setShowCitySuggestions(false);
      }
    }
  };

  const handleCitySelect = (city: string) => {
    setFormData(prev => ({ ...prev, city }));
    setShowCitySuggestions(false);
  };

  const handlePhotoUrlAdd = (url: string) => {
    if (url.trim() && !formData.photoUrls.includes(url.trim())) {
      setFormData(prev => ({
        ...prev,
        photoUrls: [...prev.photoUrls, url.trim()]
      }));
    }
  };

  const handlePhotoUrlRemove = (index: number) => {
    setFormData(prev => ({
      ...prev,
      photoUrls: prev.photoUrls.filter((_, i) => i !== index)
    }));
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    setUploadingPhotos(true);
    setError(null);

    try {
      const uploadPromises = Array.from(files).map(async (file) => {
        // Validate file type
        if (!file.type.startsWith('image/')) {
          throw new Error(`${file.name} bir resim dosyası değil.`);
        }
        
        // Validate file size (max 5MB)
        if (file.size > 5 * 1024 * 1024) {
          throw new Error(`${file.name} dosyası 5MB'dan büyük.`);
        }

        const formData = new FormData();
        formData.append('file', file);

        const response = await api.post('/upload/listing-photo', formData, {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        });

        return response.data.url;
      });

      const uploadedUrls = await Promise.all(uploadPromises);
      setFormData(prev => ({
        ...prev,
        photoUrls: [...prev.photoUrls, ...uploadedUrls]
      }));
    } catch (error: any) {
      console.error('Fotoğraf yüklenemedi:', error);
      setError(error.response?.data?.message || error.message || 'Fotoğraflar yüklenirken bir hata oluştu.');
    } finally {
      setUploadingPhotos(false);
      // Reset input
      e.target.value = '';
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!formData.title.trim()) {
      setError('Başlık zorunludur.');
      return;
    }

    if (!formData.description.trim()) {
      setError('Açıklama zorunludur.');
      return;
    }

    try {
      setIsSubmitting(true);

      // Gender mapping: Türkçe -> İngilizce
      const genderMap: { [key: string]: string } = {
        'Erkek': 'Male',
        'Dişi': 'Female',
        'Bilinmiyor': 'Unknown'
      };

      // Size mapping: Türkçe -> İngilizce
      const sizeMap: { [key: string]: string } = {
        'Küçük': 'Small',
        'Orta': 'Medium',
        'Büyük': 'Large'
      };

      const requestData = {
        type: 2, // Lost = 2
        title: formData.title.trim(),
        description: formData.description.trim(),
        species: formData.species || null,
        breed: formData.breed || null,
        age: formData.age ? parseInt(formData.age) : null,
        gender: formData.gender ? genderMap[formData.gender] || formData.gender : null,
        size: formData.size ? sizeMap[formData.size] || formData.size : null,
        color: formData.color || null,
        city: formData.city || null,
        district: formData.district || null,
        photoUrls: formData.photoUrls.length > 0 ? formData.photoUrls : null,
      };

      await api.post('/petlistings', requestData);
      
      alert('Kayıp hayvan ilanınız başarıyla oluşturuldu! Onay sürecinden sonra yayınlanacaktır.');
      navigate('/listings');
    } catch (error: any) {
      console.error('İlan oluşturulamadı:', error);
      setError(error.response?.data?.message || 'İlan oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen pt-24 pb-8" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-8 max-w-4xl">
        <button
          onClick={() => navigate(-1)}
          className="flex items-center gap-2 text-gray-600 hover:text-pink-600 mb-6 font-poppins transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
          Geri Dön
        </button>

        <div className="bg-white rounded-2xl shadow-xl p-6 md:p-8">
          <h1 className="text-3xl font-poppins font-bold mb-6 text-gray-900">
            Kayıp Hayvan İlanı Oluştur
          </h1>

          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg font-poppins text-sm flex items-center gap-2">
                <AlertCircle className="w-5 h-5" />
                {error}
              </div>
            )}

            {/* Başlık */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                Başlık *
              </label>
              <input
                type="text"
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                value={formData.title}
                onChange={(e) => handleInputChange('title', e.target.value)}
                placeholder="Örn: Ankara'da kaybolan siyah-beyaz kedim"
                required
              />
            </div>

            {/* Açıklama */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                Açıklama *
              </label>
              <textarea
                rows={6}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins resize-none"
                value={formData.description}
                onChange={(e) => handleInputChange('description', e.target.value)}
                placeholder="Hayvanınız hakkında detaylı bilgi verin: ne zaman kayboldu, son görüldüğü yer, özellikleri, iletişim bilgileriniz..."
                required
              />
            </div>

            <div className="grid md:grid-cols-2 gap-6">
              {/* Hayvan Türü */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Hayvan Türü
                </label>
                <select
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={formData.species}
                  onChange={(e) => handleInputChange('species', e.target.value)}
                >
                  <option value="">Seçiniz</option>
                  <option value="Kedi">Kedi</option>
                  <option value="Köpek">Köpek</option>
                  <option value="Kuş">Kuş</option>
                  <option value="Diğer">Diğer</option>
                </select>
              </div>

              {/* Irk */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Irk
                </label>
                <input
                  type="text"
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={formData.breed}
                  onChange={(e) => handleInputChange('breed', e.target.value)}
                  placeholder="Irk adı"
                />
              </div>

              {/* Yaş */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Yaş (ay)
                </label>
                <input
                  type="number"
                  min="0"
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={formData.age}
                  onChange={(e) => handleInputChange('age', e.target.value)}
                  placeholder="Yaş (ay cinsinden)"
                />
              </div>

              {/* Cinsiyet */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Cinsiyet
                </label>
                <select
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={formData.gender}
                  onChange={(e) => handleInputChange('gender', e.target.value)}
                >
                  <option value="">Seçiniz</option>
                  <option value="Erkek">Erkek</option>
                  <option value="Dişi">Dişi</option>
                  <option value="Bilinmiyor">Bilinmiyor</option>
                </select>
              </div>

              {/* Boyut */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Boyut
                </label>
                <select
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={formData.size}
                  onChange={(e) => handleInputChange('size', e.target.value)}
                >
                  <option value="">Seçiniz</option>
                  <option value="Küçük">Küçük</option>
                  <option value="Orta">Orta</option>
                  <option value="Büyük">Büyük</option>
                </select>
              </div>

              {/* Renk */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Renk
                </label>
                <input
                  type="text"
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={formData.color}
                  onChange={(e) => handleInputChange('color', e.target.value)}
                  placeholder="Renk"
                />
              </div>

              {/* Şehir */}
              <div className="relative">
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Şehir
                </label>
                <div className="relative">
                  <input
                    type="text"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                    value={formData.city}
                    onChange={(e) => handleInputChange('city', e.target.value)}
                    onFocus={() => {
                      if (formData.city.length > 0) {
                        const normalizedSearch = normalizeTurkish(formData.city);
                        const filtered = TURKISH_CITIES.filter(city =>
                          normalizeTurkish(city).includes(normalizedSearch)
                        );
                        setCitySuggestions(filtered);
                        setShowCitySuggestions(true);
                      } else {
                        setCitySuggestions(TURKISH_CITIES);
                        setShowCitySuggestions(true);
                      }
                    }}
                    onBlur={() => {
                      setTimeout(() => setShowCitySuggestions(false), 200);
                    }}
                    placeholder="Şehir seçin veya arayın..."
                  />
                  {showCitySuggestions && (
                    <div className="absolute z-50 w-full mt-1 bg-white border border-gray-300 rounded-lg shadow-xl max-h-80 overflow-auto">
                      {citySuggestions.length > 0 ? (
                        citySuggestions.map((city, index) => (
                          <div
                            key={index}
                            className="px-4 py-2.5 hover:bg-pink-50 cursor-pointer font-poppins text-gray-700 border-b border-gray-100 last:border-b-0 transition-colors"
                            onMouseDown={(e) => {
                              e.preventDefault();
                              handleCitySelect(city);
                            }}
                          >
                            {city}
                          </div>
                        ))
                      ) : (
                        <div className="px-4 py-3 text-gray-500 font-poppins text-sm">
                          Şehir bulunamadı
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>

              {/* İlçe */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  İlçe
                </label>
                <input
                  type="text"
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={formData.district}
                  onChange={(e) => handleInputChange('district', e.target.value)}
                  placeholder="İlçe"
                />
              </div>
            </div>

            {/* Fotoğraflar */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                Fotoğraflar
              </label>
              <div className="space-y-4">
                {/* Dosya Yükleme */}
                <div>
                  <label className="block w-full">
                    <input
                      type="file"
                      accept="image/*"
                      multiple
                      onChange={handleFileUpload}
                      disabled={uploadingPhotos}
                      className="hidden"
                    />
                    <div className={`w-full px-4 py-3 border-2 border-dashed rounded-lg cursor-pointer transition-colors font-poppins text-center ${
                      uploadingPhotos 
                        ? 'border-gray-300 bg-gray-50 cursor-not-allowed' 
                        : 'border-gray-300 hover:border-pink-500 hover:bg-pink-50'
                    }`}>
                      <Upload className="w-6 h-6 mx-auto mb-2 text-gray-400" />
                      <span className="text-sm text-gray-600 block">
                        {uploadingPhotos ? 'Yükleniyor...' : 'Fotoğraf yüklemek için tıklayın veya sürükleyin'}
                      </span>
                      <p className="text-xs text-gray-500 mt-1">PNG, JPG veya GIF (Max. 5MB - Birden fazla seçebilirsiniz)</p>
                    </div>
                  </label>
                </div>

                {/* URL ile Ekleme (Opsiyonel) */}
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-2 font-poppins">
                    Veya URL ile ekleyin
                  </label>
                  <div className="flex gap-2">
                    <input
                      type="url"
                      className="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins text-sm"
                      placeholder="Fotoğraf URL'si ekleyin"
                      onKeyPress={(e) => {
                        if (e.key === 'Enter') {
                          e.preventDefault();
                          handlePhotoUrlAdd(e.currentTarget.value);
                          e.currentTarget.value = '';
                        }
                      }}
                    />
                    <Button
                      type="button"
                      onClick={(e) => {
                        e.preventDefault();
                        const input = (e.currentTarget.previousElementSibling as HTMLInputElement);
                        if (input) {
                          handlePhotoUrlAdd(input.value);
                          input.value = '';
                        }
                      }}
                      className="!bg-gray-600 hover:!bg-gray-700 text-sm"
                    >
                      Ekle
                    </Button>
                  </div>
                </div>

                {/* Yüklenen Fotoğraflar */}
                {formData.photoUrls.length > 0 && (
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4">
                    {formData.photoUrls.map((url, index) => (
                      <div key={index} className="relative group">
                        <img
                          src={url}
                          alt={`Fotoğraf ${index + 1}`}
                          className="w-full h-32 object-cover rounded-lg border border-gray-200"
                          onError={(e) => {
                            (e.target as HTMLImageElement).src = 'https://via.placeholder.com/400x300?text=Resim+Yüklenemedi';
                          }}
                        />
                        <button
                          type="button"
                          onClick={() => handlePhotoUrlRemove(index)}
                          className="absolute top-2 right-2 p-1 bg-red-500 text-white rounded-full opacity-0 group-hover:opacity-100 transition-opacity"
                          title="Kaldır"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>

            <div className="flex gap-4 pt-4">
              <Button
                type="button"
                onClick={() => navigate(-1)}
                className="flex-1 !bg-gray-600 hover:!bg-gray-700"
              >
                İptal
              </Button>
              <Button
                type="submit"
                className="flex-1 !bg-gradient-to-r !from-pink-500 !to-purple-500 hover:!from-pink-600 hover:!to-purple-600"
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Gönderiliyor...' : 'İlan Oluştur'}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

