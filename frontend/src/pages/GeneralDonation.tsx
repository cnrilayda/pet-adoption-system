import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Heart, ArrowLeft, Building2, AlertCircle } from 'lucide-react';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';
import api from '../lib/api';

export default function GeneralDonation() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [amount, setAmount] = useState<string>('');
  const [message, setMessage] = useState('');
  const [isAnonymous, setIsAnonymous] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!user) {
    navigate('/login');
    return null;
  }

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
        listingId: null, // Genel barınak bağışı için null
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

  return (
    <div className="min-h-screen pt-24 pb-8" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-8 max-w-4xl">
        <button
          onClick={() => navigate('/donations')}
          className="flex items-center gap-2 text-gray-600 hover:text-pink-600 mb-6 font-poppins transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
          Geri Dön
        </button>

        <div className="bg-white rounded-2xl shadow-xl p-6 md:p-12">
          <div className="text-center mb-8">
            <div className="w-20 h-20 bg-gradient-to-br from-pink-500 to-purple-500 rounded-full flex items-center justify-center mx-auto mb-6">
              <Building2 className="w-10 h-10 text-white" />
            </div>
            <h1 className="text-3xl font-poppins font-bold mb-4 text-gray-900">
              Genel Barınak Bağışı
            </h1>
            <p className="text-lg text-gray-600 font-poppins max-w-2xl mx-auto">
              Barınaklarımızın genel ihtiyaçlarına katkıda bulunun. Bağışınız veteriner masrafları, 
              mama, ilaç, barınak bakımı ve diğer acil ihtiyaçlar için kullanılacaktır.
            </p>
          </div>

          <div className="grid md:grid-cols-3 gap-6 mb-8">
            <div className="bg-pink-50 rounded-xl p-6 text-center">
              <Heart className="w-8 h-8 text-pink-600 mx-auto mb-3" fill="currentColor" />
              <h3 className="font-poppins font-semibold mb-2">Veteriner Masrafları</h3>
              <p className="text-sm text-gray-600 font-poppins">
                Yaralı ve hasta hayvanların tedavi giderleri
              </p>
            </div>
            <div className="bg-purple-50 rounded-xl p-6 text-center">
              <Building2 className="w-8 h-8 text-purple-600 mx-auto mb-3" />
              <h3 className="font-poppins font-semibold mb-2">Mama ve Beslenme</h3>
              <p className="text-sm text-gray-600 font-poppins">
                Günlük beslenme ihtiyaçları
              </p>
            </div>
            <div className="bg-pink-50 rounded-xl p-6 text-center">
              <Building2 className="w-8 h-8 text-pink-600 mx-auto mb-3" />
              <h3 className="font-poppins font-semibold mb-2">Barınak Bakımı</h3>
              <p className="text-sm text-gray-600 font-poppins">
                Barınak işletme ve bakım giderleri
              </p>
            </div>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6 max-w-2xl mx-auto">
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
              <div className="flex gap-2 mt-2 flex-wrap">
                {[50, 100, 250, 500, 1000, 2500].map((preset) => (
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
                  Genel Barınak Bağışı Yap
                </>
              )}
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
}

