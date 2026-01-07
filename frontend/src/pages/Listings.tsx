import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Filter, MapPin, Heart, X, Plus } from 'lucide-react';
import api from '../lib/api';
import { useAuth } from '../contexts/AuthContext';

interface Listing {
  id: string;
  title: string;
  description: string;
  species: string | null;
  breed: string | null;
  age: number | null;
  gender: string | null;
  city: string | null;
  photoUrls: string[];
  type: number;
  isVaccinated: boolean | null;
  isNeutered: boolean | null;
  ownerName: string;
  isShelter: boolean;
}

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

export default function Listings() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [listings, setListings] = useState<Listing[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showFilterModal, setShowFilterModal] = useState(false);
  const [favorites, setFavorites] = useState<Set<string>>(new Set());
  const [filters, setFilters] = useState({
    species: '',
    breed: '',
    gender: '',
    city: '',
    isShelter: '',
    minAge: '',
    maxAge: '',
    isVaccinated: '',
    isNeutered: '',
    searchTerm: '',
  });
  const [citySuggestions, setCitySuggestions] = useState<string[]>([]);
  const [showCitySuggestions, setShowCitySuggestions] = useState(false);

  useEffect(() => {
    fetchListings();
    if (user) {
      fetchFavorites();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user]);

  const fetchListings = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const params = new URLSearchParams();
      
      params.append('type', '1');
      
      if (filters.species) params.append('species', filters.species);
      if (filters.breed) params.append('breed', filters.breed);
      if (filters.gender) params.append('gender', filters.gender);
      if (filters.city) {
        const matchedCity = TURKISH_CITIES.find(city => 
          normalizeTurkish(city) === normalizeTurkish(filters.city)
        );
        params.append('city', matchedCity || filters.city);
      }
      if (filters.isShelter) params.append('isShelter', filters.isShelter);
      if (filters.minAge) params.append('minAge', filters.minAge);
      if (filters.maxAge) params.append('maxAge', filters.maxAge);
      if (filters.isVaccinated) params.append('isVaccinated', filters.isVaccinated);
      if (filters.isNeutered) params.append('isNeutered', filters.isNeutered);
      if (filters.searchTerm) params.append('searchTerm', filters.searchTerm);

      const response = await api.get(`/petlistings?${params.toString()}`);
      setListings(response.data || []);
      setShowFilterModal(false);
    } catch (error: any) {
      setError('İlanlar yüklenirken bir hata oluştu. Lütfen tekrar deneyin.');
      setListings([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleFilterChange = (key: string, value: string) => {
    setFilters(prev => ({ ...prev, [key]: value }));
    if (key === 'city') {
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

  const clearFilters = () => {
    setFilters({
      species: '',
      breed: '',
      gender: '',
      city: '',
      isShelter: '',
      minAge: '',
      maxAge: '',
      isVaccinated: '',
      isNeutered: '',
      searchTerm: '',
    });
  };

  const hasActiveFilters = Object.values(filters).some(val => val !== '');

  const fetchFavorites = async () => {
    if (!user) return;
    try {
      const response = await api.get('/favorites');
      const favoriteIds = new Set<string>((response.data || []).map((f: any) => f.listingId as string));
      setFavorites(favoriteIds);
    } catch (error) {
      console.error('Favoriler yüklenemedi:', error);
    }
  };

  const handleToggleFavorite = async (listingId: string, e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (!user) {
      navigate('/login');
      return;
    }

    try {
      if (favorites.has(listingId)) {
        await api.delete(`/favorites/${listingId}`);
        setFavorites(prev => {
          const newSet = new Set(prev);
          newSet.delete(listingId);
          return newSet;
        });
      } else {
        await api.post(`/favorites/${listingId}`);
        setFavorites(prev => new Set(prev).add(listingId));
      }
    } catch (error: any) {
      console.error('Favori işlemi başarısız:', error);
      alert(error.response?.data?.message || 'Bir hata oluştu.');
    }
  };

  return (
    <div className="min-h-screen pt-24 pb-8" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-8">
        {/* Header with Create and Filter Buttons */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 mb-8">
          <h1 className="text-4xl md:text-5xl font-poppins font-black text-gray-900">İlanlar</h1>
          <div className="flex gap-3">
            {user && (
              <button
                onClick={() => navigate('/listings/create')}
                className="px-6 py-3 rounded-full font-poppins font-semibold hover:scale-105 transition-transform text-base shadow-xl flex items-center gap-2 bg-pink-600 text-white"
              >
                <Plus className="w-5 h-5" />
                Yeni İlan Oluştur
              </button>
            )}
            <button
              onClick={() => setShowFilterModal(true)}
              className={`px-6 py-3 rounded-full font-poppins font-semibold hover:scale-105 transition-transform text-base shadow-xl flex items-center gap-2 ${
                hasActiveFilters
                  ? 'bg-pink-600 text-white'
                  : 'bg-white text-gray-700 border-2 border-gray-300'
              }`}
            >
              <Filter className="w-5 h-5" />
              Filtrele
              {hasActiveFilters && (
                <span className="bg-white text-pink-600 rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold">
                  {Object.values(filters).filter(v => v !== '').length}
                </span>
              )}
            </button>
          </div>
        </div>

        {/* Filter Modal */}
        {showFilterModal && (
          <>
            <div
              className="fixed inset-0 bg-black/50 z-40"
              onClick={() => setShowFilterModal(false)}
            />
            <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
              <div
                className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-y-auto"
                onClick={(e) => e.stopPropagation()}
              >
                <div className="sticky top-0 bg-white border-b border-gray-200 p-6 flex justify-between items-center rounded-t-2xl">
                  <h2 className="text-2xl font-poppins font-bold text-gray-900">Filtrele</h2>
                  <button
                    onClick={() => setShowFilterModal(false)}
                    className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                  >
                    <X className="w-6 h-6 text-gray-600" />
                  </button>
                </div>

                <div className="p-6 space-y-6">
                  {/* Search Term */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                      Arama
                    </label>
                    <input
                      type="text"
                      className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                      value={filters.searchTerm}
                      onChange={(e) => handleFilterChange('searchTerm', e.target.value)}
                      placeholder="Başlık veya açıklamada ara..."
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
                        value={filters.species}
                        onChange={(e) => handleFilterChange('species', e.target.value)}
                      >
                        <option value="">Tümü</option>
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
                        value={filters.breed}
                        onChange={(e) => handleFilterChange('breed', e.target.value)}
                        placeholder="Irk adı..."
                      />
                    </div>

                    {/* Cinsiyet */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Cinsiyet
                      </label>
                      <select
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={filters.gender}
                        onChange={(e) => handleFilterChange('gender', e.target.value)}
                      >
                        <option value="">Tümü</option>
                        <option value="Erkek">Erkek</option>
                        <option value="Dişi">Dişi</option>
                      </select>
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
                          value={filters.city}
                          onChange={(e) => handleFilterChange('city', e.target.value)}
                          onFocus={() => {
                            if (filters.city.length > 0) {
                              const normalizedSearch = normalizeTurkish(filters.city);
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
                                    handleFilterChange('city', city);
                                    setShowCitySuggestions(false);
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

                    {/* Barınak */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Kaynak
                      </label>
                      <select
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={filters.isShelter}
                        onChange={(e) => handleFilterChange('isShelter', e.target.value)}
                      >
                        <option value="">Tümü</option>
                        <option value="true">Barınak</option>
                        <option value="false">Bireysel</option>
                      </select>
                    </div>

                    {/* Min Yaş */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Minimum Yaş (ay)
                      </label>
                      <input
                        type="number"
                        min="0"
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={filters.minAge}
                        onChange={(e) => handleFilterChange('minAge', e.target.value)}
                        placeholder="0"
                      />
                    </div>

                    {/* Max Yaş */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Maksimum Yaş (ay)
                      </label>
                      <input
                        type="number"
                        min="0"
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={filters.maxAge}
                        onChange={(e) => handleFilterChange('maxAge', e.target.value)}
                        placeholder="100"
                      />
                    </div>

                    {/* Aşılı */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Aşı Durumu
                      </label>
                      <select
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={filters.isVaccinated}
                        onChange={(e) => handleFilterChange('isVaccinated', e.target.value)}
                      >
                        <option value="">Fark Etmez</option>
                        <option value="true">Aşılı</option>
                        <option value="false">Aşısız</option>
                      </select>
                    </div>

                    {/* Kısırlaştırılmış */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Kısırlaştırma Durumu
                      </label>
                      <select
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={filters.isNeutered}
                        onChange={(e) => handleFilterChange('isNeutered', e.target.value)}
                      >
                        <option value="">Fark Etmez</option>
                        <option value="true">Kısırlaştırılmış</option>
                        <option value="false">Kısırlaştırılmamış</option>
                      </select>
                    </div>
                  </div>
                </div>

                <div className="sticky bottom-0 bg-white border-t border-gray-200 p-6 flex justify-end gap-4 rounded-b-2xl">
                  <button
                    onClick={clearFilters}
                    className="px-6 py-2 text-gray-700 font-poppins font-medium hover:text-gray-900 transition-colors"
                  >
                    Filtreleri Temizle
                  </button>
                  <button
                    onClick={fetchListings}
                    className="px-8 py-2 bg-pink-600 text-white rounded-full font-poppins font-semibold hover:scale-105 transition-transform shadow-xl"
                  >
                    Uygula
                  </button>
                </div>
              </div>
            </div>
          </>
        )}

        {/* Listings Grid */}
        {isLoading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600 mx-auto"></div>
            <p className="mt-4 text-gray-600 font-poppins">Yükleniyor...</p>
          </div>
        ) : error ? (
          <div className="text-center py-12 bg-white rounded-2xl shadow-xl">
            <p className="text-red-600 font-poppins mb-4">{error}</p>
            <button
              onClick={fetchListings}
              className="bg-pink-600 text-white px-6 py-2 rounded-full font-poppins font-semibold hover:scale-105 transition-transform"
            >
              Tekrar Dene
            </button>
          </div>
        ) : listings.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-2xl shadow-xl">
            <p className="text-gray-600 font-poppins">İlan bulunamadı</p>
          </div>
        ) : (
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {listings.map((listing) => (
              <Link key={listing.id} to={`/listings/${listing.id}`}>
                <div className="bg-white border border-gray-200 rounded-xl overflow-hidden hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer h-full flex flex-col">
                  <div className="relative">
                    <img
                      src={listing.photoUrls && listing.photoUrls.length > 0 ? listing.photoUrls[0] : 'https://via.placeholder.com/400x300?text=No+Image'}
                      alt={listing.title}
                      className="w-full h-64 object-cover"
                    />
                    <div className="absolute top-4 right-4">
                      <button 
                        className={`bg-white/95 backdrop-blur-sm p-2.5 rounded-full shadow-lg hover:bg-white hover:scale-110 transition-all ${
                          favorites.has(listing.id) ? 'bg-pink-100' : ''
                        }`}
                        onClick={(e) => handleToggleFavorite(listing.id, e)}
                      >
                        <Heart 
                          className={`w-5 h-5 ${favorites.has(listing.id) ? 'text-pink-600 fill-current' : 'text-pink-600'}`} 
                        />
                      </button>
                    </div>
                    <div className="absolute top-4 left-4">
                      <span className="bg-blue-600/90 backdrop-blur-sm text-white px-3 py-1.5 rounded-full text-xs font-poppins font-semibold">
                        Sahiplenme
                      </span>
                    </div>
                  </div>
                  
                  <div className="p-5 flex-1 flex flex-col">
                    <h3 className="text-lg font-poppins font-semibold mb-2 line-clamp-1 text-gray-900">{listing.title}</h3>
                    <p className="text-gray-600 text-sm mb-4 line-clamp-2 font-poppins flex-1">{listing.description}</p>
                    
                    <div className="flex items-center gap-2 text-xs text-gray-500 mb-3 font-poppins flex-wrap">
                      {listing.species && <span className="font-medium">{listing.species}</span>}
                      {listing.breed && <><span className="text-gray-300">•</span><span>{listing.breed}</span></>}
                      {listing.age && <><span className="text-gray-300">•</span><span>{listing.age} ay</span></>}
                      {listing.gender && <><span className="text-gray-300">•</span><span>{listing.gender}</span></>}
                    </div>

                    {listing.city && (
                      <div className="flex items-center gap-2 text-sm text-gray-600 mb-4 font-poppins">
                        <MapPin className="w-4 h-4 text-gray-400" />
                        <span>{listing.city}</span>
                      </div>
                    )}

                    <div className="flex gap-2 flex-wrap mb-4">
                      {listing.isVaccinated && (
                        <span className="px-2.5 py-1 bg-green-50 text-green-700 text-xs rounded-md font-poppins font-medium border border-green-200">
                          Aşılı
                        </span>
                      )}
                      {listing.isNeutered && (
                        <span className="px-2.5 py-1 bg-blue-50 text-blue-700 text-xs rounded-md font-poppins font-medium border border-blue-200">
                          Kısır
                        </span>
                      )}
                      {listing.isShelter && (
                        <span className="px-2.5 py-1 bg-purple-50 text-purple-700 text-xs rounded-md font-poppins font-medium border border-purple-200">
                          Barınak
                        </span>
                      )}
                    </div>

                    <div className="mt-auto pt-4 border-t border-gray-100">
                      <div className="text-xs text-gray-500 font-poppins">
                        <span className="font-medium text-gray-700">{listing.ownerName}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
