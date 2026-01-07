import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  BarChart3, Users, FileText, CheckCircle, XCircle, Shield, 
  AlertCircle, DollarSign, Eye, X, Lock
} from 'lucide-react';
import api from '../lib/api';
import { useAuth } from '../contexts/AuthContext';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';

interface AdminReports {
  totalUsers: number;
  activeUsers: number;
  bannedUsers: number;
  shelterUsers: number;
  verifiedShelters: number;
  totalListings: number;
  activeListings: number;
  pendingApprovalListings: number;
  adoptionListings: number;
  lostListings: number;
  helpRequestListings: number;
  totalApplications: number;
  pendingApplications: number;
  acceptedApplications: number;
  completedAdoptions: number;
  totalDonations: number;
  totalDonationAmount: number;
  totalComplaints: number;
  openComplaints: number;
  resolvedComplaints: number;
  pendingStories: number;
  approvedStories: number;
}

interface PendingListing {
  id: string;
  title: string;
  type: number;
  ownerName: string;
  ownerEmail: string;
  createdAt: string;
  primaryPhotoUrl?: string;
}

interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  city?: string;
  isAdmin: boolean;
  isShelter: boolean;
  isShelterVerified: boolean;
  isActive: boolean;
  isBanned: boolean;
  createdAt: string;
  listingCount: number;
  applicationCount: number;
}

type Tab = 'dashboard' | 'listings' | 'users';

