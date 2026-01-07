import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import Input from '../components/ui/Input';
import logo from '../assets/logo_transparent.png';
import pati2 from '../assets/pati2.png';

export default function Register() {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    city: '',
    isShelter: false,
  });
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [generalError, setGeneralError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrors({});
    setGeneralError('');

    if (formData.password !== formData.confirmPassword) {
      setErrors({ confirmPassword: 'Şifreler eşleşmiyor' });
      return;
    }

    setIsLoading(true);

    try {
      await register(formData);
      navigate('/');
    } catch (err: any) {
      if (err.response?.data && Array.isArray(err.response.data)) {
        const fieldErrors: Record<string, string> = {};
        err.response.data.forEach((error: any) => {
          const fieldName = error.propertyName?.charAt(0).toLowerCase() + error.propertyName?.slice(1);
          if (fieldName) {
            fieldErrors[fieldName] = error.errorMessage || 'Geçersiz değer';
          }
        });
        setErrors(fieldErrors);
      } else {
        setGeneralError(err.response?.data?.message || 'Kayıt olunamadı. Lütfen tekrar deneyin.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  return (
    <div className="min-h-screen flex items-center justify-center py-12 px-4 relative overflow-hidden" style={{ backgroundColor: '#fffcf1' }}>
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute left-[-130px] top-[-80px] w-[400px] h-[400px] rotate-[-290deg] opacity-30"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute left-[-100px] top-[50vh] -translate-y-1/2 w-56 h-56 rotate-80 opacity-25"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute left-[-10px] top-[75vh] w-64 h-64 rotate-12 opacity-25"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute right-[-140px] top-[-80px] w-[350px] h-[350px] rotate-[-70deg] opacity-30"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute right-[-10px] top-[50vh] -translate-y-1/2 w-64 h-64 rotate-[-120deg] opacity-25"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute right-[-100px] top-[70vh] w-52 h-52 rotate-[-15deg] opacity-25"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute left-[30%] top-[10vh] w-28 h-28 rotate-12 opacity-20"
      />
            <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute left-[50%] top-[80vh] w-28 h-28 rotate-12 opacity-20"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute left-[16%] top-[50vh] -translate-y-1/2 w-32 h-32 rotate-45 opacity-20"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute left-[17%] top-[85vh] w-36 h-36 rotate-75 opacity-20"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute right-[25%] top-[15vh] w-40 h-40 rotate-[-30deg] opacity-20"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute right-[16%] top-[55vh] -translate-y-1/2 w-32 h-32 rotate-[45deg] opacity-20"
      />
      <img
        src={pati2}
        alt="Pati deseni"
        className="hidden md:block pointer-events-none select-none absolute right-[17%] top-[90vh] w-32 h-32 rotate-[-90deg] opacity-20"
      />
      
      <div className="max-w-md w-full relative z-10">
        <div className="text-center mb-8">
          <Link to="/" className="inline-flex items-center gap-3 mb-6">
            <img 
              src={logo}
              alt="PembePati" 
              className="h-16 w-16 object-contain"
            />
            <span 
              className="text-4xl text-gray-900"
              style={{ fontFamily: 'Caprasimo, cursive' }}
            >
              PembePati
            </span>
          </Link>
          <h2 className="text-4xl font-poppins font-black text-gray-900 mb-4">Kayıt Ol</h2>
          <p className="text-gray-600 font-poppins">
            Zaten hesabınız var mı?{' '}
            <Link to="/login" className="text-pink-600 font-poppins font-semibold hover:underline">
              Giriş yapın
            </Link>
          </p>
        </div>

        <div className="bg-white rounded-2xl shadow-xl p-8">
          <form onSubmit={handleSubmit} className="space-y-5">
            {generalError && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg font-poppins">
                {generalError}
              </div>
            )}

            <div className="grid grid-cols-2 gap-4">
              <Input
                label="Ad"
                name="firstName"
                value={formData.firstName}
                onChange={handleChange}
                error={errors.firstName}
                required
              />
              <Input
                label="Soyad"
                name="lastName"
                value={formData.lastName}
                onChange={handleChange}
                error={errors.lastName}
                required
              />
            </div>

            <Input
              label="E-posta"
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              placeholder="ornek@email.com"
              error={errors.email}
              required
            />

            <Input
              label="Telefon"
              type="tel"
              name="phoneNumber"
              value={formData.phoneNumber}
              onChange={handleChange}
              placeholder="0555 123 4567"
              error={errors.phoneNumber}
            />

            <Input
              label="Şehir"
              name="city"
              value={formData.city}
              onChange={handleChange}
              placeholder="İstanbul"
              error={errors.city}
            />

            <Input
              label="Şifre"
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              placeholder="••••••••"
              error={errors.password}
              required
            />

            <Input
              label="Şifre Tekrar"
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              placeholder="••••••••"
              error={errors.confirmPassword}
              required
            />

            <label className="flex items-center gap-2 font-poppins">
              <input
                type="checkbox"
                name="isShelter"
                checked={formData.isShelter}
                onChange={handleChange}
                className="w-4 h-4 text-pink-600 border-gray-300 rounded focus:ring-pink-500"
              />
              <span className="text-sm text-gray-700">Barınak veya kuruluş hesabı</span>
            </label>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-pink-600 text-white px-8 py-4 rounded-full font-poppins font-semibold hover:scale-105 transition-transform text-lg shadow-xl disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
            >
              {isLoading ? 'Kayıt yapılıyor...' : 'Kayıt Ol'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

