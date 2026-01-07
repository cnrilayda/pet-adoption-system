import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AuthProvider } from './contexts/AuthContext';
import Navbar from './components/layout/Navbar';
import { useState, useEffect } from 'react';
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Listings from './pages/Listings';
import ListingDetail from './pages/ListingDetail';
import Profile from './pages/Profile';
import Messages from './pages/Messages';
import ApplyListing from './pages/ApplyListing';
import Donations from './pages/Donations';
import DonationDetail from './pages/DonationDetail';
import GeneralDonation from './pages/GeneralDonation';
import CreateLostPetListing from './pages/CreateLostPetListing';
import LostPetListings from './pages/LostPetListings';
import CreatePetListing from './pages/CreatePetListing';
import EligibilityForm from './pages/EligibilityForm';
import Favorites from './pages/Favorites';
import AdminDashboard from './pages/AdminDashboard';
import Stories from './pages/Stories';

const queryClient = new QueryClient();

function Footer() {
  const [email, setEmail] = useState('');

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <footer className="bg-gray-900 text-white">
      <div className="container mx-auto px-8 py-12">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-8 mb-8">
          {/* Newsletter Section */}
          <div className="lg:col-span-2">
            <h3 className="text-lg font-semibold mb-4 font-poppins">HABERDAR OLUN</h3>
            <p className="text-sm text-gray-400 mb-4 font-poppins">
              Yeni ilanlar ve duyurular için e-posta adresinizi girin
            </p>
            <div className="flex flex-col sm:flex-row gap-2">
              <input
                type="email"
                placeholder="E-posta adresiniz"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="px-4 py-2 bg-gray-800 border-b border-gray-700 focus:outline-none focus:border-white text-white placeholder-gray-500 font-poppins text-sm"
              />
              <button className="px-6 py-2 bg-white text-gray-900 font-semibold hover:bg-gray-100 transition-colors font-poppins text-sm whitespace-nowrap">
                Abone Ol
              </button>
            </div>
          </div>

          {/* İLANLAR Column */}
          <div>
            <h4 className="text-sm font-semibold mb-4 uppercase tracking-wider font-poppins">İLANLAR</h4>
            <ul className="space-y-2">
              <li>
                <Link to="/listings" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Tüm İlanlar
                </Link>
              </li>
              <li>
                <Link to="/listings?type=Sahiplendirme" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Sahiplendirme
                </Link>
              </li>
              <li>
                <Link to="/listings?type=Kayip" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Kayıp Hayvanlar
                </Link>
              </li>
              <li>
                <Link to="/listings?type=Bulundu" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Bulunan Hayvanlar
                </Link>
              </li>
            </ul>
          </div>

          {/* HAKKIMIZDA Column */}
          <div>
            <h4 className="text-sm font-semibold mb-4 uppercase tracking-wider font-poppins">HAKKIMIZDA</h4>
            <ul className="space-y-2">
              <li>
                <Link to="/about" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Hikayemiz
                </Link>
              </li>
              <li>
                <Link to="/stories" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Başarı Hikayeleri
                </Link>
              </li>
              <li>
                <Link to="/donations" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Bağış
                </Link>
              </li>
              <li>
                <a href="#" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Bize Ulaşın
                </a>
              </li>
            </ul>
          </div>

          {/* YARDIM Column */}
          <div>
            <h4 className="text-sm font-semibold mb-4 uppercase tracking-wider font-poppins">YARDIM</h4>
            <ul className="space-y-2">
              <li>
                <Link to="/profile" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Hesabım
                </Link>
              </li>
              <li>
                <Link to="/messages" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Mesajlarım
                </Link>
              </li>
              <li>
                <a href="#" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  SSS
                </a>
              </li>
              <li>
                <a href="#" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  İletişim
                </a>
              </li>
            </ul>
          </div>
        </div>

        {/* Bottom Section */}
        <div className="border-t border-gray-800 pt-8 mt-8">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4">
            <div className="flex flex-col md:flex-row items-center gap-4 md:gap-6">
              <p className="text-gray-400 text-sm font-poppins">
                &copy; 2024 PembePati. Tüm hakları saklıdır.
              </p>
              <div className="flex gap-4">
                <a href="#" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Gizlilik Politikası
                </a>
                <a href="#" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Kullanım Şartları
                </a>
                <a href="#" className="text-gray-400 hover:text-white text-sm font-poppins transition-colors">
                  Çerez Politikası
                </a>
              </div>
            </div>
            <div className="flex gap-4">
              <a href="#" className="text-gray-400 hover:text-white transition-colors" aria-label="Instagram">
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 2.163c3.204 0 3.584.012 4.85.07 3.252.148 4.771 1.691 4.919 4.919.058 1.265.069 1.645.069 4.849 0 3.205-.012 3.584-.069 4.849-.149 3.225-1.664 4.771-4.919 4.919-1.266.058-1.644.07-4.85.07-3.204 0-3.584-.012-4.849-.07-3.26-.149-4.771-1.699-4.919-4.92-.058-1.265-.07-1.644-.07-4.849 0-3.204.013-3.583.07-4.849.149-3.227 1.664-4.771 4.919-4.919 1.266-.057 1.645-.069 4.849-.069zm0-2.163c-3.259 0-3.667.014-4.947.072-4.358.2-6.78 2.618-6.98 6.98-.059 1.281-.073 1.689-.073 4.948 0 3.259.014 3.668.072 4.948.2 4.358 2.618 6.78 6.98 6.98 1.281.058 1.689.072 4.948.072 3.259 0 3.668-.014 4.948-.072 4.354-.2 6.782-2.618 6.979-6.98.059-1.28.073-1.689.073-4.948 0-3.259-.014-3.667-.072-4.947-.196-4.354-2.617-6.78-6.979-6.98-1.281-.059-1.69-.073-4.949-.073zm0 5.838c-3.403 0-6.162 2.759-6.162 6.162s2.759 6.163 6.162 6.163 6.162-2.759 6.162-6.163c0-3.403-2.759-6.162-6.162-6.162zm0 10.162c-2.209 0-4-1.79-4-4 0-2.209 1.791-4 4-4s4 1.791 4 4c0 2.21-1.791 4-4 4zm6.406-11.845c-.796 0-1.441.645-1.441 1.44s.645 1.44 1.441 1.44c.795 0 1.439-.645 1.439-1.44s-.644-1.44-1.439-1.44z"/>
                </svg>
              </a>
              <a href="#" className="text-gray-400 hover:text-white transition-colors" aria-label="Twitter">
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M23.953 4.57a10 10 0 01-2.825.775 4.958 4.958 0 002.163-2.723c-.951.555-2.005.959-3.127 1.184a4.92 4.92 0 00-8.384 4.482C7.69 8.095 4.067 6.13 1.64 3.162a4.822 4.822 0 00-.666 2.475c0 1.71.87 3.213 2.188 4.096a4.904 4.904 0 01-2.228-.616v.06a4.923 4.923 0 003.946 4.827 4.996 4.996 0 01-2.212.085 4.936 4.936 0 004.604 3.417 9.867 9.867 0 01-6.102 2.105c-.39 0-.779-.023-1.17-.067a13.995 13.995 0 007.557 2.209c9.053 0 13.998-7.496 13.998-13.985 0-.21 0-.42-.015-.63A9.935 9.935 0 0024 4.59z"/>
                </svg>
              </a>
              <a href="#" className="text-gray-400 hover:text-white transition-colors" aria-label="Facebook">
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
                </svg>
              </a>
            </div>
            <button
              onClick={scrollToTop}
              className="w-10 h-10 border border-gray-700 hover:border-white flex items-center justify-center transition-colors"
              aria-label="Yukarı çık"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 10l7-7m0 0l7 7m-7-7v18" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </footer>
  );
}

