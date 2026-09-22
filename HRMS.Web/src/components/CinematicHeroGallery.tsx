import React, { useState, useEffect, useCallback } from 'react';
import { LeftOutlined, RightOutlined } from '@ant-design/icons';
import './CinematicHeroGallery.css';

export interface CinematicSlide {
  id: string;
  tag: string;
  imageSrc: string;
  altText: string;
  quote: string;
  quoteSub?: string;
  author?: string;
  whyNotYou?: string;
  isJapanese?: boolean;
}

// 10 CÂU TRUYỀN CẢM HỨNG GIỮ NGUYÊN 100% NGUYÊN BẢN, KHÔNG DỊCH
const CINEMATIC_SLIDES: CinematicSlide[] = [
  {
    id: 'hero-01',
    tag: 'COMEBACK',
    imageSrc: '/images/inspirational/hero-01.jpg',
    altText: 'Đường đua vắng lặng lúc bình minh',
    quote: 'Dear ông trời, con là đứa trẻ ưu tú năm ấy. Xin hãy mang con trở lại đường đua!!!'.normalize('NFC'),
  },
  {
    id: 'hero-02',
    tag: 'START AGAIN',
    imageSrc: '/images/inspirational/hero-02.jpg',
    altText: 'Bàn học đêm khuya với laptop và đèn bàn',
    quote: 'Nó không khó, nó chỉ mới thôi. Cái gì không làm được, thì vừa khóc vừa làm.'.normalize('NFC'),
  },
  {
    id: 'hero-03',
    tag: 'SELF ACCEPTANCE',
    imageSrc: '/images/inspirational/hero-03.jpg',
    altText: 'Căn phòng tĩnh lặng với ánh sáng ban mai xuyên qua cửa sổ',
    quote: 'Đừng ghét bản thân vì những ngày chưa giỏi giang.'.normalize('NFC'),
  },
  {
    id: 'hero-04',
    tag: 'FORGIVE THE PAST',
    imageSrc: '/images/inspirational/hero-04.jpg',
    altText: 'Con đường vắng chìm trong sương mù dày đặc',
    quote: 'Đừng tự trách bản thân trong quá khứ nữa, khi đó cậu ấy đứng một mình trong sương cũng rất hoang mang mờ mịt.'.normalize('NFC'),
  },
  {
    id: 'hero-05',
    tag: 'PATIENCE',
    imageSrc: '/images/inspirational/hero-05.jpg',
    altText: 'Nụ hoa e ấp trong sương sớm',
    quote: 'Hãy kiên nhẫn với chính mình, hoa nào cũng cần thời gian để nở...'.normalize('NFC'),
  },
  {
    id: 'hero-06',
    tag: 'THE CHOSEN PATH',
    imageSrc: '/images/inspirational/hero-06.jpg',
    altText: 'Con đường núi gồ ghề hiểm trở kéo dài vô tận về phía chân trời',
    quote: 'Con đường mình chọn có quỳ cũng phải đi cho hết.'.normalize('NFC'),
  },
  {
    id: 'hero-07',
    tag: 'DECISION',
    imageSrc: '/images/inspirational/hero-07.jpg',
    altText: 'Đường ray chia đôi hướng trước thành phố về đêm',
    quote: '始めようぜ。この間の続きをよ。また負ける？俺はまだ負けてねえぞ。覚えとけよ、俺が負ける時は、俺が死ぬ時だぜ。何度も立ち上がれ。勝利は忍耐する者に訪れる。',
    isJapanese: true,
  },
  {
    id: 'hero-08',
    tag: 'MOVE FORWARD',
    imageSrc: '/images/inspirational/hero-08.jpg',
    altText: 'Cây cầu cao tốc kéo dài trong mưa bão',
    quote: '弱い！未熟！そんなものは男ではない！進め！男なら！男に生まれたなら、進む以外の道などない！',
    isJapanese: true,
  },
  {
    id: 'hero-09',
    tag: 'WHY NOT YOU?',
    imageSrc: '/images/inspirational/hero-09.jpg',
    altText: 'Sân thượng nhìn ra chân trời thành phố lúc bình minh rạng rỡ',
    quote: 'But I want you to take it personal, and my personal question to you is: Why not you?',
    quoteSub: "You've got the brains, you can make decisions, you can study the plan, you can change your life, you can make your dreams come true. Why not you?",
    whyNotYou: 'Why not you?',
  },
  {
    id: 'hero-10',
    tag: 'TRY HARD AGAIN',
    imageSrc: '/images/inspirational/hero-10.jpg',
    altText: 'Con đường uốn lượn qua thung lũng sương mù hướng về phía bình minh rạng rỡ',
    quote: 'Ever tried. Ever failed. No matter. Try again. Fail again. Fail better. The world is yours. Treat everyone kindly and light up the night.',
    author: '— Peter Dinklage',
  },
];