export default function AdminDashboard() {
  const navigate = useNavigate();
  const { user, login } = useAuth();
  const [activeTab, setActiveTab] = useState<Tab>('dashboard');
  const [reports, setReports] = useState<AdminReports | null>(null);
  const [pendingListings, setPendingListings] = useState<PendingListing[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [loadingListings, setLoadingListings] = useState(false);
  const [loadingUsers, setLoadingUsers] = useState(false);
  const [selectedListing, setSelectedListing] = useState<PendingListing | null>(null);
  const [showListingModal, setShowListingModal] = useState(false);
  const [showUserModal, setShowUserModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [approvalNotes, setApprovalNotes] = useState('');
  const [isApproving, setIsApproving] = useState(false);
  const [userStatusUpdate, setUserStatusUpdate] = useState({
    isActive: false,
    isBanned: false,
    isShelterVerified: false,
    adminNotes: ''
  });
  const [showAdminLogin, setShowAdminLogin] = useState(false);
  const [adminLoginData, setAdminLoginData] = useState({
    email: '',
    password: ''
  });
  const [adminLoginError, setAdminLoginError] = useState('');
  const [isLoggingIn, setIsLoggingIn] = useState(false);

  useEffect(() => {
    if (!user) {
      setShowAdminLogin(true);
      return;
    }
    if (!user.isAdmin) {
      setShowAdminLogin(true);
      return;
    }
    setShowAdminLogin(false);
  }, [user]);

  const handleAdminLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setAdminLoginError('');
    setIsLoggingIn(true);

    try {
      await login(adminLoginData.email, adminLoginData.password);
      
      const currentUser = JSON.parse(localStorage.getItem('user') || '{}');
      if (!currentUser.isAdmin) {
        setAdminLoginError('Bu hesap admin yetkisine sahip değil.');
        setIsLoggingIn(false);
        return;
      }
      
      setShowAdminLogin(false);
      window.location.reload(); // Reload to refresh user context
    } catch (error: any) {
      console.error('Admin girişi başarısız:', error);
      setAdminLoginError(error.response?.data?.message || 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.');
    } finally {
      setIsLoggingIn(false);
    }
  };

  useEffect(() => {
    if (activeTab === 'dashboard') {
      loadReports();
    } else if (activeTab === 'listings') {
      loadPendingListings();
    } else if (activeTab === 'users') {
      loadUsers();
    }
  }, [activeTab]);

  const loadReports = async () => {
    try {
      setIsLoading(true);
      const response = await api.get('/admin/reports');
      setReports(response.data);
    } catch (error) {
      console.error('Raporlar yüklenemedi:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadPendingListings = async () => {
    try {
      setLoadingListings(true);
      const response = await api.get('/admin/listings/pending');
      setPendingListings(response.data);
    } catch (error) {
      console.error('Onay bekleyen ilanlar yüklenemedi:', error);
    } finally {
      setLoadingListings(false);
    }
  };

  const loadUsers = async () => {
    try {
      setLoadingUsers(true);
      const response = await api.get('/admin/users?Page=1&PageSize=50');
      setUsers(response.data);
    } catch (error) {
      console.error('Kullanıcılar yüklenemedi:', error);
    } finally {
      setLoadingUsers(false);
    }
  };

  const handleApproveListing = async (isApproved: boolean) => {
    if (!selectedListing) return;

    try {
      setIsApproving(true);
      await api.post(`/admin/listings/${selectedListing.id}/approve`, {
        isApproved,
        adminNotes: approvalNotes || undefined
      });
      alert(`İlan başarıyla ${isApproved ? 'onaylandı' : 'reddedildi'}.`);
      setShowListingModal(false);
      setSelectedListing(null);
      setApprovalNotes('');
      loadPendingListings();
    } catch (error: any) {
      console.error('İlan onaylanamadı:', error);
      alert(error.response?.data?.message || 'İlan onaylanırken bir hata oluştu.');
    } finally {
      setIsApproving(false);
    }
  };

  const handleUpdateUserStatus = async () => {
    if (!selectedUser) return;

    try {
      const updateData: any = {
        adminNotes: userStatusUpdate.adminNotes || undefined
      };
      
      if (userStatusUpdate.isActive !== selectedUser.isActive) {
        updateData.isActive = userStatusUpdate.isActive;
      }
      if (userStatusUpdate.isBanned !== selectedUser.isBanned) {
        updateData.isBanned = userStatusUpdate.isBanned;
      }
      if (selectedUser.isShelter && userStatusUpdate.isShelterVerified !== selectedUser.isShelterVerified) {
        updateData.isShelterVerified = userStatusUpdate.isShelterVerified;
      }

      await api.put(`/admin/users/${selectedUser.id}/status`, updateData);
      alert('Kullanıcı durumu başarıyla güncellendi.');
      setShowUserModal(false);
      setSelectedUser(null);
      setUserStatusUpdate({
        isActive: false,
        isBanned: false,
        isShelterVerified: false,
        adminNotes: ''
      });
      loadUsers();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Kullanıcı durumu güncellenirken bir hata oluştu.');
    }
  };

  const getListingTypeLabel = (type: number) => {
    switch (type) {
      case 1: return 'Sahiplendirme';
      case 2: return 'Kayıp Hayvan';
      case 3: return 'Yardım İsteği';
      default: return 'Bilinmeyen';
    }
  };

  if (showAdminLogin || !user?.isAdmin) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-pink-50 via-purple-50 to-blue-50 flex items-center justify-center pt-24 pb-8 px-4">
        <Card className="w-full max-w-md p-8">
          <div className="text-center mb-6">
            <div className="w-16 h-16 bg-gradient-to-br from-pink-500 to-purple-500 rounded-full flex items-center justify-center mx-auto mb-4">
              <Shield className="w-8 h-8 text-white" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 font-poppins">Admin Girişi</h1>
            <p className="text-gray-600 mt-2 font-poppins">Yönetici paneline erişmek için giriş yapın</p>
          </div>

          <form onSubmit={handleAdminLogin} className="space-y-4">
            {adminLoginError && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg font-poppins text-sm">
                {adminLoginError}
              </div>
            )}

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                E-posta
              </label>
              <input
                type="email"
                value={adminLoginData.email}
                onChange={(e) => setAdminLoginData({ ...adminLoginData, email: e.target.value })}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                placeholder="admin@petadoption.com"
                required
                autoFocus
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                Şifre
              </label>
              <input
                type="password"
                value={adminLoginData.password}
                onChange={(e) => setAdminLoginData({ ...adminLoginData, password: e.target.value })}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                placeholder="••••••••"
                required
              />
            </div>

            <Button
              type="submit"
              disabled={isLoggingIn}
              className="w-full !bg-gradient-to-r !from-pink-500 !to-purple-500 hover:!from-pink-600 hover:!to-purple-600"
            >
              <Lock className="w-4 h-4 mr-2" />
              {isLoggingIn ? 'Giriş yapılıyor...' : 'Giriş Yap'}
            </Button>

            <div className="text-center mt-4">
              <button
                type="button"
                onClick={() => navigate('/')}
                className="text-sm text-gray-600 hover:text-gray-900 font-poppins"
              >
                Ana sayfaya dön
              </button>
            </div>
          </form>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 pt-24 pb-8">
      <div className="container mx-auto px-4">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-3xl font-bold mb-6 font-poppins">Admin Dashboard</h1>

          {/* Tabs */}
          <div className="bg-white rounded-lg shadow-md mb-6">
            <div className="flex border-b overflow-x-auto">
              <button
                onClick={() => setActiveTab('dashboard')}
                className={`flex-shrink-0 px-6 py-4 font-medium transition-colors font-poppins ${
                  activeTab === 'dashboard'
                    ? 'text-pink-600 border-b-2 border-pink-600'
                    : 'text-gray-600 hover:text-gray-900'
                }`}
              >
                <BarChart3 className="w-5 h-5 inline-block mr-2" />
                Dashboard
              </button>
              <button
                onClick={() => setActiveTab('listings')}
                className={`flex-shrink-0 px-6 py-4 font-medium transition-colors font-poppins ${
                  activeTab === 'listings'
                    ? 'text-pink-600 border-b-2 border-pink-600'
                    : 'text-gray-600 hover:text-gray-900'
                }`}
              >
                <FileText className="w-5 h-5 inline-block mr-2" />
                Onay Bekleyen İlanlar ({pendingListings.length})
              </button>
              <button
                onClick={() => setActiveTab('users')}
                className={`flex-shrink-0 px-6 py-4 font-medium transition-colors font-poppins ${
                  activeTab === 'users'
                    ? 'text-pink-600 border-b-2 border-pink-600'
                    : 'text-gray-600 hover:text-gray-900'
                }`}
              >
                <Users className="w-5 h-5 inline-block mr-2" />
                Kullanıcı Yönetimi
              </button>
            </div>

            <div className="p-6">
              {activeTab === 'dashboard' && (
                <>
                  {isLoading ? (
                    <div className="text-center py-12">
                      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600 mx-auto"></div>
                    </div>
                  ) : reports && (
                    <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-6">
                      {/* Users Stats */}
                      <Card className="p-6">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="text-gray-600 font-poppins text-sm mb-1">Toplam Kullanıcı</p>
                            <p className="text-3xl font-bold text-gray-900">{reports.totalUsers}</p>
                            <p className="text-sm text-gray-500 mt-1">{reports.activeUsers} aktif</p>
                          </div>
                          <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
                            <Users className="w-6 h-6 text-blue-600" />
                          </div>
                        </div>
                      </Card>

                      <Card className="p-6">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="text-gray-600 font-poppins text-sm mb-1">Barınak</p>
                            <p className="text-3xl font-bold text-gray-900">{reports.shelterUsers}</p>
                            <p className="text-sm text-gray-500 mt-1">{reports.verifiedShelters} doğrulanmış</p>
                          </div>
                          <div className="w-12 h-12 bg-purple-100 rounded-full flex items-center justify-center">
                            <Shield className="w-6 h-6 text-purple-600" />
                          </div>
                        </div>
                      </Card>

                      <Card className="p-6">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="text-gray-600 font-poppins text-sm mb-1">Toplam İlan</p>
                            <p className="text-3xl font-bold text-gray-900">{reports.totalListings}</p>
                            <p className="text-sm text-gray-500 mt-1">{reports.pendingApprovalListings} onay bekliyor</p>
                          </div>
                          <div className="w-12 h-12 bg-green-100 rounded-full flex items-center justify-center">
                            <FileText className="w-6 h-6 text-green-600" />
                          </div>
                        </div>
                      </Card>

                      <Card className="p-6">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="text-gray-600 font-poppins text-sm mb-1">Tamamlanan Sahiplendirme</p>
                            <p className="text-3xl font-bold text-gray-900">{reports.completedAdoptions}</p>
                            <p className="text-sm text-gray-500 mt-1">{reports.totalApplications} toplam başvuru</p>
                          </div>
                          <div className="w-12 h-12 bg-pink-100 rounded-full flex items-center justify-center">
                            <CheckCircle className="w-6 h-6 text-pink-600" />
                          </div>
                        </div>
                      </Card>

                      <Card className="p-6">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="text-gray-600 font-poppins text-sm mb-1">Toplam Bağış</p>
                            <p className="text-3xl font-bold text-gray-900">{reports.totalDonations}</p>
                            <p className="text-sm text-gray-500 mt-1">{reports.totalDonationAmount.toFixed(2)} ₺</p>
                          </div>
                          <div className="w-12 h-12 bg-yellow-100 rounded-full flex items-center justify-center">
                            <DollarSign className="w-6 h-6 text-yellow-600" />
                          </div>
                        </div>
                      </Card>

                      <Card className="p-6">
                        <div className="flex items-center justify-between">
                          <div>
                            <p className="text-gray-600 font-poppins text-sm mb-1">Şikayetler</p>
                            <p className="text-3xl font-bold text-gray-900">{reports.totalComplaints}</p>
                            <p className="text-sm text-gray-500 mt-1">{reports.openComplaints} açık</p>
                          </div>
                          <div className="w-12 h-12 bg-red-100 rounded-full flex items-center justify-center">
                            <AlertCircle className="w-6 h-6 text-red-600" />
                          </div>
                        </div>
                      </Card>
                    </div>
                  )}
                </>
              )}

              {activeTab === 'listings' && (
                <>
                  {loadingListings ? (
                    <div className="text-center py-12">
                      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600 mx-auto"></div>
                    </div>
                  ) : pendingListings.length === 0 ? (
                    <div className="text-center py-12 text-gray-600">
                      Onay bekleyen ilan yok
                    </div>
                  ) : (
                    <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-4">
                      {pendingListings.map((listing) => (
                        <Card key={listing.id} className="p-4 hover:shadow-lg transition-shadow cursor-pointer" onClick={() => {
                          setSelectedListing(listing);
                          setShowListingModal(true);
                        }}>
                          {listing.primaryPhotoUrl && (
                            <img
                              src={listing.primaryPhotoUrl}
                              alt={listing.title}
                              className="w-full h-40 object-cover rounded-lg mb-3"
                            />
                          )}
                          <h3 className="font-semibold mb-2 line-clamp-2">{listing.title}</h3>
                          <div className="text-sm text-gray-600 space-y-1">
                            <p><span className="font-medium">Tür:</span> {getListingTypeLabel(listing.type)}</p>
                            <p><span className="font-medium">İlan Sahibi:</span> {listing.ownerName}</p>
                            <p><span className="font-medium">E-posta:</span> {listing.ownerEmail}</p>
                            <p><span className="font-medium">Tarih:</span> {
                              new Date(listing.createdAt).toLocaleDateString('tr-TR', {
                                day: 'numeric',
                                month: 'long',
                                year: 'numeric',
                                timeZone: 'Europe/Istanbul'
                              })
                            }</p>
                          </div>
                          <div className="flex gap-2 mt-4">
                            <Button
                              className="flex-1"
                              onClick={(e) => {
                                e.stopPropagation();
                                window.open(`/listings/${listing.id}`, '_blank');
                              }}
                            >
                              <Eye className="w-4 h-4 mr-2" />
                              Detay
                            </Button>
                            <Button
                              className="flex-1"
                              onClick={(e) => {
                                e.stopPropagation();
                                setSelectedListing(listing);
                                setShowListingModal(true);
                              }}
                            >
                              İncele
                            </Button>
                          </div>
                        </Card>
                      ))}
                    </div>
                  )}
                </>
              )}

              {activeTab === 'users' && (
                <>
                  {loadingUsers ? (
                    <div className="text-center py-12">
                      <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-pink-600 mx-auto"></div>
                    </div>
                  ) : (
                    <div className="overflow-x-auto">
                      <table className="w-full">
                        <thead>
                          <tr className="border-b">
                            <th className="text-left py-3 px-4 font-poppins font-semibold">Kullanıcı</th>
                            <th className="text-left py-3 px-4 font-poppins font-semibold">E-posta</th>
                            <th className="text-left py-3 px-4 font-poppins font-semibold">Durum</th>
                            <th className="text-left py-3 px-4 font-poppins font-semibold">İlan</th>
                            <th className="text-left py-3 px-4 font-poppins font-semibold">Başvuru</th>
                            <th className="text-left py-3 px-4 font-poppins font-semibold">İşlemler</th>
                          </tr>
                        </thead>
                        <tbody>
                          {users.map((u) => (
                            <tr key={u.id} className="border-b hover:bg-gray-50">
                              <td className="py-3 px-4">
                                <div>
                                  <p className="font-medium">{u.firstName} {u.lastName}</p>
                                  {u.city && <p className="text-sm text-gray-500">{u.city}</p>}
                                </div>
                              </td>
                              <td className="py-3 px-4">{u.email}</td>
                              <td className="py-3 px-4">
                                <div className="flex flex-wrap gap-1">
                                  {u.isAdmin && <span className="px-2 py-1 bg-purple-100 text-purple-700 text-xs rounded">Admin</span>}
                                  {u.isShelter && <span className="px-2 py-1 bg-blue-100 text-blue-700 text-xs rounded">Barınak</span>}
                                  {u.isShelterVerified && <span className="px-2 py-1 bg-green-100 text-green-700 text-xs rounded">Doğrulanmış</span>}
                                  {!u.isActive && <span className="px-2 py-1 bg-gray-100 text-gray-700 text-xs rounded">Pasif</span>}
                                  {u.isBanned && <span className="px-2 py-1 bg-red-100 text-red-700 text-xs rounded">Yasaklı</span>}
                                </div>
                              </td>
                              <td className="py-3 px-4">{u.listingCount}</td>
                              <td className="py-3 px-4">{u.applicationCount}</td>
                              <td className="py-3 px-4">
                                <Button
                                  variant="outline"
                                  onClick={() => {
                                    setSelectedUser(u);
                                    setUserStatusUpdate({
                                      isActive: u.isActive,
                                      isBanned: u.isBanned,
                                      isShelterVerified: u.isShelterVerified || false,
                                      adminNotes: ''
                                    });
                                    setShowUserModal(true);
                                  }}
                                >
                                  Düzenle
                                </Button>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Listing Approval Modal */}
      {showListingModal && selectedListing && (
        <>
          <div
            className="fixed inset-0 bg-black/50 z-40"
            onClick={() => {
              setShowListingModal(false);
              setSelectedListing(null);
              setApprovalNotes('');
            }}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="sticky top-0 bg-white border-b border-gray-200 p-6 flex justify-between items-center rounded-t-2xl">
                <h2 className="text-2xl font-poppins font-bold text-gray-900">İlan İnceleme</h2>
                <button
                  onClick={() => {
                    setShowListingModal(false);
                    setSelectedListing(null);
                    setApprovalNotes('');
                  }}
                  className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                >
                  <X className="w-6 h-6 text-gray-600" />
                </button>
              </div>

              <div className="p-6 space-y-4">
                {selectedListing.primaryPhotoUrl && (
                  <img
                    src={selectedListing.primaryPhotoUrl}
                    alt={selectedListing.title}
                    className="w-full h-64 object-cover rounded-lg"
                  />
                )}
                <div>
                  <h3 className="text-xl font-bold mb-2">{selectedListing.title}</h3>
                  <div className="space-y-2 text-sm">
                    <p><span className="font-semibold">Tür:</span> {getListingTypeLabel(selectedListing.type)}</p>
                    <p><span className="font-semibold">İlan Sahibi:</span> {selectedListing.ownerName}</p>
                    <p><span className="font-semibold">E-posta:</span> {selectedListing.ownerEmail}</p>
                    <p><span className="font-semibold">Oluşturulma Tarihi:</span> {
                      new Date(selectedListing.createdAt).toLocaleDateString('tr-TR', {
                        day: 'numeric',
                        month: 'long',
                        year: 'numeric',
                        timeZone: 'Europe/Istanbul'
                      })
                    }</p>
                  </div>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Admin Notları (Opsiyonel)
                  </label>
                  <textarea
                    value={approvalNotes}
                    onChange={(e) => setApprovalNotes(e.target.value)}
                    className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins"
                    rows={3}
                    placeholder="İlan hakkında notlarınız..."
                  />
                </div>

                <div className="flex gap-3 pt-4 border-t">
                  <Button
                    onClick={() => handleApproveListing(false)}
                    disabled={isApproving}
                    className="flex-1 !bg-red-600 hover:!bg-red-700"
                  >
                    <XCircle className="w-4 h-4 mr-2" />
                    Reddet
                  </Button>
                  <Button
                    onClick={() => handleApproveListing(true)}
                    disabled={isApproving}
                    className="flex-1 !bg-green-600 hover:!bg-green-700"
                  >
                    <CheckCircle className="w-4 h-4 mr-2" />
                    Onayla
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </>
      )}

      {/* User Status Update Modal */}
      {showUserModal && selectedUser && (
        <>
          <div
            className="fixed inset-0 bg-black/50 z-40"
            onClick={() => {
              setShowUserModal(false);
              setSelectedUser(null);
            }}
          />
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <div
              className="bg-white rounded-2xl shadow-2xl w-full max-w-md"
              onClick={(e) => e.stopPropagation()}
            >
              <div className="sticky top-0 bg-white border-b border-gray-200 p-6 flex justify-between items-center rounded-t-2xl">
                <h2 className="text-2xl font-poppins font-bold text-gray-900">Kullanıcı Durumu</h2>
                <button
                  onClick={() => {
                    setShowUserModal(false);
                    setSelectedUser(null);
                  }}
                  className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                >
                  <X className="w-6 h-6 text-gray-600" />
                </button>
              </div>

              <div className="p-6 space-y-4">
                <div>
                  <p className="font-semibold">{selectedUser.firstName} {selectedUser.lastName}</p>
                  <p className="text-sm text-gray-600">{selectedUser.email}</p>
                </div>

                <div className="space-y-3">
                  <label className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={userStatusUpdate.isActive}
                      onChange={(e) => setUserStatusUpdate({ ...userStatusUpdate, isActive: e.target.checked })}
                      className="w-4 h-4"
                    />
                    <span className="font-poppins">Aktif</span>
                  </label>

                  <label className="flex items-center gap-2">
                    <input
                      type="checkbox"
                      checked={userStatusUpdate.isBanned}
                      onChange={(e) => setUserStatusUpdate({ ...userStatusUpdate, isBanned: e.target.checked })}
                      className="w-4 h-4"
                    />
                    <span className="font-poppins">Yasakla</span>
                  </label>

                  {selectedUser.isShelter && (
                    <label className="flex items-center gap-2">
                      <input
                        type="checkbox"
                        checked={userStatusUpdate.isShelterVerified}
                        onChange={(e) => setUserStatusUpdate({ ...userStatusUpdate, isShelterVerified: e.target.checked })}
                        className="w-4 h-4"
                      />
                      <span className="font-poppins">Barınak Doğrulaması</span>
                    </label>
                  )}

                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                      Admin Notları (Opsiyonel)
                    </label>
                    <textarea
                      value={userStatusUpdate.adminNotes}
                      onChange={(e) => setUserStatusUpdate({ ...userStatusUpdate, adminNotes: e.target.value })}
                      className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins"
                      rows={3}
                      placeholder="Notlarınız..."
                    />
                  </div>
                </div>

                <div className="flex gap-3 pt-4 border-t">
                  <Button
                    onClick={() => {
                      setShowUserModal(false);
                      setSelectedUser(null);
                    }}
                    variant="outline"
                    className="flex-1"
                  >
                    İptal
                  </Button>
                  <Button
                    onClick={handleUpdateUserStatus}
                    className="flex-1 !bg-pink-600 hover:!bg-pink-700"
                  >
                    Kaydet
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

