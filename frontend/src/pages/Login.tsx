import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import Input from '../components/ui/Input';
import logo from '../assets/logo_transparent.png';
import pati2 from '../assets/pati2.png';

export default function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [generalError, setGeneralError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrors({});
    setGeneralError('');
    setIsLoading(true);

    try {
      await login(email, password);
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
        setGeneralError(err.response?.data?.message || 'Giriş yapılamadı. Lütfen bilgilerinizi kontrol edin.');
      }
    } finally {
      setIsLoading(false);
    }
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
          <h2 className="text-4xl font-poppins font-black text-gray-900 mb-4">Giriş Yap</h2>
          <p className="text-gray-600 font-poppins">
            Hesabınız yok mu?{' '}
            <Link to="/register" className="text-pink-600 font-poppins font-semibold hover:underline">
              Kayıt olun
            </Link>
          </p>
        </div>

        <div className="bg-white rounded-2xl shadow-xl p-8">
          <form onSubmit={handleSubmit} className="space-y-6">
            {generalError && (
              <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg font-poppins">
                {generalError}
              </div>
            )}

            <Input
              label="E-posta"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="ornek@email.com"
              error={errors.email}
              required
            />

            <Input
              label="Şifre"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              error={errors.password}
              required
            />

            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-pink-600 text-white px-8 py-4 rounded-full font-poppins font-semibold hover:scale-105 transition-transform text-lg shadow-xl disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
            >
              {isLoading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

