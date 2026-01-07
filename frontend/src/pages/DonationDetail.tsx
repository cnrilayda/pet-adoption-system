import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Heart, ArrowLeft, AlertCircle } from 'lucide-react';
import api from '../lib/api';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';

interface Listing {
  id: string;
  title: string;
  description: string;
  photoUrls: string[];
  city: string | null;
  requiredAmount: number;
  collectedAmount: number;
  ownerName: string;
}

export default function DonationDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [listing, setListing] = useState<Listing | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [amount, setAmount] = useState<string>('');
  const [message, setMessage] = useState('');
  const [isAnonymous, setIsAnonymous] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (!user) {
      navigate('/login');
      return;
    }
    fetchListing();
  }, [id, user, navigate]);

  const fetchListing = async () => {
    try {
      setIsLoading(true);
      const response = await api.get(`/petlistings/${id}`);
      setListing(response.data);
    } catch (error: any) {
      console.error('İlan yüklenemedi:', error);
      setError('İlan bulunamadı veya yüklenemedi.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!amount || parseFloat(amount) <= 0) {
      setError('Lütfen geçerli bir tutar girin.');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      const response = await api.post('/donations', {
        listingId: id,
        amount: parseFloat(amount),
        message: message || null,
        isAnonymous: isAnonymous,
      });

      alert('Bağışınız başarıyla tamamlandı! Teşekkür ederiz. 💝');
      navigate('/donations');
    } catch (error: any) {
      console.error('Bağış yapılamadı:', error);
      setError(error.response?.data?.message || 'Bağış yapılırken bir hata oluştu. Lütfen tekrar deneyin.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen pt-24 pb-8 flex items-center justify-center" style={{ backgroundColor: '#fffcf1' }}>
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600 mx-auto"></div>
          <p className="mt-4 text-gray-600 font-poppins">Yükleniyor...</p>
        </div>
      </div>
    );
  }

  if (error && !listing) {
    return (
      <div className="min-h-screen pt-24 pb-8 flex items-center justify-center" style={{ backgroundColor: '#fffcf1' }}>
        <div className="text-center bg-white rounded-2xl shadow-xl p-8 max-w-md mx-4">
          <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
          <p className="text-gray-700 font-poppins mb-4">{error}</p>
          <Button onClick={() => navigate('/donations')}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Bağış Sayfasına Dön
          </Button>
        </div>
      </div>
    );
  }

  if (!listing) return null;

  const requiredAmount = listing.requiredAmount || 0;
  const collectedAmount = listing.collectedAmount || 0;
  const percentage = requiredAmount > 0 ? (collectedAmount / requiredAmount) * 100 : 0;
  const remaining = requiredAmount - collectedAmount;

  return (
    <div className="min-h-screen pt-24 pb-8" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-8 max-w-6xl">
        <button
          onClick={() => navigate('/donations')}
          className="flex items-center gap-2 text-gray-600 hover:text-pink-600 mb-6 font-poppins transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
          Geri Dön
        </button>

        <div className="grid md:grid-cols-2 gap-8">
          {/* Listing Info */}
          <div className="bg-white rounded-2xl shadow-xl overflow-hidden">
            <div className="relative">
              <img
                src={listing.photoUrls && listing.photoUrls.length > 0 
                  ? listing.photoUrls[0] 
                  : 'https://via.placeholder.com/600x400?text=Yardım+Bekliyor'}
                alt={listing.title}
                className="w-full h-64 object-cover"
              />
              <div className="absolute top-4 right-4">
                <span className="bg-pink-600 text-white px-4 py-2 rounded-full text-sm font-poppins font-semibold">
                  Yardım İsteği
                </span>
              </div>
            </div>

            <div className="p-6">
              <h1 className="text-2xl font-poppins font-bold mb-4 text-gray-900">{listing.title}</h1>
              <p className="text-gray-600 mb-6 font-poppins whitespace-pre-wrap">{listing.description}</p>

              {requiredAmount > 0 && (
                <div className="mb-6">
                  <div className="flex justify-between text-sm mb-2">
                    <span className="text-gray-600 font-poppins">Toplanan</span>
                    <span className="font-bold text-pink-600 font-poppins">
                      {percentage.toFixed(0)}%
                    </span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-4 overflow-hidden mb-3">
                    <div
                      className="bg-gradient-to-r from-pink-500 to-purple-500 h-4 rounded-full transition-all duration-500"
                      style={{ width: `${Math.min(percentage, 100)}%` }}
                    ></div>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-700 font-semibold font-poppins">
                      ₺{collectedAmount.toLocaleString('tr-TR')}
                    </span>
                    <span className="text-gray-500 font-poppins">
                      Hedef: ₺{requiredAmount.toLocaleString('tr-TR')}
                    </span>
                  </div>
                  {remaining > 0 && (
                    <p className="text-sm text-pink-600 font-medium mt-2 font-poppins">
                      ₺{remaining.toLocaleString('tr-TR')} daha gerekli
                    </p>
                  )}
                </div>
              )}

              <div className="text-sm text-gray-600 font-poppins space-y-2">
                {listing.city && <p>📍 {listing.city}</p>}
                <p>İlan Sahibi: {listing.ownerName}</p>
              </div>
            </div>
          </div>

          {/* Donation Form */}
          <div className="bg-white rounded-2xl shadow-xl p-6 md:p-8">
            <h2 className="text-2xl font-poppins font-bold mb-6 text-gray-900 flex items-center gap-2">
              <Heart className="w-6 h-6 text-pink-600" fill="currentColor" />
              Bağış Yap
            </h2>

            <form onSubmit={handleSubmit} className="space-y-6">
              {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg font-poppins text-sm">
                  {error}
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Bağış Tutarı (₺) *
                </label>
                <input
                  type="number"
                  step="0.01"
                  min="1"
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                  placeholder="Bağış tutarını girin"
                  required
                />
                <div className="flex gap-2 mt-2">
                  {[50, 100, 250, 500, 1000].map((preset) => (
                    <button
                      key={preset}
                      type="button"
                      onClick={() => setAmount(preset.toString())}
                      className="px-3 py-1.5 text-sm bg-gray-100 hover:bg-pink-100 text-gray-700 hover:text-pink-600 rounded-lg font-poppins transition-colors"
                    >
                      ₺{preset}
                    </button>
                  ))}
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Mesajınız (Opsiyonel)
                </label>
                <textarea
                  rows={4}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins resize-none"
                  value={message}
                  onChange={(e) => setMessage(e.target.value)}
                  placeholder="Bağışınızla ilgili bir mesaj yazabilirsiniz..."
                />
              </div>

              <div className="flex items-center">
                <input
                  type="checkbox"
                  id="anonymous"
                  className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                  checked={isAnonymous}
                  onChange={(e) => setIsAnonymous(e.target.checked)}
                />
                <label htmlFor="anonymous" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                  İsmini gizle (Anonim bağış)
                </label>
              </div>

              <Button
                type="submit"
                className="w-full !bg-gradient-to-r !from-pink-500 !to-purple-500 hover:!from-pink-600 hover:!to-purple-600"
                disabled={isSubmitting}
              >
                {isSubmitting ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                    İşleniyor...
                  </>
                ) : (
                  <>
                    <Heart className="w-4 h-4 mr-2" fill="currentColor" />
                    Bağış Yap
                  </>
                )}
              </Button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}

