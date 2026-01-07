import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { AlertCircle, ArrowLeft } from 'lucide-react';
import api from '../lib/api';
import Button from '../components/ui/Button';
import { useAuth } from '../contexts/AuthContext';

export default function EligibilityForm() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [existingForm, setExistingForm] = useState(false);

  const [formData, setFormData] = useState({
    livingType: '',
    hasGarden: false,
    hasBalcony: false,
    squareMeters: '',
    hasPreviousPetExperience: false,
    previousPetTypes: '',
    yearsOfExperience: '',
    householdMembers: '',
    hasChildren: false,
    childrenAge: '',
    allMembersAgree: false,
    workSchedule: '',
    hoursAwayFromHome: '',
    canSpendTimeWithPet: false,
    canAffordPetExpenses: false,
    monthlyBudgetForPet: '',
    additionalNotes: '',
  });

  useEffect(() => {
    if (!user) {
      navigate('/login');
      return;
    }
    checkExistingForm();
  }, [user, navigate]);

  const checkExistingForm = async () => {
    try {
      const response = await api.get('/eligibilityforms/my-form');
      if (response.data) {
        setExistingForm(true);
        // Mevcut formu yükle
        setFormData({
          livingType: response.data.livingType || '',
          hasGarden: response.data.hasGarden ?? false,
          hasBalcony: response.data.hasBalcony ?? false,
          squareMeters: response.data.squareMeters?.toString() || '',
          hasPreviousPetExperience: response.data.hasPreviousPetExperience ?? false,
          previousPetTypes: response.data.previousPetTypes || '',
          yearsOfExperience: response.data.yearsOfExperience?.toString() || '',
          householdMembers: response.data.householdMembers?.toString() || '',
          hasChildren: response.data.hasChildren ?? false,
          childrenAge: response.data.childrenAge?.toString() || '',
          allMembersAgree: response.data.allMembersAgree ?? false,
          workSchedule: response.data.workSchedule || '',
          hoursAwayFromHome: response.data.hoursAwayFromHome?.toString() || '',
          canSpendTimeWithPet: response.data.canSpendTimeWithPet ?? false,
          canAffordPetExpenses: response.data.canAffordPetExpenses ?? false,
          monthlyBudgetForPet: response.data.monthlyBudgetForPet?.toString() || '',
          additionalNotes: response.data.additionalNotes || '',
        });
      }
    } catch (error: any) {
      // Form yoksa 404 döner, bu normal
      if (error.response?.status !== 404) {
        console.error('Form kontrol edilemedi:', error);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleInputChange = (field: string, value: string | boolean | number) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      setIsSubmitting(true);

      const requestData = {
        livingType: formData.livingType || null,
        hasGarden: formData.hasGarden || null,
        hasBalcony: formData.hasBalcony || null,
        squareMeters: formData.squareMeters ? parseInt(formData.squareMeters) : null,
        hasPreviousPetExperience: formData.hasPreviousPetExperience || null,
        previousPetTypes: formData.previousPetTypes || null,
        yearsOfExperience: formData.yearsOfExperience ? parseInt(formData.yearsOfExperience) : null,
        householdMembers: formData.householdMembers ? parseInt(formData.householdMembers) : null,
        hasChildren: formData.hasChildren || null,
        childrenAge: formData.childrenAge ? parseInt(formData.childrenAge) : null,
        allMembersAgree: formData.allMembersAgree || null,
        workSchedule: formData.workSchedule || null,
        hoursAwayFromHome: formData.hoursAwayFromHome ? parseInt(formData.hoursAwayFromHome) : null,
        canSpendTimeWithPet: formData.canSpendTimeWithPet || null,
        canAffordPetExpenses: formData.canAffordPetExpenses || null,
        monthlyBudgetForPet: formData.monthlyBudgetForPet ? parseFloat(formData.monthlyBudgetForPet) : null,
        additionalNotes: formData.additionalNotes || null,
      };

      if (existingForm) {
        await api.put('/eligibilityforms', requestData);
        alert('Form başarıyla güncellendi!');
      } else {
        await api.post('/eligibilityforms', requestData);
        alert('Form başarıyla oluşturuldu! Artık ilanlara başvuru yapabilirsiniz.');
      }
      navigate('/profile');
    } catch (error: any) {
      console.error('Form kaydedilemedi:', error);
      setError(error.response?.data?.message || 'Form kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (!user) return null;

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
          <h1 className="text-3xl font-poppins font-bold mb-2 text-gray-900">
            Sahiplendirme Uygunluk Formu
          </h1>
          <p className="text-gray-600 mb-6 font-poppins">
            Bu form, evcil hayvan sahiplendirme sürecinde uygun bir aday olup olmadığınızı değerlendirmek için kullanılır.
          </p>

          <form onSubmit={handleSubmit} className="space-y-6">
            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg font-poppins text-sm flex items-center gap-2">
                <AlertCircle className="w-5 h-5" />
                {error}
              </div>
            )}

            {/* Yaşam Koşulları */}
            <div className="border-b border-gray-200 pb-6">
              <h2 className="text-xl font-poppins font-semibold mb-4 text-gray-900">Yaşam Koşulları</h2>
              
              <div className="grid md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Konut Tipi
                  </label>
                  <select
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                    value={formData.livingType}
                    onChange={(e) => handleInputChange('livingType', e.target.value)}
                  >
                    <option value="">Seçiniz</option>
                    <option value="Apartman">Apartman</option>
                    <option value="Müstakil Ev">Müstakil Ev</option>
                    <option value="Villa">Villa</option>
                    <option value="Bahçeli Ev">Bahçeli Ev</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Metrekare (m²)
                  </label>
                  <input
                    type="number"
                    min="0"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                    value={formData.squareMeters}
                    onChange={(e) => handleInputChange('squareMeters', e.target.value)}
                    placeholder="Örn: 80"
                  />
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="hasGarden"
                    className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                    checked={formData.hasGarden}
                    onChange={(e) => handleInputChange('hasGarden', e.target.checked)}
                  />
                  <label htmlFor="hasGarden" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                    Bahçem var
                  </label>
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="hasBalcony"
                    className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                    checked={formData.hasBalcony}
                    onChange={(e) => handleInputChange('hasBalcony', e.target.checked)}
                  />
                  <label htmlFor="hasBalcony" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                    Balkonum var
                  </label>
                </div>
              </div>
            </div>

            {/* Deneyim */}
            <div className="border-b border-gray-200 pb-6">
              <h2 className="text-xl font-poppins font-semibold mb-4 text-gray-900">Deneyim</h2>
              
              <div className="space-y-4">
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="hasPreviousPetExperience"
                    className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                    checked={formData.hasPreviousPetExperience}
                    onChange={(e) => handleInputChange('hasPreviousPetExperience', e.target.checked)}
                  />
                  <label htmlFor="hasPreviousPetExperience" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                    Daha önce evcil hayvan baktım
                  </label>
                </div>

                {formData.hasPreviousPetExperience && (
                  <>
                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Baktığınız Hayvan Türleri
                      </label>
                      <input
                        type="text"
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={formData.previousPetTypes}
                        onChange={(e) => handleInputChange('previousPetTypes', e.target.value)}
                        placeholder="Örn: Kedi, Köpek"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                        Deneyim Süresi (Yıl)
                      </label>
                      <input
                        type="number"
                        min="0"
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                        value={formData.yearsOfExperience}
                        onChange={(e) => handleInputChange('yearsOfExperience', e.target.value)}
                        placeholder="Örn: 5"
                      />
                    </div>
                  </>
                )}
              </div>
            </div>

            {/* Hane Halkı */}
            <div className="border-b border-gray-200 pb-6">
              <h2 className="text-xl font-poppins font-semibold mb-4 text-gray-900">Hane Halkı</h2>
              
              <div className="grid md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Hane Halkı Sayısı
                  </label>
                  <input
                    type="number"
                    min="1"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                    value={formData.householdMembers}
                    onChange={(e) => handleInputChange('householdMembers', e.target.value)}
                    placeholder="Örn: 2"
                  />
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="hasChildren"
                    className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                    checked={formData.hasChildren}
                    onChange={(e) => handleInputChange('hasChildren', e.target.checked)}
                  />
                  <label htmlFor="hasChildren" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                    Çocuk var
                  </label>
                </div>

                {formData.hasChildren && (
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                      Çocuk Yaşı
                    </label>
                    <input
                      type="number"
                      min="0"
                      className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                      value={formData.childrenAge}
                      onChange={(e) => handleInputChange('childrenAge', e.target.value)}
                      placeholder="Örn: 8"
                    />
                  </div>
                )}

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="allMembersAgree"
                    className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                    checked={formData.allMembersAgree}
                    onChange={(e) => handleInputChange('allMembersAgree', e.target.checked)}
                  />
                  <label htmlFor="allMembersAgree" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                    Tüm aile üyeleri evcil hayvan sahiplenmeyi onaylıyor
                  </label>
                </div>
              </div>
            </div>

            {/* İş ve Zaman */}
            <div className="border-b border-gray-200 pb-6">
              <h2 className="text-xl font-poppins font-semibold mb-4 text-gray-900">İş ve Zaman</h2>
              
              <div className="grid md:grid-cols-2 gap-6">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Çalışma Durumu
                  </label>
                  <select
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                    value={formData.workSchedule}
                    onChange={(e) => handleInputChange('workSchedule', e.target.value)}
                  >
                    <option value="">Seçiniz</option>
                    <option value="Full-time">Tam Zamanlı</option>
                    <option value="Part-time">Yarı Zamanlı</option>
                    <option value="Unemployed">İşsiz</option>
                    <option value="Student">Öğrenci</option>
                    <option value="Retired">Emekli</option>
                    <option value="Home-based">Evden Çalışıyorum</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Evden Uzakta Geçirdiğiniz Süre (Saat/Gün)
                  </label>
                  <input
                    type="number"
                    min="0"
                    max="24"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                    value={formData.hoursAwayFromHome}
                    onChange={(e) => handleInputChange('hoursAwayFromHome', e.target.value)}
                    placeholder="Örn: 8"
                  />
                </div>

                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="canSpendTimeWithPet"
                    className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                    checked={formData.canSpendTimeWithPet}
                    onChange={(e) => handleInputChange('canSpendTimeWithPet', e.target.checked)}
                  />
                  <label htmlFor="canSpendTimeWithPet" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                    Evcil hayvanımla yeterince zaman geçirebileceğim
                  </label>
                </div>
              </div>
            </div>

            {/* Mali Durum */}
            <div className="border-b border-gray-200 pb-6">
              <h2 className="text-xl font-poppins font-semibold mb-4 text-gray-900">Mali Durum</h2>
              
              <div className="grid md:grid-cols-2 gap-6">
                <div className="flex items-center">
                  <input
                    type="checkbox"
                    id="canAffordPetExpenses"
                    className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
                    checked={formData.canAffordPetExpenses}
                    onChange={(e) => handleInputChange('canAffordPetExpenses', e.target.checked)}
                  />
                  <label htmlFor="canAffordPetExpenses" className="ml-2 text-sm text-gray-700 font-poppins cursor-pointer">
                    Evcil hayvan giderlerini karşılayabilirim
                  </label>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                    Aylık Bütçe (₺)
                  </label>
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins"
                    value={formData.monthlyBudgetForPet}
                    onChange={(e) => handleInputChange('monthlyBudgetForPet', e.target.value)}
                    placeholder="Örn: 500"
                  />
                </div>
              </div>
            </div>

            {/* Ek Notlar */}
            <div>
              <h2 className="text-xl font-poppins font-semibold mb-4 text-gray-900">Ek Notlar</h2>
              
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Ek Bilgiler
                </label>
                <textarea
                  rows={4}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 transition-colors font-poppins resize-none"
                  value={formData.additionalNotes}
                  onChange={(e) => handleInputChange('additionalNotes', e.target.value)}
                  placeholder="Eklemek istediğiniz başka bilgiler..."
                />
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
                {isSubmitting ? 'Kaydediliyor...' : existingForm ? 'Formu Güncelle' : 'Formu Kaydet'}
              </Button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