function ScrollToTop() {
  const location = useLocation();

  useEffect(() => {
    if (!location.hash) {
      window.scrollTo(0, 0);
    }
  }, [location.pathname, location.hash]);

  return null;
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <Router>
          <ScrollToTop />
          <div className="min-h-screen flex flex-col">
            <Navbar />
            <main className="flex-1">
              <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/listings" element={<Listings />} />
                <Route path="/listings/:id" element={<ListingDetail />} />
                <Route path="/listings/:id/apply" element={<ApplyListing />} />
                <Route path="/eligibility-form" element={<EligibilityForm />} />
                <Route path="/profile" element={<Profile />} />
                <Route path="/messages" element={<Messages />} />
                <Route path="/donations" element={<Donations />} />
                <Route path="/donations/:id" element={<DonationDetail />} />
                <Route path="/donations/general" element={<GeneralDonation />} />
                <Route path="/listings/create" element={<CreatePetListing />} />
                <Route path="/lost-pets" element={<LostPetListings />} />
                <Route path="/lost-pet/create" element={<CreateLostPetListing />} />
                <Route path="/stories" element={<Stories />} />
                <Route path="/about" element={<div className="p-8 text-center">Hakkımızda yakında...</div>} />
                <Route path="/favorites" element={<Favorites />} />
                <Route path="/admin" element={<AdminDashboard />} />
              </Routes>
            </main>
            <Footer />
          </div>
        </Router>
      </AuthProvider>
    </QueryClientProvider>
  );
}

export default App;
