import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Heart, MapPin, Trash2, AlertCircle } from 'lucide-react';
import api from '../lib/api';
import Card from '../components/ui/Card';
import { useAuth } from '../contexts/AuthContext';

interface Favorite {
  id: string;
  listingId: string;
  listingTitle: string;
  listingSpecies: string | null;
  listingBreed: string | null;
  primaryPhotoUrl: string | null;
  createdAt: string;
}

export default function Favorites() {
  const { user } = useAuth();
  const [favorites, setFavorites] = useState<Favorite[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (user) {
      fetchFavorites();
    } else {
      setIsLoading(false);
    }
  }, [user]);

  const fetchFavorites = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const response = await api.get('/favorites');
      setFavorites(response.data || []);
    } catch (error: any) {
      console.error('Favoriler yüklenemedi:', error);
      setError('Favoriler yüklenirken bir hata oluştu.');
      setFavorites([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleRemoveFavorite = async (listingId: string, e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (!confirm('Bu ilanı favorilerden kaldırmak istediğinize emin misiniz?')) {
      return;
    }

    try {
      await api.delete(`/favorites/${listingId}`);
      setFavorites(prev => prev.filter(fav => fav.listingId !== listingId));
    } catch (error: any) {
      console.error('Favori kaldırılamadı:', error);
      alert('Favori kaldırılırken bir hata oluştu.');
    }
  };

  if (!user) {
    return (
      <div className="min-h-screen pt-24 pb-8 flex items-center justify-center" style={{ backgroundColor: '#fffcf1' }}>
        <div className="text-center bg-white rounded-2xl shadow-xl p-8 max-w-md mx-4">
          <Heart className="w-16 h-16 text-gray-400 mx-auto mb-4" />
          <h2 className="text-2xl font-poppins font-bold mb-4 text-gray-900">Giriş Yapın</h2>
          <p className="text-gray-600 font-poppins mb-6">
            Favorilerinizi görmek için giriş yapmanız gerekiyor.
          </p>
          <Link to="/login" className="inline-block px-6 py-3 bg-pink-600 text-white rounded-full font-poppins font-semibold hover:bg-pink-700 transition-colors">
            Giriş Yap
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pt-24 pb-8" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-8">
        <div className="mb-8">
          <h1 className="text-4xl md:text-5xl font-poppins font-black text-gray-900 mb-2">Favorilerim</h1>
          <p className="text-gray-600 font-poppins">
            Beğendiğiniz ilanları buradan takip edebilirsiniz
          </p>
        </div>

        {isLoading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600 mx-auto"></div>
            <p className="mt-4 text-gray-600 font-poppins">Yükleniyor...</p>
          </div>
        ) : error ? (
          <div className="text-center py-12 bg-white rounded-2xl shadow-xl">
            <AlertCircle className="w-16 h-16 text-red-500 mx-auto mb-4" />
            <p className="text-red-600 font-poppins mb-4">{error}</p>
            <button
              onClick={fetchFavorites}
              className="bg-pink-600 text-white px-6 py-2 rounded-full font-poppins font-semibold hover:scale-105 transition-transform"
            >
              Tekrar Dene
            </button>
          </div>
        ) : favorites.length === 0 ? (
          <div className="text-center py-12 bg-white rounded-2xl shadow-xl">
            <Heart className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-600 font-poppins mb-4">Henüz favori ilanınız bulunmuyor</p>
            <Link
              to="/listings"
              className="inline-block px-6 py-3 bg-pink-600 text-white rounded-full font-poppins font-semibold hover:bg-pink-700 transition-colors"
            >
              İlanlara Göz At
            </Link>
          </div>
        ) : (
          <div className="grid md:grid-cols-3 gap-6">
            {favorites.map((favorite) => (
              <Link key={favorite.id} to={`/listings/${favorite.listingId}`}>
                <Card className="h-full hover:shadow-2xl transition-all hover:scale-[1.02] relative">
                  <button
                    onClick={(e) => handleRemoveFavorite(favorite.listingId, e)}
                    className="absolute top-3 right-3 z-10 bg-white/90 backdrop-blur-sm p-2 rounded-full shadow-lg hover:bg-white hover:scale-110 transition-transform"
                    title="Favorilerden Kaldır"
                  >
                    <Trash2 className="w-5 h-5 text-red-600" />
                  </button>
                  
                  <div className="relative">
                    <img
                      src={favorite.primaryPhotoUrl || 'https://via.placeholder.com/400x300?text=No+Image'}
                      alt={favorite.listingTitle}
                      className="w-full h-56 object-cover"
                    />
                    <div className="absolute top-3 left-3">
                      <Heart className="w-6 h-6 text-pink-600" fill="currentColor" />
                    </div>
                  </div>
                  
                  <div className="p-5">
                    <h3 className="text-xl font-poppins font-bold mb-2 line-clamp-1 text-gray-900">
                      {favorite.listingTitle}
                    </h3>
                    
                    <div className="flex items-center gap-2 text-sm text-gray-500 mb-3 font-poppins flex-wrap">
                      {favorite.listingSpecies && (
                        <>
                          <span className="font-semibold">{favorite.listingSpecies}</span>
                          <span>•</span>
                        </>
                      )}
                      {favorite.listingBreed && <span>{favorite.listingBreed}</span>}
                    </div>

                    <div className="mt-4 pt-4 border-t border-gray-200 text-xs text-gray-500 font-poppins">
                      {new Date(favorite.createdAt).toLocaleDateString('tr-TR', {
                        day: 'numeric',
                        month: 'long',
                        year: 'numeric',
                        timeZone: 'Europe/Istanbul'
                      })}
                    </div>
                  </div>
                </Card>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