// Tốc độ gõ từng chữ chậm rãi, có nhịp điệu và cảm xúc điện ảnh (~65ms/ký tự)
const TYPING_SPEED_MS = 65;

// Thời gian chuyển ảnh tính từ lúc hiển thị hết chữ: đúng sau 3s (3000ms)
const POST_TYPING_WAIT_MS = 3000;

export const CinematicHeroGallery: React.FC = () => {
  const [activeIndex, setActiveIndex] = useState<number>(0);
  const [isPaused, setIsPaused] = useState<boolean>(false);
  const [touchStartX, setTouchStartX] = useState<number | null>(null);

  // Typewriter streaming text state
  const [displayedQuote, setDisplayedQuote] = useState<string>('');
  const [isTypingComplete, setIsTypingComplete] = useState<boolean>(false);

  const totalSlides = CINEMATIC_SLIDES.length;

  const handleNext = useCallback(() => {
    setActiveIndex((prev) => (prev + 1) % totalSlides);
  }, [totalSlides]);

  const handlePrev = useCallback(() => {
    setActiveIndex((prev) => (prev - 1 + totalSlides) % totalSlides);
  }, [totalSlides]);

  const handleSelectSlide = (index: number) => {
    setActiveIndex(index);
  };

  // 1. Hiệu ứng chữ chạy ra theo thời gian khi đổi frame (chạy từng chữ chậm rãi, có cảm xúc)
  useEffect(() => {
    setDisplayedQuote('');
    setIsTypingComplete(false);

    const fullText = CINEMATIC_SLIDES[activeIndex].quote;
    let charIdx = 0;

    const typingTimer = setInterval(() => {
      charIdx++;
      setDisplayedQuote(fullText.slice(0, charIdx));

      if (charIdx >= fullText.length) {
        clearInterval(typingTimer);
        setIsTypingComplete(true);
      }
    }, TYPING_SPEED_MS);

    return () => clearInterval(typingTimer);
  }, [activeIndex]);

  // 2. Tự động chuyển frame: TÍNH TỪ LÚC HIỂN THỊ HẾT CHỮ SAU ĐÚNG 3 GIÂY
  useEffect(() => {
    if (!isTypingComplete || isPaused) return;

    const autoNextTimer = setTimeout(() => {
      handleNext();
    }, POST_TYPING_WAIT_MS);

    return () => clearTimeout(autoNextTimer);
  }, [isTypingComplete, isPaused, handleNext]);

  // Touch Swipe Handlers for mobile
  const handleTouchStart = (e: React.TouchEvent) => {
    setTouchStartX(e.touches[0].clientX);
    setIsPaused(true);
  };

  const handleTouchEnd = (e: React.TouchEvent) => {
    if (touchStartX === null) return;
    const touchEndX = e.changedTouches[0].clientX;
    const diff = touchStartX - touchEndX;
    if (Math.abs(diff) > 40) {
      if (diff > 0) {
        handleNext();
      } else {
        handlePrev();
      }
    }
    setTouchStartX(null);
    setIsPaused(false);
  };

  // Keyboard navigation
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowRight') {
      handleNext();
    } else if (e.key === 'ArrowLeft') {
      handlePrev();
    }
  };

  const activeSlide = CINEMATIC_SLIDES[activeIndex];

  // Tính toán vị trí offset 3D Coverflow (-3, -2, -1, 0, 1, 2, 3)
  const getCardOffset = (idx: number) => {
    let diff = idx - activeIndex;
    if (diff > totalSlides / 2) diff -= totalSlides;
    if (diff < -totalSlides / 2) diff += totalSlides;
    return diff;
  };

  return (
    <section
      className="cinematic-hero-root-v2"
      aria-label="Cinematic Hero Gallery"
      onMouseEnter={() => setIsPaused(true)}
      onMouseLeave={() => setIsPaused(false)}
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      {/* 1. BACKGROUND FULL-BLEED IMAGES (Crossfade & subtle scale) */}
      <div className="v2-bg-container">
        {CINEMATIC_SLIDES.map((slide, idx) => {
          const isActive = idx === activeIndex;
          return (
            <div
              key={slide.id}
              className={`v2-bg-item ${isActive ? 'is-active' : ''}`}
              style={{
                backgroundImage: `url(${slide.imageSrc})`,
              }}
              aria-hidden={!isActive}
            />
          );
        })}
      </div>

      {/* 2. CINEMATIC GRADIENT VIGNETTE */}
      <div className="v2-vignette-overlay" />

      {/* 3. CHỮ CHO LÊN GÓC TRÊN BÊN TRÁI (Top-Left Editorial Quote Header) */}
      <div className="v2-quote-top-left" key={`quote-${activeIndex}`}>
        <div className="v2-tag-pill">
          <span className="tag-dot" />
          <span className="tag-text">{activeSlide.tag}</span>
        </div>

        <div className="v2-quote-body">
          <blockquote
            className={`v2-main-quote ${activeSlide.isJapanese ? 'is-japanese' : ''}`}
          >
            "{displayedQuote}"
            {!isTypingComplete && <span className="typewriter-cursor">|</span>}
          </blockquote>

          {activeSlide.quoteSub && (
            <p className={`v2-sub-quote ${isTypingComplete ? 'is-revealed' : ''}`}>
              "{activeSlide.quoteSub}"
            </p>
          )}

          {activeSlide.author && (
            <div
              className={`v2-quote-author ${isTypingComplete ? 'is-revealed' : ''}`}
              style={{
                fontFamily: "'Cinzel', 'Plus Jakarta Sans', -apple-system, sans-serif",
                fontSize: 'clamp(14px, 1.4vw, 18px)',
                fontWeight: 700,
                color: '#93c5fd',
                letterSpacing: '1px',
                marginTop: 4,
                textShadow: '0 2px 10px rgba(0, 0, 0, 0.9)',
                opacity: isTypingComplete ? 1 : 0,
                transform: isTypingComplete ? 'translateY(0)' : 'translateY(6px)',
                transition: 'opacity 0.5s ease, transform 0.5s ease',
              }}
            >
              {activeSlide.author}
            </div>
          )}

          {activeSlide.whyNotYou && (
            <div
              className={`v2-why-not-you-highlight ${isTypingComplete ? 'is-revealed' : ''}`}
              aria-label="Why not you?"
            >
              {activeSlide.whyNotYou}
            </div>
          )}
        </div>
      </div>

      {/* 4. CHO RA GÓC DƯỚI BÊN TRÁI & HƠI NHỎ LẠI (Bottom-Left Compact 3D Coverflow) */}
      <div className="v2-coverflow-bottom-left">
        {/* Stage 3D Coverflow Cards */}
        <div className="v2-stage-3d">
          <div className="v2-cards-track">
            {CINEMATIC_SLIDES.map((slide, idx) => {
              const offset = getCardOffset(idx);
              const isCenter = offset === 0;
              const isVisible = Math.abs(offset) <= 2;

              return (
                <div
                  key={slide.id}
                  className={`v2-coverflow-card ${isCenter ? 'is-center' : ''} ${!isVisible ? 'is-hidden' : ''}`}
                  style={{
                    '--offset': offset,
                  } as React.CSSProperties}
                  onClick={() => handleSelectSlide(idx)}
                  role="button"
                  tabIndex={0}
                  title={`Chuyển đến: ${slide.tag}`}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      handleSelectSlide(idx);
                    }
                  }}
                >
                  <img
                    src={slide.imageSrc}
                    alt={slide.altText}
                    className="v2-card-img"
                    draggable={false}
                    loading={Math.abs(offset) <= 1 ? 'eager' : 'lazy'}
                  />

                  {/* Card overlay */}
                  <div className="v2-card-overlay" />

                  {/* Frosted Glass Pill Tag on Card (Không bị đè lấp) */}
                  <div className="v2-card-pill">
                    <span>{slide.tag}</span>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* 5. CỤM ĐIỀU KHIỂN Ở DƯỚI HẲN THẺ ẢNH (Dạng giống ảnh 3, tuyệt đối không bị đè hay lấp chữ) */}
        <div className="v2-bottom-nav-bar">
          <button
            type="button"
            className="v2-nav-arrow-btn prev-btn"
            onClick={handlePrev}
            aria-label="Khung trước"
            title="Khung trước (←)"
          >
            <LeftOutlined />
          </button>

          {/* 9 Pagination Dots dạng giống ảnh 3: Active dot là viên thuốc dài thanh mảnh */}
          <div className="v2-dots-track" aria-label="Slide Dots Navigation">
            {CINEMATIC_SLIDES.map((slide, dotIdx) => (
              <button
                key={dotIdx}
                type="button"
                className={`v2-dot-item ${dotIdx === activeIndex ? 'is-active' : ''}`}
                onClick={() => handleSelectSlide(dotIdx)}
                aria-label={`Slide ${dotIdx + 1}`}
                title={`Slide ${dotIdx + 1}: ${slide.tag}`}
              />
            ))}
          </div>

          <button
            type="button"
            className="v2-nav-arrow-btn next-btn"
            onClick={handleNext}
            aria-label="Khung tiếp theo"
            title="Khung tiếp theo (→)"
          >
            <RightOutlined />
          </button>

          {/* Slide Counter */}
          <div className="v2-slide-counter">
            <span className="current">{String(activeIndex + 1).padStart(2, '0')}</span>
            <span className="sep">/</span>
            <span className="total">{String(totalSlides).padStart(2, '0')}</span>
          </div>
        </div>
      </div>
    </section>
  );
};

export default CinematicHeroGallery;
