import { useState, useEffect } from 'react';
import { Heart, TrendingUp, Building2, FileHeart } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import api from '../lib/api';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';

interface DonationListing {
  id: string;
  title: string;
  description: string;
  photoUrls: string[];
  city: string;
  requiredAmount: number;
  collectedAmount: number;
  ownerName: string;
  isShelter: boolean;
}

type DonationTab = 'listings' | 'shelters';

export default function Donations() {
  const [activeTab, setActiveTab] = useState<DonationTab>('listings');
  const [listings, setListings] = useState<DonationListing[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const { user } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (activeTab === 'listings') {
      fetchDonationListings();
    }
  }, [activeTab]);

  const fetchDonationListings = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/petlistings?type=3');
      setListings(response.data || []);
    } catch (error) {
      console.error('Bağış ilanları yüklenemedi:', error);
      setListings([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDonateToListing = (listingId: string) => {
    if (!user) {
      navigate('/login');
      return;
    }
    navigate(`/donations/${listingId}`);
  };

  const handleGeneralDonation = () => {
    if (!user) {
      navigate('/login');
      return;
    }
    navigate('/donations/general');
  };

  return (
    <div className="min-h-screen pt-24 pb-8" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-8">
        {/* Header */}
        <div className="text-center mb-12">
          <div className="flex items-center justify-center gap-3 mb-4">
            <Heart className="w-12 h-12 text-pink-600" fill="currentColor" />
            <h1 className="text-4xl md:text-5xl font-poppins font-black bg-gradient-to-r from-pink-600 to-purple-600 bg-clip-text text-transparent">
              Bağış Yap, Hayat Kurtar
            </h1>
          </div>
          <p className="text-lg text-gray-700 max-w-2xl mx-auto font-poppins">
            İster spesifik ilanlara destek olun, ister barınakların genel ihtiyaçlarına katkıda bulunun. 
            Her bağış bir can kurtarır! 🐾
          </p>
        </div>

        {/* Tabs */}
        <div className="flex justify-center mb-8 gap-4">
          <button
            onClick={() => setActiveTab('listings')}
            className={`px-8 py-3 rounded-full font-poppins font-semibold transition-all flex items-center gap-2 ${
              activeTab === 'listings'
                ? 'bg-pink-600 text-white shadow-xl'
                : 'bg-white text-gray-700 border-2 border-gray-300 hover:border-pink-300'
            }`}
          >
            <FileHeart className="w-5 h-5" />
            İlan Bağışları
          </button>
          <button
            onClick={() => setActiveTab('shelters')}
            className={`px-8 py-3 rounded-full font-poppins font-semibold transition-all flex items-center gap-2 ${
              activeTab === 'shelters'
                ? 'bg-pink-600 text-white shadow-xl'
                : 'bg-white text-gray-700 border-2 border-gray-300 hover:border-pink-300'
            }`}
          >
            <Building2 className="w-5 h-5" />
            Genel Barınak Bağışları
          </button>
        </div>

        {activeTab === 'listings' ? (
          isLoading ? (
            <div className="text-center py-12">
              <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600 mx-auto"></div>
              <p className="mt-4 text-gray-600 font-poppins">Yükleniyor...</p>
            </div>
          ) : listings.length === 0 ? (
            <div className="text-center py-12 bg-white rounded-2xl shadow-xl">
              <Heart className="w-16 h-16 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-600 font-poppins">Şu anda bağış bekleyen ilan bulunmuyor</p>
            </div>
          ) : (
            <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
              {listings.map((listing) => {
                const requiredAmount = listing.requiredAmount || 0;
                const collectedAmount = listing.collectedAmount || 0;
                const percentage = requiredAmount > 0 ? (collectedAmount / requiredAmount) * 100 : 0;
                const remaining = requiredAmount - collectedAmount;

                return (
                  <Card key={listing.id} className="overflow-hidden hover:shadow-2xl transition-all hover:scale-[1.02]">
                    <div className="relative">
                      <img
                        src={listing.photoUrls && listing.photoUrls.length > 0 
                          ? listing.photoUrls[0] 
                          : 'https://via.placeholder.com/400x300?text=Yardım+Bekliyor'}
                        alt={listing.title}
                        className="w-full h-48 object-cover"
                      />
                      <div className="absolute top-3 right-3">
                        <span className="bg-pink-600 text-white px-3 py-1.5 rounded-full text-xs font-poppins font-semibold">
                          Yardım İsteği
                        </span>
                      </div>
                      {listing.isShelter && (
                        <div className="absolute top-3 left-3">
                          <span className="bg-purple-600 text-white px-3 py-1.5 rounded-full text-xs font-poppins font-semibold">
                            Barınak
                          </span>
                        </div>
                      )}
                    </div>

                    <div className="p-5">
                      <h3 className="text-xl font-poppins font-bold mb-2 text-gray-900 line-clamp-1">{listing.title}</h3>
                      <p className="text-gray-600 text-sm mb-4 line-clamp-2 font-poppins">{listing.description}</p>

                      {requiredAmount > 0 && (
                        <div className="mb-4">
                          <div className="flex justify-between text-sm mb-2">
                            <span className="text-gray-600 font-poppins">Toplanan</span>
                            <span className="font-bold text-pink-600 font-poppins">
                              {percentage.toFixed(0)}%
                            </span>
                          </div>
                          <div className="w-full bg-gray-200 rounded-full h-3 overflow-hidden">
                            <div
                              className="bg-gradient-to-r from-pink-500 to-purple-500 h-3 rounded-full transition-all duration-500"
                              style={{ width: `${Math.min(percentage, 100)}%` }}
                            ></div>
                          </div>
                          <div className="flex justify-between mt-2 text-sm">
                            <span className="text-gray-700 font-semibold font-poppins">
                              ₺{collectedAmount.toLocaleString('tr-TR')}
                            </span>
                            <span className="text-gray-500 font-poppins">
                              Hedef: ₺{requiredAmount.toLocaleString('tr-TR')}
                            </span>
                          </div>
                          {remaining > 0 && (
                            <p className="text-sm text-pink-600 font-medium mt-1 font-poppins">
                              ₺{remaining.toLocaleString('tr-TR')} daha gerekli
                            </p>
                          )}
                        </div>
                      )}

                      <div className="text-sm text-gray-600 mb-4 font-poppins">
                        {listing.city && <p className="mb-1">📍 {listing.city}</p>}
                        <p>İlan Sahibi: {listing.ownerName}</p>
                      </div>

                      <Button
                        className="w-full !bg-gradient-to-r !from-pink-500 !to-purple-500 hover:!from-pink-600 hover:!to-purple-600"
                        onClick={() => handleDonateToListing(listing.id)}
                      >
                        <Heart className="w-4 h-4 mr-2" fill="currentColor" />
                        Bağış Yap
                      </Button>
                    </div>
                  </Card>
                );
              })}
            </div>
          )
        ) : (
          /* Genel Barınak Bağışları */
          <div className="max-w-4xl mx-auto">
            <div className="bg-white rounded-2xl shadow-xl p-8 md:p-12 text-center">
              <div className="w-20 h-20 bg-gradient-to-br from-pink-500 to-purple-500 rounded-full flex items-center justify-center mx-auto mb-6">
                <Building2 className="w-10 h-10 text-white" />
              </div>
              <h2 className="text-3xl font-poppins font-bold mb-4 text-gray-900">
                Genel Barınak Bağışı
              </h2>
              <p className="text-lg text-gray-600 mb-8 font-poppins max-w-2xl mx-auto">
                Barınaklarımızın genel ihtiyaçlarına katkıda bulunun. Bağışınız veteriner masrafları, 
                mama, ilaç, barınak bakımı ve diğer acil ihtiyaçlar için kullanılacaktır.
              </p>
              
              <div className="grid md:grid-cols-3 gap-6 mb-8">
                <div className="bg-pink-50 rounded-xl p-6">
                  <TrendingUp className="w-8 h-8 text-pink-600 mx-auto mb-3" />
                  <h3 className="font-poppins font-semibold mb-2">Veteriner Masrafları</h3>
                  <p className="text-sm text-gray-600 font-poppins">
                    Yaralı ve hasta hayvanların tedavi giderleri
                  </p>
                </div>
                <div className="bg-purple-50 rounded-xl p-6">
                  <Heart className="w-8 h-8 text-purple-600 mx-auto mb-3" fill="currentColor" />
                  <h3 className="font-poppins font-semibold mb-2">Mama ve Beslenme</h3>
                  <p className="text-sm text-gray-600 font-poppins">
                    Günlük beslenme ihtiyaçları
                  </p>
                </div>
                <div className="bg-pink-50 rounded-xl p-6">
                  <Building2 className="w-8 h-8 text-pink-600 mx-auto mb-3" />
                  <h3 className="font-poppins font-semibold mb-2">Barınak Bakımı</h3>
                  <p className="text-sm text-gray-600 font-poppins">
                    Barınak işletme ve bakım giderleri
                  </p>
                </div>
              </div>

              <Button
                className="px-12 py-4 !bg-gradient-to-r !from-pink-500 !to-purple-500 hover:!from-pink-600 hover:!to-purple-600 text-lg font-poppins font-semibold"
                onClick={handleGeneralDonation}
              >
                <Heart className="w-5 h-5 mr-2" fill="currentColor" />
                Genel Barınak Bağışı Yap
              </Button>
            </div>

            {/* Info Section */}
            <div className="mt-12 bg-white rounded-2xl p-8 shadow-lg">
              <h2 className="text-2xl font-poppins font-bold text-center mb-6 text-pink-600">
                Bağışınız Nasıl Kullanılıyor?
              </h2>
              <div className="grid md:grid-cols-3 gap-6">
                <div className="text-center">
                  <div className="w-16 h-16 bg-pink-100 rounded-full flex items-center justify-center mx-auto mb-3">
                    <TrendingUp className="w-8 h-8 text-pink-600" />
                  </div>
                  <h3 className="font-semibold mb-2 font-poppins">Veteriner Tedavisi</h3>
                  <p className="text-sm text-gray-600 font-poppins">
                    Yaralı ve hasta hayvanların tedavi masrafları
                  </p>
                </div>
                <div className="text-center">
                  <div className="w-16 h-16 bg-purple-100 rounded-full flex items-center justify-center mx-auto mb-3">
                    <Heart className="w-8 h-8 text-purple-600" />
                  </div>
                  <h3 className="font-semibold mb-2 font-poppins">Barınak Masrafları</h3>
                  <p className="text-sm text-gray-600 font-poppins">
                    Barınakların bakım ve işletme giderleri
                  </p>
                </div>
                <div className="text-center">
                  <div className="w-16 h-16 bg-pink-100 rounded-full flex items-center justify-center mx-auto mb-3">
                    <Heart className="w-8 h-8 text-pink-600" fill="currentColor" />
                  </div>
                  <h3 className="font-semibold mb-2 font-poppins">Mama ve Bakım</h3>
                  <p className="text-sm text-gray-600 font-poppins">
                    Günlük beslenme ve hijyen ihtiyaçları
                  </p>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
