import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, BookOpen, Calendar, User, Image as ImageIcon, Eye } from 'lucide-react';
import api from '../lib/api';
import { useAuth } from '../contexts/AuthContext';

interface Story {
  id: string;
  authorId: string;
  authorName: string;
  title: string;
  content: string;
  photoUrl?: string;
  status: string;
  createdAt: string;
}

export default function Stories() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [stories, setStories] = useState<Story[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [selectedStory, setSelectedStory] = useState<Story | null>(null);
  const [showStoryDetail, setShowStoryDetail] = useState(false);

  const [formData, setFormData] = useState({
    title: '',
    content: '',
    photoUrl: '',
    applicationId: '' as string | null
  });
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    fetchStories();
  }, []);

  const fetchStories = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/stories');
      setStories(response.data || []);
    } catch (error) {
      console.error('Hikayeler yüklenemedi:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateStory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) {
      navigate('/login');
      return;
    }

    try {
      setIsSubmitting(true);
      const payload: any = {
        title: formData.title,
        content: formData.content
      };
      
      if (formData.photoUrl) {
        payload.photoUrl = formData.photoUrl;
      }
      
      if (formData.applicationId) {
        payload.applicationId = formData.applicationId;
      }

      await api.post('/stories', payload);
      alert('Hikaye başarıyla oluşturuldu. Admin onayından sonra yayınlanacaktır.');
      setShowCreateModal(false);
      setFormData({ title: '', content: '', photoUrl: '', applicationId: null });
      fetchStories();
    } catch (error: any) {
      console.error('Hikaye oluşturulamadı:', error);
      alert(error.response?.data?.message || 'Hikaye oluşturulurken bir hata oluştu.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      alert('Lütfen bir resim dosyası seçin.');
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      alert('Resim boyutu 5MB\'dan küçük olmalıdır.');
      return;
    }

    try {
      const formData = new FormData();
      formData.append('file', file);
      
      const response = await api.post('/upload/listing-photo', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });
      
      setFormData(prev => ({ ...prev, photoUrl: response.data.url }));
    } catch (error: any) {
      console.error('Resim yüklenemedi:', error);
      alert('Resim yüklenirken bir hata oluştu.');
    }
  };

  const handleViewStory = (story: Story) => {
    setSelectedStory(story);
    setShowStoryDetail(true);
  };

  if (isLoading) {
    return (
      <div className="min-h-screen pt-24 pb-12 flex items-center justify-center" style={{ backgroundColor: '#fffcf1' }}>
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600"></div>
      </div>
    );
  }

  return (
    <div className="min-h-screen pt-24 pb-12" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-6 max-w-7xl">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h1 className="text-4xl font-poppins font-bold text-gray-900 mb-2">Mutlu Hikayeler</h1>
              <p className="text-gray-600 font-poppins">Sahiplenilen hayvanların mutlu yaşam hikayeleri</p>
            </div>
            {user && (
              <button
                onClick={() => setShowCreateModal(true)}
                className="flex items-center gap-2 px-6 py-3 bg-gradient-to-r from-pink-600 to-purple-600 text-white rounded-xl font-poppins font-semibold hover:from-pink-700 hover:to-purple-700 transition-all shadow-lg hover:shadow-xl"
              >
                <Plus className="w-5 h-5" />
                Hikaye Yaz
              </button>
            )}
          </div>
        </div>

        {/* Stories Grid */}
        {stories.length === 0 ? (
          <div className="text-center py-16 bg-white rounded-2xl shadow-sm border border-gray-200">
            <BookOpen className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500 font-poppins text-lg">Henüz hikaye yok</p>
            {user && (
              <button
                onClick={() => setShowCreateModal(true)}
                className="mt-4 px-6 py-2 bg-pink-600 text-white rounded-lg font-poppins font-medium hover:bg-pink-700 transition-colors"
              >
                İlk Hikayeyi Sen Yaz
              </button>
            )}
          </div>
        ) : (
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {stories.map((story) => (
              <div
                key={story.id}
                className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden hover:shadow-lg transition-all cursor-pointer"
                onClick={() => handleViewStory(story)}
              >
                {story.photoUrl && (
                  <div className="relative h-48 overflow-hidden">
                    <img
                      src={story.photoUrl}
                      alt={story.title}
                      className="w-full h-full object-cover"
                    />
                  </div>
                )}
                <div className="p-6">
                  <h3 className="text-xl font-poppins font-bold text-gray-900 mb-3 line-clamp-2">
                    {story.title}
                  </h3>
                  <p className="text-gray-600 font-poppins text-sm line-clamp-3 mb-4">
                    {story.content}
                  </p>
                  <div className="flex items-center justify-between text-xs text-gray-500 font-poppins pt-4 border-t border-gray-100">
                    <div className="flex items-center gap-2">
                      <User className="w-4 h-4" />
                      <span>{story.authorName}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <Calendar className="w-4 h-4" />
                      <span>
                        {new Date(story.createdAt).toLocaleDateString('tr-TR', {
                          day: 'numeric',
                          month: 'short',
                          year: 'numeric',
                          timeZone: 'Europe/Istanbul'
                        })}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Create Story Modal */}
      {showCreateModal && (
        <>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
            onClick={() => {
              setShowCreateModal(false);
              setFormData({ title: '', content: '', photoUrl: '', applicationId: null });
            }}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl p-6 max-h-[90vh] overflow-y-auto"
              onClick={(e) => e.stopPropagation()}
            >
              <h2 className="text-2xl font-poppins font-bold mb-6 text-gray-900">Yeni Hikaye Yaz</h2>
              
              <form onSubmit={handleCreateStory} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Başlık *
                  </label>
                  <input
                    type="text"
                    value={formData.title}
                    onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins"
                    placeholder="Hikayenizin başlığı"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    İçerik *
                  </label>
                  <textarea
                    value={formData.content}
                    onChange={(e) => setFormData({ ...formData, content: e.target.value })}
                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins resize-none"
                    placeholder="Hikayenizi yazın..."
                    rows={8}
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Fotoğraf (İsteğe bağlı)
                  </label>
                  {formData.photoUrl ? (
                    <div className="relative">
                      <img
                        src={formData.photoUrl}
                        alt="Preview"
                        className="w-full h-48 object-cover rounded-lg mb-2"
                      />
                      <button
                        type="button"
                        onClick={() => setFormData({ ...formData, photoUrl: '' })}
                        className="absolute top-2 right-2 p-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
                      >
                        Kaldır
                      </button>
                    </div>
                  ) : (
                    <label className="block w-full px-4 py-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-pink-400 hover:bg-pink-50/50 transition-colors cursor-pointer text-center font-poppins">
                      <input
                        type="file"
                        accept="image/*"
                        onChange={handleImageUpload}
                        className="hidden"
                      />
                      <ImageIcon className="w-5 h-5 mx-auto mb-2 text-gray-400" />
                      <span className="text-sm text-gray-700 font-medium block">
                        Fotoğraf yüklemek için tıklayın
                      </span>
                      <p className="text-xs text-gray-500 mt-1">PNG, JPG veya GIF (Max. 5MB)</p>
                    </label>
                  )}
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    type="button"
                    onClick={() => {
                      setShowCreateModal(false);
                      setFormData({ title: '', content: '', photoUrl: '', applicationId: null });
                    }}
                    className="flex-1 px-4 py-2.5 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-poppins font-medium transition-colors"
                  >
                    İptal
                  </button>
                  <button
                    type="submit"
                    disabled={isSubmitting}
                    className="flex-1 px-4 py-2.5 bg-gradient-to-r from-pink-600 to-purple-600 text-white rounded-lg hover:from-pink-700 hover:to-purple-700 disabled:opacity-50 disabled:cursor-not-allowed font-poppins font-semibold transition-all"
                  >
                    {isSubmitting ? 'Gönderiliyor...' : 'Gönder'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </>
      )}

      {/* Story Detail Modal */}
      {showStoryDetail && selectedStory && (
        <>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
            onClick={() => {
              setShowStoryDetail(false);
              setSelectedStory(null);
            }}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-3xl max-h-[90vh] overflow-y-auto"
              onClick={(e) => e.stopPropagation()}
            >
              {selectedStory.photoUrl && (
                <div className="relative h-64 overflow-hidden">
                  <img
                    src={selectedStory.photoUrl}
                    alt={selectedStory.title}
                    className="w-full h-full object-cover"
                  />
                </div>
              )}
              
              <div className="p-8">
                <h2 className="text-3xl font-poppins font-bold text-gray-900 mb-4">
                  {selectedStory.title}
                </h2>
                
                <div className="flex items-center gap-4 text-sm text-gray-600 font-poppins mb-6">
                  <div className="flex items-center gap-2">
                    <User className="w-4 h-4" />
                    <span>{selectedStory.authorName}</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4" />
                    <span>
                      {new Date(selectedStory.createdAt).toLocaleDateString('tr-TR', {
                        day: 'numeric',
                        month: 'long',
                        year: 'numeric',
                        timeZone: 'Europe/Istanbul'
                      })}
                    </span>
                  </div>
                </div>

                <div className="prose max-w-none">
                  <p className="text-gray-700 font-poppins leading-relaxed whitespace-pre-wrap">
                    {selectedStory.content}
                  </p>
                </div>

                <button
                  onClick={() => {
                    setShowStoryDetail(false);
                    setSelectedStory(null);
                  }}
                  className="mt-6 px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 font-poppins font-medium transition-colors"
                >
                  Kapat
                </button>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

