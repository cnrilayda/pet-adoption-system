import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { MapPin, Calendar, Heart, MessageCircle, Share2, ArrowLeft, Flag } from 'lucide-react';
import api from '../lib/api';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';

interface ListingDetail {
  id: string;
  title: string;
  description: string;
  species: string;
  breed: string;
  age: number;
  gender: string;
  size: string;
  color: string;
  city: string;
  district: string;
  photoUrls: string[];
  type: number;
  isVaccinated: boolean;
  isNeutered: boolean;
  ownerName: string;
  ownerId: string;
  isShelter: boolean;
  createdAt: string;
  applicationCount: number;
  favoriteCount: number;
  requiredAmount?: number;
  collectedAmount?: number;
}

export default function ListingDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [listing, setListing] = useState<ListingDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedImage, setSelectedImage] = useState(0);
  const [isFavorite, setIsFavorite] = useState(false);
  const [userApplication, setUserApplication] = useState<any>(null);
  const [showMessageModal, setShowMessageModal] = useState(false);
  const [messageContent, setMessageContent] = useState('');
  const [isSendingMessage, setIsSendingMessage] = useState(false);
  const [showComplaintModal, setShowComplaintModal] = useState(false);
  const [complaintData, setComplaintData] = useState({
    reason: '',
    description: ''
  });
  const [isSubmittingComplaint, setIsSubmittingComplaint] = useState(false);

  useEffect(() => {
    fetchListing();
    if (user && id) {
      checkFavorite();
      checkUserApplication();
    }
  }, [id, user]);

  const fetchListing = async () => {
    try {
      const response = await api.get(`/petlistings/${id}`);
      setListing(response.data);
    } catch (error) {
    } finally {
      setIsLoading(false);
    }
  };

  const handleApply = () => {
    if (!user) {
      navigate('/login');
      return;
    }
    navigate(`/listings/${id}/apply`);
  };

  const checkFavorite = async () => {
    if (!user || !id) return;
    try {
      const response = await api.get(`/favorites/${id}/check`);
      setIsFavorite(response.data.isFavorite);
    } catch (error) {
    }
  };

  const handleFavorite = async () => {
    if (!user) {
      navigate('/login');
      return;
    }
    if (!id) return;

    try {
      if (isFavorite) {
        await api.delete(`/favorites/${id}`);
        setIsFavorite(false);
      } else {
        await api.post(`/favorites/${id}`);
        setIsFavorite(true);
      }
      fetchListing();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Bir hata oluştu.');
    }
  };

  const checkUserApplication = async () => {
    if (!user || !id) return;
    try {
      const response = await api.get('/applications/my-applications');
      const applications = response.data || [];
      const application = applications.find((app: any) => app.listingId === id);
      setUserApplication(application);
    } catch (error) {
    }
  };

  const handleMessage = () => {
    if (!user) {
      navigate('/login');
      return;
    }
    if (userApplication) {
      navigate(`/messages?applicationId=${userApplication.id}`);
    } else {
      setShowMessageModal(true);
    }
  };

  const sendInquiryMessage = async () => {
    if (!messageContent.trim() || !user || !listing || !id) return;

    try {
      setIsSendingMessage(true);
      const applicationResponse = await api.post('/applications', {
        listingId: id,
        message: messageContent
      });

      const applicationId = applicationResponse.data.id;

      try {
        await api.post('/messages', {
          applicationId: applicationId,
          content: messageContent
        });
      } catch (msgError) {
      }

      await checkUserApplication();
      setShowMessageModal(false);
      setMessageContent('');
      navigate(`/messages?applicationId=${applicationId}`);
    } catch (error: any) {
      console.error('Başvuru oluşturulamadı:', error);
      const errorMessage = error.response?.data?.message || 'Başvuru oluşturulurken bir hata oluştu.';
      if (errorMessage.includes('eligibility')) {
        alert('Başvuru yapmak için önce Uygunluk Formunu doldurmalısınız.');
      } else {
        alert(errorMessage);
      }
    } finally {
      setIsSendingMessage(false);
    }
  };

  const handleSubmitComplaint = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !listing || !id) return;

    try {
      setIsSubmittingComplaint(true);
      await api.post('/complaints', {
        targetListingId: id,
        reason: complaintData.reason,
        description: complaintData.description
      });
      alert('Şikayetiniz başarıyla gönderildi. İncelenmesi gerekiyor.');
      setShowComplaintModal(false);
      setComplaintData({ reason: '', description: '' });
    } catch (error: any) {
      console.error('Şikayet gönderilemedi:', error);
      alert(error.response?.data?.message || 'Şikayet gönderilirken bir hata oluştu.');
    } finally {
      setIsSubmittingComplaint(false);
    }
  };

  const getListingTypeLabel = (type: number) => {
    const types = { 1: 'Sahiplenme', 2: 'Kayıp', 3: 'Yardım İsteği', 4: 'Arıyorum' };
    return types[type as keyof typeof types] || '';
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center" style={{ backgroundColor: '#fffcf1' }}>
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600"></div>
      </div>
    );
  }

  if (!listing) {
    return (
      <div className="min-h-screen flex items-center justify-center" style={{ backgroundColor: '#fffcf1' }}>
        <div className="text-center">
          <h2 className="text-2xl font-bold mb-4 font-poppins">İlan bulunamadı</h2>
          <Button onClick={() => navigate('/listings')}>İlanlara Dön</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pt-20 pb-12" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-6 max-w-7xl">
        {/* Back Button */}
        <button
          onClick={() => navigate('/listings')}
          className="flex items-center gap-2 text-gray-600 hover:text-pink-600 mb-6 font-poppins transition-colors"
        >
          <ArrowLeft className="w-5 h-5" />
          <span>İlanlara Dön</span>
        </button>

        <div className="grid lg:grid-cols-3 gap-8">
          {/* Left Column - Images */}
          <div className="lg:col-span-2">
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden mb-6">
              <div className="relative aspect-square">
                <img
                  src={listing.photoUrls[selectedImage] || 'https://via.placeholder.com/800x800?text=No+Image'}
                  alt={listing.title}
                  className="w-full h-full object-cover"
                />
              </div>
              {listing.photoUrls.length > 1 && (
                <div className="p-4 grid grid-cols-5 gap-3 bg-gray-50">
                  {listing.photoUrls.map((url, index) => (
                    <button
                      key={index}
                      onClick={() => setSelectedImage(index)}
                      className={`relative aspect-square rounded-lg overflow-hidden border-2 transition-all ${
                        selectedImage === index 
                          ? 'border-pink-600 ring-2 ring-pink-200' 
                          : 'border-gray-200 hover:border-gray-300'
                      }`}
                    >
                      <img
                        src={url}
                        alt={`${listing.title} ${index + 1}`}
                        className="w-full h-full object-cover"
                      />
                    </button>
                  ))}
                </div>
              )}
            </div>

            {/* Description */}
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 p-8">
              <h2 className="text-2xl font-poppins font-bold mb-4 text-gray-900">Açıklama</h2>
              <p className="text-gray-700 whitespace-pre-line font-poppins leading-relaxed text-lg">
                {listing.description}
              </p>
            </div>
          </div>

          {/* Right Column - Info */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-2xl shadow-sm border border-gray-200 p-6 sticky top-24">
              {/* Type Badge */}
              <div className="mb-4">
                <span className="inline-block px-4 py-2 bg-gradient-to-r from-pink-500 to-purple-500 text-white text-sm rounded-full font-poppins font-semibold">
                  {getListingTypeLabel(listing.type)}
                </span>
              </div>

              {/* Title */}
              <h1 className="text-3xl font-poppins font-bold mb-4 text-gray-900 leading-tight">
                {listing.title}
              </h1>
              
              {/* Location & Date */}
              <div className="flex flex-col gap-3 text-gray-600 mb-6 font-poppins">
                <div className="flex items-center gap-2">
                  <MapPin className="w-5 h-5 text-gray-400" />
                  <span className="text-base">{listing.city}{listing.district && `, ${listing.district}`}</span>
                </div>
                <div className="flex items-center gap-2">
                  <Calendar className="w-5 h-5 text-gray-400" />
                  <span className="text-base">
                    {new Date(listing.createdAt).toLocaleDateString('tr-TR', { 
                      timeZone: 'Europe/Istanbul',
                      day: 'numeric',
                      month: 'long',
                      year: 'numeric'
                    })}
                  </span>
                </div>
              </div>

              {/* Donation Progress */}
              {listing.type === 3 && listing.requiredAmount && (
                <div className="bg-gradient-to-br from-yellow-50 to-orange-50 border border-yellow-200 rounded-xl p-5 mb-6">
                  <div className="text-sm text-gray-600 mb-2 font-poppins">Hedef Tutar</div>
                  <div className="text-3xl font-bold text-gray-900 mb-3 font-poppins">
                    ₺{listing.collectedAmount?.toLocaleString('tr-TR') || 0} / ₺{listing.requiredAmount.toLocaleString('tr-TR')}
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-3 overflow-hidden">
                    <div
                      className="bg-gradient-to-r from-yellow-500 to-orange-500 h-3 rounded-full transition-all"
                      style={{ width: `${Math.min(((listing.collectedAmount || 0) / listing.requiredAmount) * 100, 100)}%` }}
                    ></div>
                  </div>
                </div>
              )}

              {/* Pet Details Grid */}
              <div className="grid grid-cols-2 gap-4 mb-6">
                <div className="bg-gray-50 rounded-xl p-4">
                  <div className="text-xs text-gray-500 uppercase tracking-wide font-poppins mb-1">Tür</div>
                  <div className="font-poppins font-semibold text-gray-900 text-lg">{listing.species}</div>
                </div>
                <div className="bg-gray-50 rounded-xl p-4">
                  <div className="text-xs text-gray-500 uppercase tracking-wide font-poppins mb-1">Irk</div>
                  <div className="font-poppins font-semibold text-gray-900 text-lg">{listing.breed}</div>
                </div>
                <div className="bg-gray-50 rounded-xl p-4">
                  <div className="text-xs text-gray-500 uppercase tracking-wide font-poppins mb-1">Yaş</div>
                  <div className="font-poppins font-semibold text-gray-900 text-lg">{listing.age} ay</div>
                </div>
                <div className="bg-gray-50 rounded-xl p-4">
                  <div className="text-xs text-gray-500 uppercase tracking-wide font-poppins mb-1">Cinsiyet</div>
                  <div className="font-poppins font-semibold text-gray-900 text-lg">{listing.gender}</div>
                </div>
                <div className="bg-gray-50 rounded-xl p-4">
                  <div className="text-xs text-gray-500 uppercase tracking-wide font-poppins mb-1">Boyut</div>
                  <div className="font-poppins font-semibold text-gray-900 text-lg">{listing.size}</div>
                </div>
                <div className="bg-gray-50 rounded-xl p-4">
                  <div className="text-xs text-gray-500 uppercase tracking-wide font-poppins mb-1">Renk</div>
                  <div className="font-poppins font-semibold text-gray-900 text-lg">{listing.color}</div>
                </div>
              </div>

              {/* Health Badges */}
              <div className="flex flex-wrap gap-2 mb-6">
                {listing.isVaccinated && (
                  <span className="px-4 py-2 bg-green-50 text-green-700 rounded-lg text-sm font-poppins font-medium border border-green-200">
                    ✓ Aşılı
                  </span>
                )}
                {listing.isNeutered && (
                  <span className="px-4 py-2 bg-blue-50 text-blue-700 rounded-lg text-sm font-poppins font-medium border border-blue-200">
                    ✓ Kısırlaştırılmış
                  </span>
                )}
                {listing.isShelter && (
                  <span className="px-4 py-2 bg-purple-50 text-purple-700 rounded-lg text-sm font-poppins font-medium border border-purple-200">
                    🏠 Barınak
                  </span>
                )}
              </div>

              {/* Action Buttons */}
              <div className="space-y-3 mb-6">
                {user?.id !== listing.ownerId && (
                  <>
                    {userApplication && listing.type !== 3 ? (
                      <button
                        onClick={handleMessage}
                        className="w-full px-6 py-4 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl font-poppins font-semibold hover:from-blue-700 hover:to-blue-800 transition-all shadow-lg hover:shadow-xl flex items-center justify-center gap-2 text-lg"
                      >
                        <MessageCircle className="w-6 h-6" />
                        Mesajlaş
                      </button>
                    ) : listing.type !== 3 ? (
                      <>
                        <button
                          onClick={handleMessage}
                          className="w-full px-6 py-4 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl font-poppins font-semibold hover:from-blue-700 hover:to-blue-800 transition-all shadow-lg hover:shadow-xl flex items-center justify-center gap-2 text-lg"
                        >
                          <MessageCircle className="w-6 h-6" />
                          Soru Sor
                        </button>
                        <button
                          onClick={handleApply}
                          className="w-full px-6 py-4 bg-gradient-to-r from-pink-600 to-purple-600 text-white rounded-xl font-poppins font-semibold hover:from-pink-700 hover:to-purple-700 transition-all shadow-lg hover:shadow-xl text-lg"
                        >
                          Başvuru Yap
                        </button>
                      </>
                    ) : (
                      <button
                        onClick={handleApply}
                        className="w-full px-6 py-4 bg-gradient-to-r from-pink-600 to-purple-600 text-white rounded-xl font-poppins font-semibold hover:from-pink-700 hover:to-purple-700 transition-all shadow-lg hover:shadow-xl text-lg"
                      >
                        Bağış Yap
                      </button>
                    )}
                  </>
                )}
                
                <div className="flex gap-3">
                  {user && (
                    <button
                      onClick={handleFavorite}
                      className={`flex-1 px-4 py-3 border-2 rounded-xl transition-all font-poppins font-medium ${
                        isFavorite 
                          ? 'border-pink-500 bg-pink-50 text-pink-600 shadow-md' 
                          : 'border-gray-300 text-gray-600 hover:border-gray-400 hover:bg-gray-50'
                      }`}
                    >
                      <Heart className={`w-5 h-5 mx-auto ${isFavorite ? 'fill-current' : ''}`} />
                    </button>
                  )}
                  <button className="flex-1 px-4 py-3 border-2 border-gray-300 rounded-xl text-gray-600 hover:border-gray-400 hover:bg-gray-50 transition-all">
                    <Share2 className="w-5 h-5 mx-auto" />
                  </button>
                </div>
              </div>

              {/* Owner Info */}
              <div className="border-t border-gray-200 pt-6">
                <div className="text-xs text-gray-500 uppercase tracking-wide font-poppins mb-3">İlan Sahibi</div>
                <div className="font-poppins font-bold text-gray-900 text-xl mb-2">{listing.ownerName}</div>
                <div className="text-sm text-gray-500 font-poppins space-y-1">
                  <div className="flex items-center gap-2">
                    <span className="font-medium">📋 {listing.applicationCount} başvuru</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="font-medium">❤️ {listing.favoriteCount} favori</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Message Modal */}
      {showMessageModal && (
        <>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
            onClick={() => setShowMessageModal(false)}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-md p-6"
              onClick={(e) => e.stopPropagation()}
            >
              <h3 className="text-xl font-poppins font-bold mb-4 text-gray-900">
                {listing.ownerName}'e Soru Sor
              </h3>
              <p className="text-sm text-gray-600 mb-4 font-poppins">
                Bu ilan hakkında sorularınızı buradan iletebilirsiniz. İlan sahibi size cevap verdikten sonra başvuru yapabilirsiniz.
              </p>
              <textarea
                value={messageContent}
                onChange={(e) => setMessageContent(e.target.value)}
                placeholder="Mesajınızı buraya yazın..."
                className="w-full h-32 px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins resize-none"
              />
              <div className="flex gap-3 mt-6">
                <button
                  onClick={() => {
                    setShowMessageModal(false);
                    setMessageContent('');
                  }}
                  className="flex-1 px-4 py-3 border border-gray-300 rounded-xl font-poppins font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                >
                  İptal
                </button>
                <button
                  onClick={sendInquiryMessage}
                  disabled={!messageContent.trim() || isSendingMessage}
                  className="flex-1 px-4 py-3 bg-gradient-to-r from-pink-600 to-purple-600 text-white rounded-xl font-poppins font-semibold hover:from-pink-700 hover:to-purple-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isSendingMessage ? 'Gönderiliyor...' : 'Gönder'}
                </button>
              </div>
            </div>
          </div>
        </>
      )}

      {/* Complaint Modal */}
      {showComplaintModal && (
        <>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
            onClick={() => {
              setShowComplaintModal(false);
              setComplaintData({ reason: '', description: '' });
            }}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-md p-6"
              onClick={(e) => e.stopPropagation()}
            >
              <h3 className="text-xl font-poppins font-bold mb-4 text-gray-900">
                Şikayet Et
              </h3>
              <p className="text-sm text-gray-600 mb-4 font-poppins">
                Bu ilanla ilgili şikayetinizi bildirin. İnceleme sonrası gerekli işlemler yapılacaktır.
              </p>
              
              <form onSubmit={handleSubmitComplaint} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Şikayet Sebebi *
                  </label>
                  <input
                    type="text"
                    value={complaintData.reason}
                    onChange={(e) => setComplaintData({ ...complaintData, reason: e.target.value })}
                    className="w-full px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-red-500 transition-colors font-poppins"
                    placeholder="Örn: Yanıltıcı bilgi, uygunsuz içerik..."
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Açıklama *
                  </label>
                  <textarea
                    value={complaintData.description}
                    onChange={(e) => setComplaintData({ ...complaintData, description: e.target.value })}
                    className="w-full h-32 px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-red-500 transition-colors font-poppins resize-none"
                    placeholder="Şikayetinizi detaylı olarak açıklayın..."
                    required
                  />
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    type="button"
                    onClick={() => {
                      setShowComplaintModal(false);
                      setComplaintData({ reason: '', description: '' });
                    }}
                    className="flex-1 px-4 py-3 border border-gray-300 rounded-xl font-poppins font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                  >
                    İptal
                  </button>
                  <button
                    type="submit"
                    disabled={isSubmittingComplaint}
                    className="flex-1 px-4 py-3 bg-red-600 text-white rounded-xl font-poppins font-semibold hover:bg-red-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {isSubmittingComplaint ? 'Gönderiliyor...' : 'Şikayet Et'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
