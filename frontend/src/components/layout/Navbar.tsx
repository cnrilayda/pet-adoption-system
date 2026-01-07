import { useState, useEffect, useRef } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Heart, MessageCircle, User, LogOut, Shield } from 'lucide-react';
import { useAuth } from '../../contexts/AuthContext';
import api from '../../lib/api';

export default function Navbar() {
  const { user, logout } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [isScrolled, setIsScrolled] = useState(false);
  const [showProfileDropdown, setShowProfileDropdown] = useState(false);
  const profileDropdownRef = useRef<HTMLDivElement>(null);
  const isHomePage = location.pathname === '/';
  const [unreadCount, setUnreadCount] = useState(0);

  const handleAboutClick = (e: React.MouseEvent<HTMLAnchorElement>) => {
    e.preventDefault();
    if (isHomePage) {
      const element = document.getElementById('hakkimizda');
      if (element) {
        const navbarHeight = 100;
        const offsetPosition = element.offsetTop - navbarHeight;
        window.scrollTo({ top: offsetPosition, behavior: 'smooth' });
      }
    } else {
      navigate('/#hakkimizda', { replace: false });
    }
  };

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 50);
    };

    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  useEffect(() => {
    if (!user) {
      setUnreadCount(0);
      return;
    }

    const fetchUnreadCount = async () => {
      try {
        const response = await api.get('/messages/unread/count');
        setUnreadCount(response.data.unreadCount || 0);
      } catch (error) {
      }
    };

    fetchUnreadCount();
    const interval = setInterval(fetchUnreadCount, 30000);
    return () => clearInterval(interval);
  }, [user, location.pathname]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (profileDropdownRef.current && !profileDropdownRef.current.contains(event.target as Node)) {
        setShowProfileDropdown(false);
      }
    };

    if (showProfileDropdown) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showProfileDropdown]);

  const handleLogout = () => {
    logout();
    navigate('/');
    setShowProfileDropdown(false);
  };

  const shouldShowOpaqueBg = !isHomePage || isScrolled;
  const getBackgroundColor = () => {
    if (!isHomePage) return '#fffcf1'; // Ana sayfa dışında siyah
    if (isScrolled) return '#fffcf1'; 
    return 'transparent'; 
  };

  return (
    <nav 
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 backdrop-blur-sm border-b ${
        shouldShowOpaqueBg
          ? isHomePage ? 'border-[#FFA696]/20' : 'border-gray-800' 
          : 'bg-transparent border-transparent'
      }`}
      style={{ backgroundColor: getBackgroundColor() }}
    >
      <div className="w-full px-12">
        <div className="grid grid-cols-3 items-center h-16">
          {/* Left Menu */}
          <div className="hidden md:flex items-center gap-9 justify-start pl-1">
            <a 
              href="#hakkimizda" 
              onClick={handleAboutClick}
              className={`text-base font-poppins font-medium transition cursor-pointer ${
                shouldShowOpaqueBg
                  ? 'text-black hover:text-gray-100' 
                  : 'text-black hover:text-pink-200'
              }`}
            >
              Hakkımızda
            </a>
            <Link to="/listings" className={`text-base font-poppins font-medium transition ${
              shouldShowOpaqueBg
                ? 'text-black hover:text-gray-100' 
                : 'text-black hover:text-pink-200'
            }`}>
              İlanlar
            </Link>
            <Link to="/lost-pets" className={`text-base font-poppins font-medium transition ${
              shouldShowOpaqueBg
                ? 'text-black hover:text-gray-100' 
                : 'text-black hover:text-pink-200'
            }`}>
              Kayıp
            </Link>
            <Link to="/donations" className={`text-base font-poppins font-medium transition ${
              shouldShowOpaqueBg
                ? 'text-black hover:text-gray-100' 
                : 'text-black hover:text-pink-200'
            }`}>
              Bağış
            </Link>
            <Link to="/stories" className={`text-base font-poppins font-medium transition ${
              shouldShowOpaqueBg
                ? 'text-black hover:text-gray-100' 
                : 'text-black hover:text-pink-200'
            }`}>
              Hikayeler
            </Link>
          </div>

          {/* Center Logo with Hover */}
          <Link to="/" className="relative group flex items-center justify-center h-16">
            <div className="relative w-48 h-16 flex items-center justify-center overflow-visible">
              {/* Text - Default */}
              <div className="flex items-center justify-center transition-all duration-300 group-hover:opacity-0 group-hover:scale-0">
                <span 
                  className="text-3xl whitespace-nowrap transition-colors text-black"
                  style={{ fontFamily: 'Caprasimo, cursive' }}
                >
                  PembePati
                </span>
              </div>
              {/* Logo - Hover */}
              <div className="absolute inset-0 flex items-center justify-center opacity-0 scale-0 group-hover:opacity-100 group-hover:scale-100 transition-all duration-300">
                <img 
                  src="/src/assets/logo_transparent.png" 
                  alt="PembePati" 
                  className="h-16 w-16"
                />
              </div>
            </div>
          </Link>

          {/* Right Auth Buttons */}
          <div className="flex items-center gap-3 justify-end pr-1">
            {user ? (
              <>
                <Link to="/favorites">
                  <button className={`w-9 h-9 rounded-full flex items-center justify-center transition ${
                    shouldShowOpaqueBg
                      ? 'bg-white/20 text-black hover:bg-white/30' 
                      : 'bg-white/20 text-black hover:bg-white/30'
                  }`}>
                    <Heart className="w-4 h-4" />
                  </button>
                </Link>
                <Link to="/messages" className="relative">
                  <button className={`w-9 h-9 rounded-full flex items-center justify-center transition ${
                    shouldShowOpaqueBg
                      ? 'bg-white/20 text-black hover:bg-white/30' 
                      : 'bg-white/20 text-black hover:bg-white/30'
                  }`}>
                    <MessageCircle className="w-4 h-4" />
                  </button>
                  {unreadCount > 0 && (
                    <span className="absolute -top-1 -right-1 bg-red-600 text-white text-xs font-bold rounded-full w-5 h-5 flex items-center justify-center">
                      {unreadCount > 9 ? '9+' : unreadCount}
                    </span>
                  )}
                </Link>
                <div className="relative" ref={profileDropdownRef}>
                  <button 
                    onClick={() => setShowProfileDropdown(!showProfileDropdown)}
                    className={`w-9 h-9 rounded-full flex items-center justify-center transition ${
                      shouldShowOpaqueBg
                        ? 'bg-white/30 text-black hover:bg-white/40' 
                        : 'bg-white/30 text-black hover:bg-white/40'
                    }`}
                  >
                    <User className="w-4 h-4" />
                  </button>
                  
                  {/* Dropdown Menu */}
                  {showProfileDropdown && (
                    <div className="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-50">
                      <Link 
                        to="/profile"
                        onClick={() => setShowProfileDropdown(false)}
                        className="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 transition-colors font-poppins"
                      >
                        <User className="w-4 h-4" />
                        Profil
                      </Link>
                      {user?.isAdmin && (
                        <Link 
                          to="/admin"
                          onClick={() => setShowProfileDropdown(false)}
                          className="flex items-center gap-2 px-4 py-2 text-sm text-purple-700 hover:bg-purple-50 transition-colors font-poppins"
                        >
                          <Shield className="w-4 h-4" />
                          Admin Panel
                        </Link>
                      )}
                      <button
                        onClick={handleLogout}
                        className="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50 transition-colors font-poppins text-left"
                      >
                        <LogOut className="w-4 h-4" />
                        Çıkış Yap
                      </button>
                    </div>
                  )}
                </div>
              </>
            ) : (
              <Link to="/login">
                <button className="px-5 py-1.5 rounded-lg text-base font-poppins font-medium transition bg-black/20 text-black hover:bg-black/30 backdrop-blur-sm">
                  Giriş Yap
                </button>
              </Link>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}

