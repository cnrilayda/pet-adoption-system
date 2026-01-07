import { useState, FormEvent } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import api from '../lib/api';
import Button from '../components/ui/Button';
import Card from '../components/ui/Card';

export default function ApplyListing() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    message: '',
  });
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setIsLoading(true);

    try {
      await api.post('/applications', {
        listingId: id,
        message: formData.message || null,
      });
      alert('Başvurunuz başarıyla gönderildi!');
      navigate('/profile');
    } catch (err: any) {
      const errorMessage = err.response?.data?.message || 'Başvuru gönderilemedi';
      setError(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setFormData(prev => ({
      ...prev,
      message: e.target.value
    }));
  };

  return (
    <div className="min-h-screen bg-gray-50 pt-24 pb-8">
      <div className="container mx-auto px-4">
        <div className="max-w-2xl mx-auto">
          <h1 className="text-3xl font-bold mb-6">Başvuru Formu</h1>

          <Card className="p-6">
            <form onSubmit={handleSubmit} className="space-y-4">
              {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                  {error}
                  {(error.includes('eligibility') || error.includes('Eligibility') || error.includes('complete the Adoption Eligibility Form')) ? (
                    <div className="mt-3">
                      <p className="mb-2 text-sm">Başvuru yapmak için önce Sahiplendirme Uygunluk Formu'nu doldurmalısınız.</p>
                      <Link 
                        to="/eligibility-form" 
                        className="inline-block px-4 py-2 bg-pink-600 text-white rounded-lg hover:bg-pink-700 transition-colors font-medium"
                      >
                        Uygunluk Formunu Doldur
                      </Link>
                    </div>
                  ) : null}
                </div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2 font-poppins">
                  Mesajınız (Opsiyonel)
                </label>
                <p className="text-sm text-gray-600 mb-3 font-poppins">
                  İlan sahibine iletmek istediğiniz mesajınızı buraya yazabilirsiniz. 
                  Detaylı bilgileriniz için daha önce doldurduğunuz Uygunluk Formu kullanılacaktır.
                </p>
                <textarea
                  name="message"
                  value={formData.message}
                  onChange={handleChange}
                  rows={6}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 font-poppins resize-none"
                  placeholder="İlan sahibine özel mesajınızı buraya yazabilirsiniz..."
                />
              </div>

              <div className="flex gap-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate(-1)}
                  className="flex-1"
                >
                  İptal
                </Button>
                <Button
                  type="submit"
                  disabled={isLoading}
                  className="flex-1"
                >
                  {isLoading ? 'Gönderiliyor...' : 'Başvuru Gönder'}
                </Button>
              </div>
            </form>
          </Card>
        </div>
      </div>
    </div>
  );
}


