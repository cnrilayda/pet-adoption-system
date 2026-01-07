import { useEffect, useRef, useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import mainImage from '../assets/main.png';
import logo from '../assets/logo_transparent.png';
import pati from '../assets/pati.png';
import donatePopup from '../assets/donatepopup.png';
import howToUse from '../assets/howtouse.png';
import pati2 from '../assets/pati2.png';
import icon1 from '../assets/1.png';
import icon2 from '../assets/2.png';
import icon3 from '../assets/3.png';
import icon4 from '../assets/4.png';
import icon5 from '../assets/5.png';
import icon6 from '../assets/6.png';
import api from '../lib/api';

interface Listing {
  id: string;
  title: string;
  description: string;
  species: string | null;
  breed: string | null;
  age: number | null;
  gender: string | null;
  city: string | null;
  photoUrls: string[];
  type: number;
  isVaccinated: boolean | null;
  isNeutered: boolean | null;
  ownerName: string;
  isShelter: boolean;
}

export default function Home() {
  const location = useLocation();
  const [showDonationModal, setShowDonationModal] = useState(false);
  const statsRef = useRef<HTMLDivElement | null>(null);
  const [statsStarted, setStatsStarted] = useState(false);
  const [stats, setStats] = useState({
    adoptions: 0,
    listings: 0,
    members: 0,
    satisfaction: 0,
  });
  const [featuredListings, setFeaturedListings] = useState<Listing[]>([]);

  const targetStats = {
    adoptions: 1200,
    listings: 560,
    members: 720,
    satisfaction: 4.8,
  };

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowDonationModal(true);
    }, 1200);
    return () => clearTimeout(timer);
  }, []);

  useEffect(() => {
    const fetchFeaturedListings = async () => {
      try {
        const response = await api.get('/petlistings?pageSize=10');
        const listings = response.data || [];
        setFeaturedListings(listings);
      } catch (error) {
        console.error('Featured listings yüklenemedi:', error);
      }
    };
    fetchFeaturedListings();
  }, []);

  useEffect(() => {
    const hash = location.hash || window.location.hash;
    if (hash === '#hakkimizda' || hash.includes('hakkimizda')) {
      const elementId = 'hakkimizda';
      
      const performScroll = () => {
        const element = document.getElementById(elementId);
        if (element && element.offsetTop > 0) {
          const navbarHeight = 100;
          const offsetPosition = Math.max(0, element.offsetTop - navbarHeight);
          window.scrollTo({ top: offsetPosition, behavior: 'smooth' });
          return true;
        }
        return false;
      };
      
      let retries = 0;
      const maxRetries = 30;
      const retryInterval = setInterval(() => {
        retries++;
        if (performScroll() || retries >= maxRetries) {
          clearInterval(retryInterval);
        }
      }, 30);
      
      performScroll();
      
      return () => clearInterval(retryInterval);
    }
  }, [location]);

  useEffect(() => {
    if (!statsRef.current || statsStarted) return;

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            setStatsStarted(true);
          }
        });
      },
      { threshold: 0.35 }
    );

    observer.observe(statsRef.current);
    return () => observer.disconnect();
  }, [statsStarted]);

  useEffect(() => {
    if (!statsStarted) return;

    const duration = 1200;
    const start = performance.now();
    const from = { ...stats };
    const to = targetStats;

    const step = (now: number) => {
      const progress = Math.min((now - start) / duration, 1);
      setStats({
        adoptions: from.adoptions + (to.adoptions - from.adoptions) * progress,
        listings: from.listings + (to.listings - from.listings) * progress,
        members: from.members + (to.members - from.members) * progress,
        satisfaction: from.satisfaction + (to.satisfaction - from.satisfaction) * progress,
      });
      if (progress < 1) requestAnimationFrame(step);
    };

    const frame = requestAnimationFrame(step);
    return () => cancelAnimationFrame(frame);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [statsStarted]);

  const formatStat = (value: number, key: keyof typeof targetStats) => {
    if (key === 'satisfaction') return value.toFixed(1);
    if (value >= 1000) return `${(value / 1000).toFixed(1)}K`;
    return Math.round(value).toString();
  };

  return (
    <div className="min-h-screen scroll-smooth overflow-x-hidden" style={{ backgroundColor: '#fffcf1' }}>
      {/* Donation Modal */}
      {showDonationModal && (
        <div
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/60 px-4"
          onClick={() => setShowDonationModal(false)}
        >
          <div
            className="relative max-w-xl w-full shadow-2xl overflow-hidden"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Background image */}
            <img
              src={donatePopup}
              alt="Bağış çağrısı"
              className="w-full h-full object-cover"
            />

            {/* Close button */}
            <button
              aria-label="Kapat"
              className="absolute top-1 right-4 z-10 text-black text-xl font-bold drop-shadow cursor-pointer"
              onClick={() => setShowDonationModal(false)}
            >
              ✕
            </button>

            {/* Logo + PembePati text */}
            <div className="absolute top-1 inset-x-0 flex items-center justify-center">
              <img
                src={logo}
                alt="PembePati"
                className="h-16 w-16 object-contain drop-shadow transform scale-110"
              />
              <span
                className="text-3xl text-black drop-shadow"
                style={{ fontFamily: 'Caprasimo, cursive' }}
              >
                PembePati
              </span>
            </div>

            {/* Donate button at bottom */}
            <div className="absolute bottom-4 inset-x-0 flex justify-center px-8">
              <Link to="/donations" onClick={() => setShowDonationModal(false)} className="w-full max-w-xs">
                <button className="w-full bg-white text-black px-8 py-3 font-poppins font-bold text-xl shadow-xl hover:scale-105 transition-transform">
                  BAĞIŞ YAP
                </button>
              </Link>
            </div>
          </div>
        </div>
      )}

      {/* Hero Section - Full Screen Image */}
      <section id="anasayfa" className="relative w-full h-screen overflow-hidden">
        {/* Full Screen Background Image */}
        <div 
          className="absolute inset-0 w-full h-full bg-cover bg-center bg-no-repeat"
          style={{ backgroundImage: `url(${mainImage})` }}
        />
        
        {/* Content Overlay */}
        <div className="absolute inset-0 z-10 flex flex-col md:flex-row items-center md:items-center justify-between gap-8 px-8 md:px-16">
          {/* Left Side - Main Heading */}
          <div className="text-white max-w-3xl">
            <h1 className="text-5xl md:text-7xl font-poppins font-black mb-6 leading-tight drop-shadow-2xl text-left">
              Bir can bekliyor, çünkü umudu sensin.
            </h1>
          </div>

          {/* Right Side - Subheading and Buttons */}
          <div className="text-white max-w-2xl">
            <p className="text-xl md:text-2xl text-white/90 mb-8 drop-shadow-lg font-poppins text-right">
              Sevgi dolu bir yuva arayan dostlarımızı keşfedin
            </p>
            <div className="flex gap-4 justify-end">
              <Link to="/listings">
                <button className="bg-white text-black px-8 py-4 rounded-full font-poppins font-semibold hover:scale-105 transition-transform text-lg shadow-xl">
                  Şimdi Keşfet
                </button>
              </Link>

            </div>
          </div>

          
        </div>
      </section>



      {/* Stats - Modern Clean Style */}
      <section
        ref={statsRef}
        className="py-20 relative"
        style={{ backgroundColor: '#fffcf1' }}
      >
        <img
          src={pati}
          alt="Pati deseni"
          className="pointer-events-none select-none absolute left-[-40px] md:left-[-64px] top-1/2 -translate-y-1/2 w-72 md:w-80 rotate-45"
        />
        <div className="container mx-auto px-8 relative">
          <div className="grid md:grid-cols-4 gap-8 text-center">
            <div>
              <div className="text-4xl md:text-5xl font-poppins font-black text-gray-900 mb-2">
                {formatStat(stats.adoptions, 'adoptions')}
              </div>
              <div className="text-base text-gray-600 font-poppins">Mutlu Sahiplenme</div>
            </div>
            <div>
              <div className="text-4xl md:text-5xl font-poppins font-black text-gray-900 mb-2">
                {formatStat(stats.listings, 'listings')}
              </div>
              <div className="text-base text-gray-600 font-poppins">Aktif İlan</div>
            </div>
            <div>
              <div className="text-4xl md:text-5xl font-poppins font-black text-gray-900 mb-2">
                {formatStat(stats.members, 'members')}
              </div>
              <div className="text-base text-gray-600 font-poppins">Üyemiz</div>
            </div>
            <div>
              <div className="text-4xl md:text-5xl font-poppins font-black text-gray-900 mb-2">
                {formatStat(stats.satisfaction, 'satisfaction')}
              </div>
              <div className="text-base text-gray-600 font-poppins">Memnuniyet</div>
            </div>
          </div>
        </div>
      </section>

      {/* About Section - Clean Style */}
      <section id="hakkimizda" className="py-20" style={{ backgroundColor: '#fffcf1' }}>
        <div className="container mx-auto px-8">
          <div className="grid md:grid-cols-3 gap-12 items-start">
            {/* Left Text */}
            <div className="text-center space-y-8">
              <div>
                <div className="flex flex-col items-center mb-3">
                  <img src={icon1} alt="Icon" className="w-12 h-12 object-contain mb-3" />
                  <h2 className="text-2xl md:text-3xl text-gray-900 leading-tight" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
                    BİZ KİMİZ
                  </h2>
                </div>
                <p className="text-base text-gray-700 leading-relaxed">
                  Biz, sahipsiz hayvanların güvenli yuvalara kavuşmasını amaçlayan
                  ve bu süreci gönüllüler, barınaklar ve hayvanseverlerle birlikte yürüten bir sahiplendirme platformuyuz.
                </p>
              </div>

              <div>
                <div className="flex flex-col items-center mb-3">
                  <img src={icon2} alt="Icon" className="w-12 h-12 object-contain mb-3" />
                  <h2 className="text-2xl md:text-3xl text-gray-900 leading-tight" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
                    NE YAPIYORUZ
                  </h2>
                </div>
                <p className="text-base text-gray-700 leading-relaxed">
                  Barınaklar ve bireysel gönüllülerle iş birliği yaparak
                  sahiplenme sürecini şeffaf, güvenli ve erişilebilir hâle getiriyoruz.
                </p>
              </div>

              <div>
                <div className="flex flex-col items-center mb-3">
                  <img src={icon3} alt="Icon" className="w-12 h-12 object-contain mb-3" />
                  <h2 className="text-2xl md:text-3xl text-gray-900 leading-tight" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
                    NEDEN VARIZ
                  </h2>
                </div>
                <p className="text-base text-gray-700 leading-relaxed">
                  Her hayvanın bir hikâyesi ve ikinci bir şansı olduğuna inanıyoruz.
                  Doğru eşleşmelerle kalıcı bağlar kurulmasını hedefliyoruz.
                </p>
              </div>
            </div>

            {/* Center Logo */}
            <div className="flex items-center justify-center mt-16 md:mt-24">
              <div className="w-full max-w-md flex items-center justify-center p-8 aspect-square">
                <img 
                  src={logo} 
                  alt="PembePati Logo" 
                  className="w-full h-full object-contain"
                />
              </div>
            </div>

            {/* Right Text */}
            <div className="text-center space-y-8">
              <div>
                <div className="flex flex-col items-center mb-3">
                  <img src={icon4} alt="Icon" className="w-12 h-12 object-contain mb-3" />
                  <h2 className="text-2xl md:text-3xl text-gray-900 leading-tight" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
                    BAĞIŞLA DESTEK OL
                  </h2>
                </div>
                <p className="text-base text-gray-700 leading-relaxed">
                  Sahiplenemesen bile bir hayatı değiştirebilirsin.
                  Bağışlar; tedavi, beslenme ve barınma ihtiyaçları için doğrudan kullanılır.
                </p>
              </div>

              <div>
                <div className="flex flex-col items-center mb-3">
                  <img src={icon5} alt="Icon" className="w-12 h-12 object-contain mb-3" />
                  <h2 className="text-2xl md:text-3xl text-gray-900 leading-tight" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
                    SORUMLU VE ŞEFFAF
                  </h2>
                </div>
                <p className="text-base text-gray-700 leading-relaxed">
                  Toplanan bağışlar, iş birliği yapılan barınaklar ve gönüllüler aracılığıyla
                  ihtiyaç sahiplerine ulaştırılır ve süreci şeffaf şekilde paylaşırız.
                </p>
              </div>

              <div>
                <div className="flex flex-col items-center mb-3">
                  <img src={icon6} alt="Icon" className="w-12 h-12 object-contain mb-3" />
                  <h2 className="text-2xl md:text-3xl text-gray-900 leading-tight" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
                    BİRLİKTE DAHA FAZLASI
                  </h2>
                </div>
                <p className="text-base text-gray-700 leading-relaxed">
                  Bir sahiplenme bir hayatı değiştirir.
                  Bir bağış ise daha fazlasına umut olur.
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* How it works visual - full width, no cropping */}
      <div className="relative w-full -mt-5" style={{ backgroundColor: '#fffcf1' }}>
        <section className="relative w-full">
          {/* Background illustration */}
          <img
            src={howToUse}
            alt="PembePati'de süreç nasıl işler?"
            className="w-full h-auto block"
          />

          <img
            src={pati2}
            alt="Pati"
            className="hidden md:block pointer-events-none select-none absolute right-[-140px] top-[-80px] w-[400px] h-[400px] rotate-[-70deg]"
          />

        {/* Title overlay - top left */}
        <div className="absolute top-8 md:top-40 left-6 md:left-[600px] z-10">
          <h2 className="text-7xl md:text-7xl lg:text-[80px] text-gray-900 text-left tracking-wider" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
            PembePati&apos;de
            <br />
            <span className="block pl-8 md:pl-12 lg:pl-28">süreç nasıl işler?</span>
          </h2>
        </div>

        {/* Text overlay - adoption steps */}
        <div className="absolute inset-0 flex items-end justify-end px-6 md:px-24 pb-8 md:pb-28">
          <div className="max-w-5xl w-full">
            {/* 2x2 Grid Layout */}
            <div className="grid md:grid-cols-2 gap-8 md:gap-12">
              {/* Left Column */}
              <div className="space-y-8">
                {/* Step 1 */}
                <div className="flex items-start gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-12 h-12 rounded-full bg-[#FFA696] flex items-center justify-center text-black font-bold text-lg z-10">
                      1
                    </div>
                    <div className="w-0.5 h-20 bg-[#FFA696] mt-2"></div>
                  </div>
                  <div className="flex-1 pt-1">
                    <h3 className="text-xl md:text-2xl font-poppins font-bold text-white mb-2">
                      Profilini oluştur
                    </h3>
                    <p className="text-gray-900 font-poppins text-base md:text-lg">
                      Kısa bir kayıt formu ile kendini ve yaşam koşullarını tanıt.
                    </p>
                  </div>
                </div>

                {/* Step 2 */}
                <div className="flex items-start gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-12 h-12 rounded-full bg-[#FFA696] flex items-center justify-center text-black font-bold text-lg z-10">
                      2
                    </div>
                  </div>
                  <div className="flex-1 pt-1">
                    <h3 className="text-xl md:text-2xl font-poppins font-bold text-white mb-2">
                      İlanları incele
                    </h3>
                    <p className="text-gray-900 font-poppins text-base md:text-lg">
                      Şehrine, yaşam tarzına ve tercihlerine en uygun dostu filtreleyerek bul.
                    </p>
                  </div>
                </div>
              </div>

              {/* Right Column */}
              <div className="space-y-8">
                {/* Step 3 */}
                <div className="flex items-start gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-12 h-12 rounded-full bg-[#FFA696] flex items-center justify-center text-black font-bold text-lg z-10">
                      3
                    </div>
                    <div className="w-0.5 h-20 bg-[#FFA696] mt-2"></div>
                  </div>
                  <div className="flex-1 pt-1">
                    <h3 className="text-xl md:text-2xl font-poppins font-bold text-white mb-2">
                      Başvur ve tanış
                    </h3>
                    <p className="text-gray-900 font-poppins text-base md:text-lg">
                      Sahiplendirme formunu doldur, barınak veya ilan sahibiyle mesajlaşarak tanışma sürecini başlat.
                    </p>
                  </div>
                </div>

                {/* Step 4 */}
                <div className="flex items-start gap-4">
                  <div className="flex flex-col items-center">
                    <div className="w-12 h-12 rounded-full bg-[#FFA696] flex items-center justify-center text-black font-bold text-lg z-10">
                      4
                    </div>
                  </div>
                  <div className="flex-1 pt-1">
                    <h3 className="text-xl md:text-2xl font-poppins font-bold text-white mb-2">
                      Evcil hayvanına kavuş
                    </h3>
                    <p className="text-gray-900 font-poppins text-base md:text-lg">
                      Uygunluk onayı sonrası sözleşme imzalanır ve yeni yol arkadaşınla güvenle aynı yuvayı paylaşırsın.
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        </section>
      </div>

      {/* Featured Listings - Dark Section */}
      <section className="py-12 bg-gray-950 w-full">
        <div className="w-full">
          {/* Title */}
          <div className="px-8 max-w-5xl mx-auto mb-8">
            <h2 className="text-5xl md:text-5xl text-white uppercase text-center" style={{ fontFamily: 'Bebas Neue, sans-serif' }}>
              Yuva Bekleyen Dostlar
            </h2>
          </div>

          {/* Auto-scrolling Container */}
          {featuredListings.length > 0 && (
            <div className="overflow-hidden pb-6 w-full">
              <div 
                className="flex gap-6"
                style={{
                  animation: 'scroll 120s linear infinite',
                  width: 'max-content'
                }}
              >
                {/* First set of cards */}
                {featuredListings.map((listing) => (
                  <Link key={listing.id} to={`/listings/${listing.id}`}>
                    <div className="flex-shrink-0 w-80 bg-gray-200 rounded-lg overflow-hidden border border-gray-300 hover:border-gray-400 transition-colors cursor-pointer flex shadow-lg">
                      {/* Photo on the left */}
                      <div className="w-32 h-full flex-shrink-0 bg-gray-100 flex items-center justify-center overflow-hidden">
                        {listing.photoUrls && listing.photoUrls.length > 0 ? (
                          <img
                            src={listing.photoUrls[0]}
                            alt={listing.title}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <span className="text-2xl">{listing.species === 'Kedi' ? '🐱' : listing.species === 'Köpek' ? '🐕' : '🐾'}</span>
                        )}
                      </div>
                      {/* Details on the right */}
                      <div className="flex-1 p-4 flex flex-col justify-center min-w-0">
                        <h3 className="text-base font-poppins font-bold text-gray-900 mb-1 line-clamp-1">{listing.title}</h3>
                        <p className="text-gray-600 font-poppins text-xs mb-2">
                          {listing.breed || listing.species || ''}
                          {listing.age && ` • ${listing.age} ay`}
                          {listing.gender && ` • ${listing.gender}`}
                        </p>
                        <p className="text-gray-700 font-poppins text-xs line-clamp-2">
                          {listing.description}
                        </p>
                      </div>
                    </div>
                  </Link>
                ))}

                {/* Duplicate set for seamless loop */}
                {featuredListings.map((listing) => (
                  <Link key={`duplicate-${listing.id}`} to={`/listings/${listing.id}`}>
                    <div className="flex-shrink-0 w-80 bg-gray-200 rounded-lg overflow-hidden border border-gray-300 hover:border-gray-400 transition-colors cursor-pointer flex shadow-lg">
                      {/* Photo on the left */}
                      <div className="w-32 h-full flex-shrink-0 bg-gray-100 flex items-center justify-center overflow-hidden">
                        {listing.photoUrls && listing.photoUrls.length > 0 ? (
                          <img
                            src={listing.photoUrls[0]}
                            alt={listing.title}
                            className="w-full h-full object-cover"
                          />
                        ) : (
                          <span className="text-2xl">{listing.species === 'Kedi' ? '🐱' : listing.species === 'Köpek' ? '🐕' : '🐾'}</span>
                        )}
                      </div>
                      {/* Details on the right */}
                      <div className="flex-1 p-4 flex flex-col justify-center min-w-0">
                        <h3 className="text-base font-poppins font-bold text-gray-900 mb-1 line-clamp-1">{listing.title}</h3>
                        <p className="text-gray-600 font-poppins text-xs mb-2">
                          {listing.breed || listing.species || ''}
                          {listing.age && ` • ${listing.age} ay`}
                          {listing.gender && ` • ${listing.gender}`}
                        </p>
                        <p className="text-gray-700 font-poppins text-xs line-clamp-2">
                          {listing.description}
                        </p>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            </div>
          )}

          {/* Add CSS animation */}
          <style>{`
            @keyframes scroll {
              0% {
                transform: translateX(0);
              }
              100% {
                transform: translateX(-50%);
              }
            }
          `}</style>

          {/* CTA Button */}
          <div className="text-center mt-8">
            <Link to="/listings">
              <button className="bg-white text-gray-900 px-6 py-2.5 font-poppins font-bold text-base hover:bg-gray-100 transition-colors  hover:scale-105 transition-transform text-lg shadow-xl">
                Daha Fazlasını Keşfet
              </button>
            </Link>
          </div>
        </div>
      </section>
      
    </div>
  );
}

