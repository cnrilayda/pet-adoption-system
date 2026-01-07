import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { 
  User, Heart, FileText, MessageCircle, Plus, ClipboardCheck, X, Trash2, 
  Edit2, DollarSign, Pause, Play, 
  MapPin, Phone, Mail, CheckCircle2, Lock, Upload, Users, Check, XCircle, Flag, Star
} from 'lucide-react';
import api from '../lib/api';
import { useAuth } from '../contexts/AuthContext';
import Button from '../components/ui/Button';

export default function Profile() {
  const { user, updateUser } = useAuth();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState('profile');
  const [myListings, setMyListings] = useState([]);
  const [myApplications, setMyApplications] = useState([]);
  const [myFavorites, setMyFavorites] = useState([]);
  const [myDonations, setMyDonations] = useState([]);
  const [myComplaints, setMyComplaints] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [hasEligibilityForm, setHasEligibilityForm] = useState(false);
  const [selectedApplication, setSelectedApplication] = useState<any>(null);
  const [showApplicationDetail, setShowApplicationDetail] = useState(false);
  const [applicationListing, setApplicationListing] = useState<any>(null);
  const [loadingListing, setLoadingListing] = useState(false);
  const [selectedListingId, setSelectedListingId] = useState<string | null>(null);
  const [listingApplications, setListingApplications] = useState<any[]>([]);
  const [showListingApplications, setShowListingApplications] = useState(false);
  const [loadingApplications, setLoadingApplications] = useState(false);
  const [updatingStatus, setUpdatingStatus] = useState<string | null>(null);
  const [showRatingModal, setShowRatingModal] = useState(false);
  const [selectedApplicationForRating, setSelectedApplicationForRating] = useState<any>(null);
  const [ratingData, setRatingData] = useState({
    score: 5,
    comment: ''
  });
  const [isSubmittingRating, setIsSubmittingRating] = useState(false);
  const [existingRating, setExistingRating] = useState<any>(null);
  const [showEditProfile, setShowEditProfile] = useState(false);
  const [profileData, setProfileData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    city: '',
    address: '',
    profilePictureUrl: ''
  });
  const [profileImageFile, setProfileImageFile] = useState<File | null>(null);
  const [profileImagePreview, setProfileImagePreview] = useState<string | null>(null);
  const [uploadingProfile, setUploadingProfile] = useState(false);
  const [showChangePassword, setShowChangePassword] = useState(false);
  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });

  useEffect(() => {
    checkEligibilityForm();
    if (activeTab === 'listings') {
      fetchMyListings();
    } else if (activeTab === 'applications') {
      fetchMyApplications();
    } else if (activeTab === 'favorites') {
      fetchMyFavorites();
    } else if (activeTab === 'donations') {
      fetchMyDonations();
    } else if (activeTab === 'complaints') {
      fetchMyComplaints();
    }
  }, [activeTab]);

  const checkEligibilityForm = async () => {
    try {
      await api.get('/eligibilityforms/my-form');
      setHasEligibilityForm(true);
    } catch (error: any) {
      if (error.response?.status === 404) {
        setHasEligibilityForm(false);
      }
    }
  };

  const fetchMyListings = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/petlistings/my-listings');
      setMyListings(response.data);
    } catch (error) {
    } finally {
      setIsLoading(false);
    }
  };

  const fetchMyApplications = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/applications/my-applications');
      setMyApplications(response.data);
    } catch (error) {
    } finally {
      setIsLoading(false);
    }
  };

  const fetchMyFavorites = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/favorites');
      setMyFavorites(response.data || []);
    } catch (error) {
      setMyFavorites([]);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchMyDonations = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/donations/my-donations');
      setMyDonations(response.data || []);
    } catch (error) {
      setMyDonations([]);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchMyComplaints = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/complaints/my-complaints');
      setMyComplaints(response.data || []);
    } catch (error) {
      setMyComplaints([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleViewApplication = async (application: any) => {
    setSelectedApplication(application);
    setShowApplicationDetail(true);
    setLoadingListing(true);
    
    try {
      const response = await api.get(`/petlistings/${application.listingId}`);
      setApplicationListing(response.data);
    } catch (error) {
    } finally {
      setLoadingListing(false);
    }
  };

  const handleCancelApplication = async (applicationId: string) => {
    if (!confirm('Bu başvuruyu iptal etmek istediğinize emin misiniz?')) {
      return;
    }

    try {
      await api.post(`/applications/${applicationId}/cancel`);
      alert('Başvuru başarıyla iptal edildi.');
      setShowApplicationDetail(false);
      fetchMyApplications();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Başvuru iptal edilirken bir hata oluştu.');
    }
  };

  const handleDeleteListing = async (listingId: string, e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    if (!confirm('Bu ilanı silmek istediğinize emin misiniz? Bu işlem geri alınamaz.')) {
      return;
    }

    try {
      await api.delete(`/petlistings/${listingId}`);
      alert('İlan başarıyla silindi.');
      fetchMyListings();
    } catch (error: any) {
      alert(error.response?.data?.message || 'İlan silinirken bir hata oluştu.');
    }
  };

  const handleToggleListing = async (listingId: string, currentStatus: boolean, e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    
    try {
      await api.post(`/petlistings/${listingId}/toggle-pause`);
      alert(`İlan ${currentStatus ? 'duraklatıldı' : 'aktif hale getirildi'}.`);
      fetchMyListings();
    } catch (error: any) {
      alert(error.response?.data?.message || 'İlan durumu değiştirilirken bir hata oluştu.');
    }
  };

  const fetchListingApplications = async (listingId: string) => {
    try {
      setLoadingApplications(true);
      const response = await api.get(`/applications/listing/${listingId}`);
      setListingApplications(response.data || []);
    } catch (error: any) {
      alert(error.response?.data?.message || 'Başvurular yüklenirken bir hata oluştu.');
    } finally {
      setLoadingApplications(false);
    }
  };

  const handleViewListingApplications = async (listingId: string, e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setSelectedListingId(listingId);
    setShowListingApplications(true);
    await fetchListingApplications(listingId);
  };

  const handleUpdateApplicationStatus = async (applicationId: string, status: 'Accepted' | 'Rejected', notes?: string) => {
    if (!confirm(`Bu başvuruyu ${status === 'Accepted' ? 'kabul' : 'reddet'}mek istediğinize emin misiniz?`)) {
      return;
    }

    try {
      setUpdatingStatus(applicationId);
      await api.put(`/applications/${applicationId}/status`, {
        status: status,
        adminNotes: notes || null
      });
      alert(`Başvuru başarıyla ${status === 'Accepted' ? 'kabul edildi' : 'reddedildi'}.`);
      if (selectedListingId) {
        await fetchListingApplications(selectedListingId);
      }
      fetchMyListings();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Başvuru durumu güncellenirken bir hata oluştu.');
    } finally {
      setUpdatingStatus(null);
    }
  };

  const handleOpenRatingModal = async (application: any) => {
    setSelectedApplicationForRating(application);
    setShowRatingModal(true);
    setRatingData({ score: 5, comment: '' });
    
    try {
      const response = await api.get(`/ratings/application/${application.id}`);
      setExistingRating(response.data);
      if (response.data) {
        setRatingData({ score: response.data.score, comment: response.data.comment || '' });
      }
    } catch (error: any) {
      if (error.response?.status === 404) {
        setExistingRating(null);
      }
    }
  };

  const handleSubmitRating = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedApplicationForRating) return;

    try {
      setIsSubmittingRating(true);
      await api.post('/ratings', {
        applicationId: selectedApplicationForRating.id,
        score: ratingData.score,
        comment: ratingData.comment || null
      });
      alert('Değerlendirmeniz başarıyla kaydedildi.');
      setShowRatingModal(false);
      setSelectedApplicationForRating(null);
      fetchMyApplications();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Değerlendirme kaydedilirken bir hata oluştu.');
    } finally {
      setIsSubmittingRating(false);
    }
  };

  const handleEditProfile = () => {
    setProfileData({
      firstName: user?.firstName || '',
      lastName: user?.lastName || '',
      email: user?.email || '',
      phoneNumber: user?.phoneNumber || '',
      city: user?.city || '',
      address: user?.address || '',
      profilePictureUrl: user?.profilePictureUrl || ''
    });
    setProfileImageFile(null);
    setProfileImagePreview(null);
    setShowEditProfile(true);
  };

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      if (!file.type.startsWith('image/')) {
        alert('Lütfen bir resim dosyası seçin.');
        return;
      }
      
      if (file.size > 5 * 1024 * 1024) {
        alert('Resim boyutu 5MB\'dan küçük olmalıdır.');
        return;
      }
      
      setProfileImageFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setProfileImagePreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    
    try {
      setUploadingProfile(true);
      
      let profilePictureUrl = profileData.profilePictureUrl;
      
      if (profileImageFile) {
        try {
          const formData = new FormData();
          formData.append('file', profileImageFile);
          
          const uploadResponse = await api.post('/upload/profile-picture', formData, {
            headers: {
              'Content-Type': 'multipart/form-data'
            }
          });
          profilePictureUrl = uploadResponse.data.url || uploadResponse.data.profilePictureUrl;
        } catch (uploadError: any) {
          alert(uploadError.response?.data?.message || 'Fotoğraf yüklenirken bir hata oluştu. Lütfen tekrar deneyin.');
          setUploadingProfile(false);
          return;
        }
      }
      
      await api.put('/auth/profile', {
        firstName: profileData.firstName,
        lastName: profileData.lastName,
        phoneNumber: profileData.phoneNumber,
        city: profileData.city,
        address: profileData.address,
        profilePictureUrl: profilePictureUrl
      });
      
      if (updateUser) {
        updateUser({
          firstName: profileData.firstName,
          lastName: profileData.lastName,
          phoneNumber: profileData.phoneNumber,
          city: profileData.city,
          address: profileData.address,
          profilePictureUrl: profilePictureUrl
        });
      }
      
      const updatedUser = {
        ...user,
        firstName: profileData.firstName,
        lastName: profileData.lastName,
        phoneNumber: profileData.phoneNumber,
        city: profileData.city,
        address: profileData.address,
        profilePictureUrl: profilePictureUrl
      };
      localStorage.setItem('user', JSON.stringify(updatedUser));
      
      alert('Profil başarıyla güncellendi.');
      setShowEditProfile(false);
      setProfileImageFile(null);
      setProfileImagePreview(null);
    } catch (error: any) {
      alert(error.response?.data?.message || 'Profil güncellenirken bir hata oluştu.');
    } finally {
      setUploadingProfile(false);
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (passwordData.newPassword !== passwordData.confirmPassword) {
      alert('Yeni şifreler eşleşmiyor.');
      return;
    }

    if (passwordData.newPassword.length < 6) {
      alert('Şifre en az 6 karakter olmalıdır.');
      return;
    }

    try {
      await api.post('/auth/change-password', {
        currentPassword: passwordData.currentPassword,
        newPassword: passwordData.newPassword
      });
      
      alert('Şifre başarıyla değiştirildi.');
      setShowChangePassword(false);
      setPasswordData({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Şifre değiştirilirken bir hata oluştu. Mevcut şifrenizi kontrol edin.';
      alert(errorMessage);
      setPasswordData({
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      });
    }
  };


  return (
    <div className="min-h-screen pt-24 pb-12" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4 md:px-8 max-w-7xl">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
          {/* Sidebar Navigation */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-xl shadow-sm border border-gray-200 sticky top-24">
              <div className="p-6 border-b border-gray-200">
                <h2 className="text-lg font-semibold text-gray-900 font-poppins">Hesap Ayarları</h2>
              </div>
              <nav className="p-4">
                <button
                  onClick={() => setActiveTab('profile')}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-colors font-poppins mb-1 ${
                    activeTab === 'profile'
                      ? 'bg-blue-50 text-blue-700 font-medium'
                      : 'text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  <User className="w-5 h-5" />
                  <span>Profil Bilgileri</span>
                </button>
                <button
                  onClick={() => setActiveTab('listings')}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-colors font-poppins mb-1 ${
                    activeTab === 'listings'
                      ? 'bg-blue-50 text-blue-700 font-medium'
                      : 'text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  <FileText className="w-5 h-5" />
                  <span>İlanlarım</span>
                </button>
                <button
                  onClick={() => setActiveTab('applications')}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-colors font-poppins mb-1 ${
                    activeTab === 'applications'
                      ? 'bg-blue-50 text-blue-700 font-medium'
                      : 'text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  <MessageCircle className="w-5 h-5" />
                  <span>Başvurularım</span>
                </button>
                <button
                  onClick={() => setActiveTab('favorites')}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-colors font-poppins mb-1 ${
                    activeTab === 'favorites'
                      ? 'bg-blue-50 text-blue-700 font-medium'
                      : 'text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  <Heart className="w-5 h-5" />
                  <span>Favorilerim</span>
                </button>
                <button
                  onClick={() => setActiveTab('donations')}
                  className={`w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-colors font-poppins mb-1 ${
                    activeTab === 'donations'
                      ? 'bg-blue-50 text-blue-700 font-medium'
                      : 'text-gray-700 hover:bg-gray-50'
                  }`}
                >
                  <DollarSign className="w-5 h-5" />
                  <span>Bağışlarım</span>
                </button>
                <Link to="/eligibility-form">
                  <button
                    className="w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-colors font-poppins mb-1 text-gray-700 hover:bg-gray-50"
                  >
                    <ClipboardCheck className="w-5 h-5" />
                    <span>Uygunluk Formu</span>
                  </button>
                </Link>
                <div className="border-t border-gray-200 mt-4 pt-4">
                  <button
                    onClick={() => setShowChangePassword(true)}
                    className="w-full flex items-center gap-3 px-4 py-3 rounded-lg text-left transition-colors font-poppins text-gray-700 hover:bg-gray-50"
                  >
                    <Lock className="w-5 h-5" />
                    <span>Şifre Değiştir</span>
                  </button>
                </div>
              </nav>
            </div>
          </div>

          {/* Main Content Area */}
          <div className="lg:col-span-3">
            {activeTab === 'profile' && (
              <div className="space-y-6">
                {/* Profile Information Card - Combined */}
                <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
                  <div className="flex items-start justify-between mb-6">
                    <h3 className="text-xl font-semibold text-gray-900 font-poppins">Profil Bilgileri</h3>
                    <button
                      onClick={handleEditProfile}
                      className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 rounded-lg transition-colors"
                    >
                      <Edit2 className="w-4 h-4" />
                      <span>Düzenle</span>
                    </button>
                  </div>
                  
                  {/* Profile Header Section */}
                  <div className="flex items-start gap-6 pb-6 border-b border-gray-100 mb-6">
                    <div className="relative">
                      {user?.profilePictureUrl ? (
                        <img 
                          src={user.profilePictureUrl} 
                          alt="Profile" 
                          className="w-20 h-20 rounded-full object-cover border-2 border-gray-200"
                        />
                      ) : (
                        <div className="w-20 h-20 bg-gray-100 rounded-full flex items-center justify-center border-2 border-gray-200">
                          <User className="w-10 h-10 text-gray-400" />
                        </div>
                      )}
                    </div>
                    <div className="flex-1">
                      <h4 className="text-2xl font-semibold text-gray-900 font-poppins mb-1">
                        {user?.firstName} {user?.lastName}
                      </h4>
                      <div className="flex items-center gap-3 mt-3">
                        {user?.isShelter && (
                          <span className="px-3 py-1 bg-purple-50 text-purple-700 text-xs rounded-full font-medium font-poppins">
                            Barınak Hesabı
                          </span>
                        )}
                        {user?.isShelterVerified && (
                          <span className="px-3 py-1 bg-green-50 text-green-700 text-xs rounded-full flex items-center gap-1 font-medium font-poppins">
                            <CheckCircle2 className="w-3 h-3" />
                            Doğrulanmış
                          </span>
                        )}
                      </div>
                    </div>
                  </div>

                  {/* Personal Information Section */}
                  <div className="grid md:grid-cols-2 gap-6">
                    <div>
                      <label className="text-xs font-medium text-gray-500 uppercase tracking-wide font-poppins">E-posta</label>
                      <p className="mt-1 text-sm text-gray-900 font-poppins flex items-center gap-2">
                        <Mail className="w-4 h-4 text-gray-400" />
                        {user?.email || '-'}
                      </p>
                    </div>
                    <div>
                      <label className="text-xs font-medium text-gray-500 uppercase tracking-wide font-poppins">Telefon</label>
                      <p className="mt-1 text-sm text-gray-900 font-poppins flex items-center gap-2">
                        <Phone className="w-4 h-4 text-gray-400" />
                        {user?.phoneNumber || '-'}
                      </p>
                    </div>
                    <div className="md:col-span-2">
                      <label className="text-xs font-medium text-gray-500 uppercase tracking-wide font-poppins">Şehir</label>
                      <p className="mt-1 text-sm text-gray-900 font-poppins flex items-center gap-2">
                        <MapPin className="w-4 h-4 text-gray-400" />
                        {user?.city || '-'}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Quick Actions */}
                <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
                  <h3 className="text-xl font-semibold text-gray-900 font-poppins mb-4">Hızlı İşlemler</h3>
                  <div className="grid md:grid-cols-2 gap-4">
                    <Link to="/eligibility-form">
                      <div className="p-4 border border-gray-200 rounded-lg hover:border-blue-300 hover:bg-blue-50 transition-colors cursor-pointer">
                        <div className="flex items-center gap-3">
                          <ClipboardCheck className="w-5 h-5 text-blue-600" />
                          <div>
                            <p className="font-medium text-gray-900 font-poppins text-sm">
                              {hasEligibilityForm ? 'Uygunluk Formunu Görüntüle' : 'Uygunluk Formunu Doldur'}
                            </p>
                          </div>
                        </div>
                      </div>
                    </Link>
                    <Link to="/listings/create">
                      <div className="p-4 border border-gray-200 rounded-lg hover:border-blue-300 hover:bg-blue-50 transition-colors cursor-pointer">
                        <div className="flex items-center gap-3">
                          <Plus className="w-5 h-5 text-blue-600" />
                          <div>
                            <p className="font-medium text-gray-900 font-poppins text-sm">Yeni İlan Oluştur</p>
                          </div>
                        </div>
                      </div>
                    </Link>
                  </div>
                </div>
              </div>
            )}

            {/* Tabs Content */}
            {activeTab !== 'profile' && (
              <div className="bg-white rounded-xl shadow-sm border border-gray-200">
                <div className="p-6 border-b border-gray-200">
                  <h3 className="text-xl font-semibold text-gray-900 font-poppins">
                    {activeTab === 'listings' && 'İlanlarım'}
                    {activeTab === 'applications' && 'Başvurularım'}
                    {activeTab === 'favorites' && 'Favorilerim'}
                    {activeTab === 'donations' && 'Bağışlarım'}
                    {activeTab === 'complaints' && 'Şikayetlerim'}
                  </h3>
                </div>
                <div className="p-6">
                  {isLoading ? (
                    <div className="text-center py-12">
                      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                      <p className="mt-4 text-gray-600 font-poppins">Yükleniyor...</p>
                    </div>
                  ) : (
                    <>
                      {activeTab === 'listings' && (
                        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
                          {myListings.length === 0 ? (
                            <div className="col-span-full text-center py-16">
                              <FileText className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                              <p className="text-gray-500 font-poppins">Henüz ilan vermediniz</p>
                            </div>
                          ) : (
                            myListings.map((listing: any) => (
                              <div key={listing.id} className="relative group">
                                <Link to={`/listings/${listing.id}`}>
                                  <div className="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-all cursor-pointer h-full">
                                    <div className="relative">
                                      <img
                                        src={listing.photoUrls?.[0] || 'https://via.placeholder.com/400x300'}
                                        alt={listing.title}
                                        className="w-full h-48 object-cover"
                                      />
                                      <div className="absolute top-3 left-3 flex gap-2">
                                        {!listing.isActive && (
                                          <span className="px-2.5 py-1 bg-gray-900/70 text-white text-xs rounded-md font-medium font-poppins backdrop-blur-sm">
                                            Pasif
                                          </span>
                                        )}
                                        {!listing.isApproved && (
                                          <span className="px-2.5 py-1 bg-yellow-500/90 text-white text-xs rounded-md font-medium font-poppins backdrop-blur-sm">
                                            Onay Bekliyor
                                          </span>
                                        )}
                                      </div>
                                    </div>
                                    <div className="p-4">
                                      <h3 className="font-poppins font-semibold text-gray-900 mb-2 line-clamp-1">{listing.title}</h3>
                                      <p className="text-sm text-gray-600 line-clamp-2 mb-3 font-poppins">{listing.description}</p>
                                      <div className="flex items-center justify-between text-xs text-gray-500 font-poppins pt-3 border-t border-gray-100">
                                        <span>{listing.applicationCount || 0} başvuru</span>
                                        <span>
                                          {new Date(listing.createdAt).toLocaleDateString('tr-TR', {
                                            day: 'numeric',
                                            month: 'short',
                                            timeZone: 'Europe/Istanbul'
                                          })}
                                        </span>
                                      </div>
                                    </div>
                                  </div>
                                </Link>
                                <div className="absolute top-3 right-3 opacity-0 group-hover:opacity-100 transition-opacity flex gap-2 flex-col">
                                  {listing.type === 1 && listing.applicationCount > 0 && (
                                    <button
                                      onClick={(e) => handleViewListingApplications(listing.id, e)}
                                      className="p-2 bg-blue-500/95 backdrop-blur-sm rounded-lg shadow-lg hover:bg-blue-600 transition-colors text-white"
                                      title="Başvuruları Gör"
                                    >
                                      <Users className="w-4 h-4" />
                                    </button>
                                  )}
                                  <div className="flex gap-2">
                                    <button
                                      onClick={(e) => {
                                        e.preventDefault();
                                        e.stopPropagation();
                                        handleToggleListing(listing.id, listing.isActive, e);
                                      }}
                                      className="p-2 bg-white/95 backdrop-blur-sm rounded-lg shadow-lg hover:bg-white transition-colors"
                                      title={listing.isActive ? 'Duraklat' : 'Aktif Et'}
                                    >
                                      {listing.isActive ? (
                                        <Pause className="w-4 h-4 text-gray-700" />
                                      ) : (
                                        <Play className="w-4 h-4 text-gray-700" />
                                      )}
                                    </button>
                                    <Link
                                      to={`/listings/${listing.id}/edit`}
                                      onClick={(e) => e.stopPropagation()}
                                      className="p-2 bg-white/95 backdrop-blur-sm rounded-lg shadow-lg hover:bg-white transition-colors"
                                      title="Düzenle"
                                    >
                                      <Edit2 className="w-4 h-4 text-gray-700" />
                                    </Link>
                                    <button
                                      onClick={(e) => {
                                        e.preventDefault();
                                        e.stopPropagation();
                                        handleDeleteListing(listing.id, e);
                                      }}
                                      className="p-2 bg-white/95 backdrop-blur-sm rounded-lg shadow-lg hover:bg-red-50 transition-colors"
                                      title="Sil"
                                    >
                                      <Trash2 className="w-4 h-4 text-red-600" />
                                    </button>
                                  </div>
                                </div>
                              </div>
                        ))
                      )}
                    </div>
                  )}

                  {activeTab === 'applications' && (
                    <div className="space-y-3">
                      {myApplications.length === 0 ? (
                        <div className="text-center py-16">
                          <MessageCircle className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                          <p className="text-gray-500 font-poppins">Henüz başvuru yapmadınız</p>
                        </div>
                      ) : (
                        myApplications.map((application: any) => (
                          <div 
                            key={application.id} 
                            className="bg-white border border-gray-200 rounded-lg p-5 hover:shadow-md transition-all"
                          >
                            <div className="flex items-center justify-between">
                              <div className="flex-1 cursor-pointer" onClick={() => handleViewApplication(application)}>
                                <h3 className="font-poppins font-semibold text-gray-900 mb-2">
                                  {application.listingTitle}
                                </h3>
                                <p className="text-sm text-gray-600 font-poppins">
                                  {new Date(application.createdAt).toLocaleDateString('tr-TR', {
                                    day: 'numeric',
                                    month: 'long',
                                    year: 'numeric',
                                    timeZone: 'Europe/Istanbul'
                                  })}
                                </p>
                              </div>
                              <div className="flex items-center gap-3">
                                <span className={`px-3 py-1 rounded-full text-sm font-poppins ${
                                  application.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                                  application.status === 'Approved' || application.status === 'Accepted' ? 'bg-green-100 text-green-800' :
                                  application.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                                  application.status === 'Cancelled' ? 'bg-gray-100 text-gray-800' :
                                  'bg-gray-100 text-gray-800'
                                }`}>
                                  {application.status === 'Pending' ? 'Bekliyor' :
                                   application.status === 'Approved' || application.status === 'Accepted' ? 'Onaylandı' :
                                   application.status === 'Rejected' ? 'Reddedildi' :
                                   application.status === 'Cancelled' ? 'İptal Edildi' :
                                   application.status}
                                </span>
                                {(application.status === 'Accepted' || application.status === 'Completed') && (
                                  <button
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      handleOpenRatingModal(application);
                                    }}
                                    className="px-4 py-2 bg-yellow-600 text-white rounded-lg hover:bg-yellow-700 transition-colors font-poppins text-sm font-medium flex items-center gap-2"
                                  >
                                    <Star className="w-4 h-4" />
                                    {existingRating ? 'Değerlendirmeyi Düzenle' : 'Değerlendir'}
                                  </button>
                                )}
                                <button
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    navigate(`/messages?applicationId=${application.id}`);
                                  }}
                                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-poppins text-sm font-medium flex items-center gap-2"
                                >
                                  <MessageCircle className="w-4 h-4" />
                                  Mesajlaş
                                </button>
                              </div>
                            </div>
                          </div>
                        ))
                      )}
                    </div>
                  )}

                  {activeTab === 'favorites' && (
                    <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
                      {myFavorites.length === 0 ? (
                        <div className="col-span-full text-center py-16">
                          <Heart className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                          <p className="text-gray-500 font-poppins">Henüz favori ilanınız yok</p>
                        </div>
                      ) : (
                        myFavorites.map((favorite: any) => (
                          <Link key={favorite.id} to={`/listings/${favorite.listingId}`}>
                            <div className="bg-white border border-gray-200 rounded-lg overflow-hidden hover:shadow-lg transition-all cursor-pointer h-full">
                              <img
                                src={favorite.primaryPhotoUrl || 'https://via.placeholder.com/400x300'}
                                alt={favorite.listingTitle}
                                className="w-full h-48 object-cover"
                              />
                              <div className="p-4">
                                <h3 className="font-poppins font-semibold mb-2 line-clamp-1 text-gray-900">{favorite.listingTitle}</h3>
                                <div className="text-sm text-gray-600 font-poppins">
                                  {favorite.listingSpecies && (
                                    <span>{favorite.listingSpecies}</span>
                                  )}
                                  {favorite.listingBreed && (
                                    <span> • {favorite.listingBreed}</span>
                                  )}
                                </div>
                              </div>
                            </div>
                          </Link>
                        ))
                      )}
                    </div>
                  )}

                  {activeTab === 'donations' && (
                    <div className="space-y-4">
                      {myDonations.length === 0 ? (
                        <div className="text-center py-16">
                          <DollarSign className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                          <p className="text-gray-500 font-poppins">Henüz bağış yapmadınız</p>
                        </div>
                      ) : (
                        <>
                          <div className="bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg p-6 border border-blue-100">
                            <h3 className="text-sm font-medium text-gray-600 mb-2 font-poppins uppercase tracking-wide">Toplam Bağış</h3>
                            <p className="text-3xl font-bold text-gray-900 font-poppins">
                              {myDonations.reduce((sum: number, d: any) => sum + (d.amount || 0), 0).toFixed(2)} ₺
                            </p>
                          </div>
                          {myDonations.map((donation: any) => (
                            <div key={donation.id} className="bg-white border border-gray-200 rounded-lg p-5 hover:shadow-md transition-all">
                              <div className="flex items-center justify-between">
                                <div className="flex-1">
                                  <h3 className="font-poppins font-semibold text-gray-900 mb-1">
                                    {donation.listingTitle ? donation.listingTitle : 'Genel Barınak Bağışı'}
                                  </h3>
                                  {donation.message && (
                                    <p className="text-sm text-gray-600 mt-2 font-poppins">{donation.message}</p>
                                  )}
                                  <p className="text-xs text-gray-500 mt-3 font-poppins">
                                    {new Date(donation.createdAt).toLocaleDateString('tr-TR', {
                                      day: 'numeric',
                                      month: 'long',
                                      year: 'numeric',
                                      hour: '2-digit',
                                      minute: '2-digit',
                                      timeZone: 'Europe/Istanbul'
                                    })}
                                  </p>
                                </div>
                                <div className="text-right">
                                  <p className="text-2xl font-bold text-gray-900 font-poppins">
                                    {donation.amount?.toFixed(2)} ₺
                                  </p>
                                  <p className={`text-xs mt-2 px-2 py-1 rounded-full font-medium font-poppins inline-block ${
                                    donation.paymentStatus === 'Completed' ? 'bg-green-100 text-green-700' :
                                    donation.paymentStatus === 'Pending' ? 'bg-yellow-100 text-yellow-700' :
                                    'bg-red-100 text-red-700'
                                  }`}>
                                    {donation.paymentStatus === 'Completed' ? 'Tamamlandı' :
                                     donation.paymentStatus === 'Pending' ? 'Bekliyor' :
                                     'Başarısız'}
                                  </p>
                                </div>
                              </div>
                            </div>
                          ))}
                        </>
                      )}
                    </div>
                  )}

                  {activeTab === 'complaints' && (
                    <div className="space-y-4">
                      {myComplaints.length === 0 ? (
                        <div className="text-center py-16">
                          <Flag className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                          <p className="text-gray-500 font-poppins">Henüz şikayet yapmadınız</p>
                        </div>
                      ) : (
                        myComplaints.map((complaint: any) => (
                          <div
                            key={complaint.id}
                            className="bg-white border border-gray-200 rounded-lg p-5 hover:shadow-md transition-all"
                          >
                            <div className="flex items-start justify-between gap-4">
                              <div className="flex-1">
                                <div className="flex items-center gap-3 mb-2">
                                  <h3 className="font-poppins font-semibold text-gray-900">
                                    {complaint.targetUserId ? 'Kullanıcı Şikayeti' : complaint.targetListingId ? 'İlan Şikayeti' : 'Genel Şikayet'}
                                  </h3>
                                  <span className={`px-3 py-1 rounded-full text-xs font-poppins font-medium ${
                                    complaint.status === 'Open' ? 'bg-yellow-100 text-yellow-800' :
                                    complaint.status === 'Resolved' ? 'bg-green-100 text-green-800' :
                                    complaint.status === 'Dismissed' ? 'bg-gray-100 text-gray-800' :
                                    'bg-red-100 text-red-800'
                                  }`}>
                                    {complaint.status === 'Open' ? 'Açık' :
                                     complaint.status === 'Resolved' ? 'Çözüldü' :
                                     complaint.status === 'Dismissed' ? 'Reddedildi' :
                                     complaint.status}
                                  </span>
                                </div>
                                <p className="text-sm font-medium text-gray-700 mb-2 font-poppins">
                                  Sebep: {complaint.reason}
                                </p>
                                <p className="text-sm text-gray-600 mb-3 font-poppins">
                                  {complaint.description}
                                </p>
                                <p className="text-xs text-gray-500 font-poppins">
                                  {new Date(complaint.createdAt).toLocaleDateString('tr-TR', {
                                    day: 'numeric',
                                    month: 'long',
                                    year: 'numeric',
                                    hour: '2-digit',
                                    minute: '2-digit',
                                    timeZone: 'Europe/Istanbul'
                                  })}
                                </p>
                                {complaint.adminNotes && (
                                  <div className="mt-3 p-3 bg-blue-50 rounded-lg">
                                    <p className="text-xs font-semibold text-blue-900 font-poppins mb-1">Admin Notu:</p>
                                    <p className="text-sm text-blue-800 font-poppins">{complaint.adminNotes}</p>
                                  </div>
                                )}
                              </div>
                            </div>
                          </div>
                        ))
                      )}
                    </div>
                  )}
                </>
              )}
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Edit Profile Modal */}
      {showEditProfile && (
        <div className="fixed inset-0 z-[100] overflow-y-auto">
          {/* Backdrop */}
          <div
            className="fixed inset-0 bg-black/60 backdrop-blur-sm"
            onClick={() => {
              setShowEditProfile(false);
              setProfileImageFile(null);
              setProfileImagePreview(null);
            }}
          />
          
          {/* Modal Container */}
          <div className="relative min-h-screen flex items-center justify-center p-4">
            <div
              className="relative bg-white rounded-xl shadow-2xl w-full max-w-2xl my-8 overflow-hidden"
              onClick={(e) => e.stopPropagation()}
            >
              {/* Header */}
              <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center z-10">
                <h2 className="text-xl font-semibold text-gray-900 font-poppins">Profili Düzenle</h2>
                <button
                  onClick={() => {
                    setShowEditProfile(false);
                    setProfileImageFile(null);
                    setProfileImagePreview(null);
                  }}
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                >
                  <X className="w-5 h-5 text-gray-500" />
                </button>
              </div>

              {/* Form Content */}
              <form onSubmit={handleSaveProfile} className="p-6 space-y-5">
                <div className="grid md:grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                      Ad
                    </label>
                    <input
                      type="text"
                      value={profileData.firstName}
                      onChange={(e) => setProfileData({ ...profileData, firstName: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-poppins text-sm"
                      placeholder="Adınız"
                      required
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                      Soyad
                    </label>
                    <input
                      type="text"
                      value={profileData.lastName}
                      onChange={(e) => setProfileData({ ...profileData, lastName: e.target.value })}
                      className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-poppins text-sm"
                      placeholder="Soyadınız"
                      required
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    E-posta
                  </label>
                  <input
                    type="email"
                    value={profileData.email}
                    readOnly
                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg bg-gray-50 cursor-not-allowed font-poppins text-sm text-gray-600"
                    placeholder="E-posta adresiniz"
                  />
                  <p className="text-xs text-gray-500 mt-1.5 font-poppins">
                    E-posta adresi değiştirilemez
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Telefon Numarası
                  </label>
                  <input
                    type="tel"
                    value={profileData.phoneNumber}
                    onChange={(e) => setProfileData({ ...profileData, phoneNumber: e.target.value })}
                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-poppins text-sm"
                    placeholder="+90 555 123 45 67"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Şehir
                  </label>
                  <input
                    type="text"
                    value={profileData.city}
                    onChange={(e) => setProfileData({ ...profileData, city: e.target.value })}
                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-poppins text-sm"
                    placeholder="İstanbul"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Adres
                  </label>
                  <textarea
                    value={profileData.address}
                    onChange={(e) => setProfileData({ ...profileData, address: e.target.value })}
                    className="w-full px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 font-poppins text-sm resize-none"
                    placeholder="Adres bilgisi (isteğe bağlı)"
                    rows={3}
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Profil Fotoğrafı
                  </label>
                  <div className="space-y-3">
                    {/* Image Preview */}
                    {(profileImagePreview || profileData.profilePictureUrl) && (
                      <div className="flex items-center gap-4">
                        <div className="relative w-24 h-24 rounded-full overflow-hidden border-2 border-gray-200">
                          <img
                            src={profileImagePreview || profileData.profilePictureUrl}
                            alt="Profil önizleme"
                            className="w-full h-full object-cover"
                          />
                        </div>
                        {profileImageFile && (
                          <button
                            type="button"
                            onClick={() => {
                              setProfileImageFile(null);
                              setProfileImagePreview(null);
                            }}
                            className="text-sm text-red-600 hover:text-red-700 font-poppins font-medium"
                          >
                            Seçimi Kaldır
                          </button>
                        )}
                      </div>
                    )}
                    
                    {/* File Input */}
                    <label className="block w-full">
                      <input
                        type="file"
                        accept="image/*"
                        onChange={handleImageChange}
                        className="hidden"
                      />
                      <div className="w-full px-4 py-4 border-2 border-dashed border-gray-300 rounded-lg hover:border-blue-400 hover:bg-blue-50/50 transition-colors cursor-pointer text-center font-poppins">
                        <Upload className="w-5 h-5 mx-auto mb-2 text-gray-400" />
                        <span className="text-sm text-gray-700 font-medium block">
                          {profileImageFile ? profileImageFile.name : 'Fotoğraf yüklemek için tıklayın'}
                        </span>
                        <p className="text-xs text-gray-500 mt-1">PNG, JPG veya GIF (Max. 5MB)</p>
                      </div>
                    </label>
                  </div>
                </div>

                {/* Form Footer */}
                <div className="flex gap-3 pt-4 border-t border-gray-200">
                  <button
                    type="button"
                    onClick={() => {
                      setShowEditProfile(false);
                      setProfileImageFile(null);
                      setProfileImagePreview(null);
                    }}
                    className="flex-1 px-4 py-2.5 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 font-medium font-poppins transition-colors"
                  >
                    İptal
                  </button>
                  <button
                    type="submit"
                    disabled={uploadingProfile}
                    className="flex-1 px-4 py-2.5 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed font-medium font-poppins transition-colors"
                  >
                    {uploadingProfile ? 'Kaydediliyor...' : 'Kaydet'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Change Password Modal */}
      {showChangePassword && (
        <>
          <div
            className="fixed inset-0 bg-black/50 z-40"
            onClick={() => setShowChangePassword(false)}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-md"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="sticky top-0 bg-white border-b border-gray-200 p-6 flex justify-between items-center rounded-t-2xl">
                <h2 className="text-2xl font-poppins font-bold text-gray-900">Şifre Değiştir</h2>
                <button
                  onClick={() => setShowChangePassword(false)}
                  className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                >
                  <X className="w-6 h-6 text-gray-600" />
                </button>
              </div>

              <form onSubmit={handleChangePassword} className="p-6 space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Mevcut Şifre
                  </label>
                  <input
                    type="password"
                    value={passwordData.currentPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins"
                    required
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Yeni Şifre
                  </label>
                  <input
                    type="password"
                    value={passwordData.newPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins"
                    required
                    minLength={6}
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Yeni Şifre (Tekrar)
                  </label>
                  <input
                    type="password"
                    value={passwordData.confirmPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, confirmPassword: e.target.value })}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins"
                    required
                    minLength={6}
                  />
                </div>

                <div className="flex gap-3 pt-4 border-t">
                  <Button
                    type="button"
                    onClick={() => setShowChangePassword(false)}
                    variant="outline"
                    className="flex-1"
                  >
                    İptal
                  </Button>
                  <Button
                    type="submit"
                    className="flex-1 !bg-pink-600 hover:!bg-pink-700"
                  >
                    Şifreyi Değiştir
                  </Button>
                </div>
              </form>
            </div>
          </div>
        </>
      )}

      {/* Application Detail Modal */}
      {showApplicationDetail && selectedApplication && (
        <>
          <div
            className="fixed inset-0 bg-black/50 z-40"
            onClick={() => setShowApplicationDetail(false)}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-y-auto"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="sticky top-0 bg-white border-b border-gray-200 p-6 flex justify-between items-center rounded-t-2xl">
                <h2 className="text-2xl font-poppins font-bold text-gray-900">Başvuru Detayı</h2>
                <button
                  onClick={() => setShowApplicationDetail(false)}
                  className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                >
                  <X className="w-6 h-6 text-gray-600" />
                </button>
              </div>

              <div className="p-6 space-y-6">
                {/* Application Status */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Durum
                  </label>
                  <span className={`inline-block px-4 py-2 rounded-full text-sm font-poppins font-semibold ${
                    selectedApplication.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                    selectedApplication.status === 'Approved' || selectedApplication.status === 'Accepted' ? 'bg-green-100 text-green-800' :
                    selectedApplication.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                    selectedApplication.status === 'Cancelled' ? 'bg-gray-100 text-gray-800' :
                    'bg-gray-100 text-gray-800'
                  }`}>
                    {selectedApplication.status === 'Pending' ? 'Bekliyor' :
                     selectedApplication.status === 'Approved' || selectedApplication.status === 'Accepted' ? 'Onaylandı' :
                     selectedApplication.status === 'Rejected' ? 'Reddedildi' :
                     selectedApplication.status === 'Cancelled' ? 'İptal Edildi' :
                     selectedApplication.status}
                  </span>
                </div>

                {/* Listing Details */}
                {loadingListing ? (
                  <div className="text-center py-8">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-pink-600 mx-auto"></div>
                    <p className="mt-2 text-gray-600 font-poppins text-sm">İlan yükleniyor...</p>
                  </div>
                ) : applicationListing ? (
                  <>
                    <div className="grid md:grid-cols-2 gap-6">
                      <div>
                        <img
                          src={applicationListing.photoUrls?.[0] || 'https://via.placeholder.com/600x400?text=No+Image'}
                          alt={applicationListing.title}
                          className="w-full h-64 object-cover rounded-lg"
                        />
                      </div>
                      <div>
                        <h3 className="text-2xl font-poppins font-bold mb-4 text-gray-900">
                          {applicationListing.title}
                        </h3>
                        <div className="space-y-2 text-sm font-poppins">
                          {applicationListing.species && (
                            <p><span className="font-semibold">Tür:</span> {applicationListing.species}</p>
                          )}
                          {applicationListing.breed && (
                            <p><span className="font-semibold">Irk:</span> {applicationListing.breed}</p>
                          )}
                          {applicationListing.age && (
                            <p><span className="font-semibold">Yaş:</span> {applicationListing.age} ay</p>
                          )}
                          {applicationListing.gender && (
                            <p><span className="font-semibold">Cinsiyet:</span> {applicationListing.gender}</p>
                          )}
                          {applicationListing.city && (
                            <p><span className="font-semibold">Şehir:</span> {applicationListing.city}</p>
                          )}
                        </div>
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Açıklama
                      </label>
                      <p className="text-gray-700 font-poppins whitespace-pre-wrap bg-gray-50 p-4 rounded-lg">
                        {applicationListing.description}
                      </p>
                    </div>
                  </>
                ) : (
                  <div className="text-center py-8 text-gray-600 font-poppins">
                    İlan yüklenemedi
                  </div>
                )}

                {/* Application Info */}
                <div className="border-t pt-4">
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Başvuru Bilgileri
                  </label>
                  <div className="space-y-2 text-sm font-poppins">
                    <p><span className="font-semibold">Başvuru Tarihi:</span> {
                      new Date(selectedApplication.createdAt).toLocaleString('tr-TR', {
                        day: 'numeric',
                        month: 'long',
                        year: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit',
                        timeZone: 'Europe/Istanbul'
                      })
                    }</p>
                    {applicationListing && (
                      <p><span className="font-semibold">İlan Sahibi:</span> {applicationListing.ownerName}</p>
                    )}
                  </div>
                </div>

                {/* Actions */}
                {(selectedApplication.status === 'Pending' || selectedApplication.status === 'UnderReview') && (
                  <div className="flex gap-3 pt-4 border-t">
                    <Button
                      onClick={() => handleCancelApplication(selectedApplication.id)}
                      className="!bg-red-600 hover:!bg-red-700 flex-1"
                    >
                      <Trash2 className="w-4 h-4 mr-2" />
                      Başvuruyu İptal Et
                    </Button>
                    <Button
                      onClick={() => window.location.href = `/listings/${selectedApplication.listingId}`}
                      className="flex-1 !bg-pink-600 hover:!bg-pink-700"
                    >
                      İlanı Görüntüle
                    </Button>
                  </div>
                )}
                {(selectedApplication.status !== 'Pending' && selectedApplication.status !== 'UnderReview') && (
                  <div className="pt-4 border-t">
                    <Button
                      onClick={() => window.location.href = `/listings/${selectedApplication.listingId}`}
                      className="w-full !bg-pink-600 hover:!bg-pink-700"
                    >
                      İlanı Görüntüle
                    </Button>
                  </div>
                )}
              </div>
            </div>
          </div>
        </>
      )}

      {/* Listing Applications Modal */}
      {showListingApplications && (
        <div className="fixed inset-0 z-[100] overflow-y-auto">
          <div
            className="fixed inset-0 bg-black/60 backdrop-blur-sm"
            onClick={() => {
              setShowListingApplications(false);
              setSelectedListingId(null);
              setListingApplications([]);
            }}
          />
          
          <div className="relative min-h-screen flex items-center justify-center p-4">
            <div
              className="relative bg-white rounded-xl shadow-2xl w-full max-w-4xl my-8 overflow-hidden"
              onClick={(e) => e.stopPropagation()}
            >
              {/* Header */}
              <div className="sticky top-0 bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center z-10">
                <h2 className="text-xl font-semibold text-gray-900 font-poppins">İlan Başvuruları</h2>
                <button
                  onClick={() => {
                    setShowListingApplications(false);
                    setSelectedListingId(null);
                    setListingApplications([]);
                  }}
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                >
                  <X className="w-5 h-5 text-gray-500" />
                </button>
              </div>

              {/* Content */}
              <div className="p-6 max-h-[70vh] overflow-y-auto">
                {loadingApplications ? (
                  <div className="text-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
                    <p className="mt-4 text-gray-600 font-poppins">Başvurular yükleniyor...</p>
                  </div>
                ) : listingApplications.length === 0 ? (
                  <div className="text-center py-12">
                    <Users className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                    <p className="text-gray-500 font-poppins">Bu ilana henüz başvuru yapılmadı</p>
                  </div>
                ) : (
                  <div className="space-y-4">
                    {listingApplications.map((application: any) => (
                      <div
                        key={application.id}
                        className="bg-white border border-gray-200 rounded-lg p-5 hover:shadow-md transition-all"
                      >
                        <div className="flex items-start justify-between gap-4">
                          <div className="flex-1">
                            <div className="flex items-center gap-3 mb-3">
                              <h3 className="font-poppins font-semibold text-gray-900 text-lg">
                                {application.adopterName || 'Kullanıcı'}
                              </h3>
                              <span className={`px-3 py-1 rounded-full text-xs font-poppins font-medium ${
                                application.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                                application.status === 'Accepted' ? 'bg-green-100 text-green-800' :
                                application.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                                application.status === 'Cancelled' ? 'bg-gray-100 text-gray-800' :
                                'bg-blue-100 text-blue-800'
                              }`}>
                                {application.status === 'Pending' ? 'Bekliyor' :
                                 application.status === 'Accepted' ? 'Kabul Edildi' :
                                 application.status === 'Rejected' ? 'Reddedildi' :
                                 application.status === 'Cancelled' ? 'İptal Edildi' :
                                 application.status}
                              </span>
                            </div>
                            
                            {application.message && (
                              <div className="mb-3">
                                <p className="text-sm text-gray-600 font-poppins bg-gray-50 p-3 rounded-lg">
                                  {application.message}
                                </p>
                              </div>
                            )}
                            
                            <div className="text-xs text-gray-500 font-poppins">
                              <p>
                                Başvuru Tarihi: {new Date(application.createdAt).toLocaleDateString('tr-TR', {
                                  day: 'numeric',
                                  month: 'long',
                                  year: 'numeric',
                                  hour: '2-digit',
                                  minute: '2-digit',
                                  timeZone: 'Europe/Istanbul'
                                })}
                              </p>
                            </div>

                            {application.adminNotes && (
                              <div className="mt-3 p-3 bg-blue-50 rounded-lg">
                                <p className="text-xs font-semibold text-blue-900 font-poppins mb-1">Not:</p>
                                <p className="text-sm text-blue-800 font-poppins">{application.adminNotes}</p>
                              </div>
                            )}
                          </div>

                          {application.status === 'Pending' && (
                            <div className="flex flex-col gap-2">
                              <button
                                onClick={() => handleUpdateApplicationStatus(application.id, 'Accepted')}
                                disabled={updatingStatus === application.id}
                                className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-poppins font-medium text-sm flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                              >
                                <Check className="w-4 h-4" />
                                {updatingStatus === application.id ? 'İşleniyor...' : 'Kabul Et'}
                              </button>
                              <button
                                onClick={() => handleUpdateApplicationStatus(application.id, 'Rejected')}
                                disabled={updatingStatus === application.id}
                                className="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors font-poppins font-medium text-sm flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                              >
                                <XCircle className="w-4 h-4" />
                                {updatingStatus === application.id ? 'İşleniyor...' : 'Reddet'}
                              </button>
                              <button
                                onClick={() => navigate(`/messages?applicationId=${application.id}`)}
                                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-poppins font-medium text-sm flex items-center gap-2"
                              >
                                <MessageCircle className="w-4 h-4" />
                                Mesajlaş
                              </button>
                            </div>
                          )}

                          {application.status !== 'Pending' && (
                            <button
                              onClick={() => navigate(`/messages?applicationId=${application.id}`)}
                              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-poppins font-medium text-sm flex items-center gap-2"
                            >
                              <MessageCircle className="w-4 h-4" />
                              Mesajlaş
                            </button>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Rating Modal */}
      {showRatingModal && selectedApplicationForRating && (
        <>
          <div
            className="fixed inset-0 bg-black/50 backdrop-blur-sm z-50"
            onClick={() => {
              setShowRatingModal(false);
              setSelectedApplicationForRating(null);
              setExistingRating(null);
            }}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-md p-6"
              onClick={(e) => e.stopPropagation()}
            >
              <h3 className="text-xl font-poppins font-bold mb-4 text-gray-900">
                {existingRating ? 'Değerlendirmeyi Düzenle' : 'Değerlendirme Yap'}
              </h3>
              <p className="text-sm text-gray-600 mb-4 font-poppins">
                {selectedApplicationForRating.listingTitle} için değerlendirme yapın
              </p>
              
              <form onSubmit={handleSubmitRating} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Puan *
                  </label>
                  <div className="flex gap-2">
                    {[1, 2, 3, 4, 5].map((score) => (
                      <button
                        key={score}
                        type="button"
                        onClick={() => setRatingData({ ...ratingData, score })}
                        className={`flex-1 py-3 rounded-lg font-poppins font-semibold transition-all ${
                          ratingData.score === score
                            ? 'bg-yellow-500 text-white'
                            : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                        }`}
                      >
                        <div className="flex items-center justify-center gap-1">
                          <Star className={`w-5 h-5 ${ratingData.score >= score ? 'fill-current' : ''}`} />
                          <span>{score}</span>
                        </div>
                      </button>
                    ))}
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Yorum (İsteğe bağlı)
                  </label>
                  <textarea
                    value={ratingData.comment}
                    onChange={(e) => setRatingData({ ...ratingData, comment: e.target.value })}
                    className="w-full h-32 px-4 py-3 border border-gray-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-yellow-500 transition-colors font-poppins resize-none"
                    placeholder="Değerlendirmenizi yazın..."
                  />
                </div>

                <div className="flex gap-3 pt-4">
                  <button
                    type="button"
                    onClick={() => {
                      setShowRatingModal(false);
                      setSelectedApplicationForRating(null);
                      setExistingRating(null);
                    }}
                    className="flex-1 px-4 py-3 border border-gray-300 rounded-xl font-poppins font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                  >
                    İptal
                  </button>
                  <button
                    type="submit"
                    disabled={isSubmittingRating}
                    className="flex-1 px-4 py-3 bg-yellow-600 text-white rounded-xl font-poppins font-semibold hover:bg-yellow-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {isSubmittingRating ? 'Kaydediliyor...' : existingRating ? 'Güncelle' : 'Kaydet'}
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