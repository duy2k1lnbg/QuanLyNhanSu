import React from 'react';
import {
  LeftOutlined,
  RightOutlined,
  PauseOutlined,
  CaretRightOutlined,
} from '@ant-design/icons';
import {
  CINEMATIC_SLIDES,
  DEFAULT_SLIDE_INDEX,
} from '../pages/login/data/cinematicSlides';
import { useHeroCarousel } from '../pages/login/hooks/useHeroCarousel';
import { useCarouselDrag } from '../pages/login/hooks/useCarouselDrag';
import { SlideAtmosphere } from '../pages/login/components/SlideAtmosphere';
import { useAppLanguage } from '../services/i18n';
import './CinematicHeroGallery.css';

interface CinematicHeroGalleryProps {
  isModalOpen?: boolean;
}

export const CinematicHeroGallery: React.FC<CinematicHeroGalleryProps> = ({
  isModalOpen = false,
}) => {
  const {
    currentIndex,
    currentSlide,
    goToSlide,
    nextSlide,
    prevSlide,
    isPlaying,
    togglePlayPause,
    addPauseReason,
    removePauseReason,
    progressBarRef,
    heroRef,
    typedQuote,
    typedSubquote,
    isTypingComplete,
    isIntroPending,
    isAtmospherePaused,
  } = useHeroCarousel({
    slides: CINEMATIC_SLIDES,
    initialIndex: DEFAULT_SLIDE_INDEX,
    isModalOpen,
  });

  const { dragOffset, isDragging, dragProps } = useCarouselDrag({
    onNext: () => nextSlide(true),
    onPrev: () => prevSlide(true),
    onDragStart: () => addPauseReason('drag'),
    onDragEnd: () => removePauseReason('drag'),
  });

  const { tLanding } = useAppLanguage();

  // Calculate 5 visible thumbnails around currentIndex (looping)
  const total = CINEMATIC_SLIDES.length;
  const visibleThumbnailIndices = [-2, -1, 0, 1, 2].map((offset) => {
    return (currentIndex + offset + total) % total;
  });

  // Slide counter format (e.g. "05 / 10")
  const counterText = `${String(currentIndex + 1).padStart(2, '0')} / ${String(total).padStart(2, '0')}`;

  // Keyboard navigation when hero gallery has focus
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowLeft') {
      e.preventDefault();
      prevSlide(true);
    } else if (e.key === 'ArrowRight') {
      e.preventDefault();
      nextSlide(true);
    } else if ((e.key === ' ' || e.key === 'Spacebar') && e.target === e.currentTarget) {
      e.preventDefault();
      togglePlayPause();
    }
  };

  return (
    <section
      id="cinematic-hero-gallery"
      ref={heroRef}
      className="tryhard-hero-root"
      data-intro-pending={isIntroPending}
      data-atmosphere-paused={isAtmospherePaused}
      data-typing-complete={isTypingComplete}
      tabIndex={0}
      onKeyDown={handleKeyDown}
      onMouseEnter={() => addPauseReason('hover')}
      onMouseLeave={() => removePauseReason('hover')}
      onFocus={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget as Node | null)) addPauseReason('focus');
      }}
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget as Node | null)) removePauseReason('focus');
      }}
      aria-label={tLanding.carouselLabel}
      style={
        {
          '--slide-accent': currentSlide.accent,
          '--slide-accent-strong': currentSlide.accentStrong,
        } as React.CSSProperties
      }
    >
      {/* 1. BACKGROUND IMAGES STACK (DRAG + GENTLE CROSSFADE) */}
      <div
        className="hero-background-viewport"
        {...dragProps}
        style={{
          ...dragProps.style,
          transform: isDragging ? `translateX(${dragOffset}px)` : 'none',
          transition: isDragging ? 'none' : 'transform 0.4s cubic-bezier(0.2, 0.8, 0.2, 1)',
        }}
      >
        {CINEMATIC_SLIDES.map((slide, idx) => {
          // Render current and adjacent slides to keep DOM light and smooth
          const isCurrent = idx === currentIndex;
          const isAdjacent =
            idx === (currentIndex - 1 + total) % total ||
            idx === (currentIndex + 1) % total;

          if (!isCurrent && !isAdjacent) return null;

          return (
            <div
              key={slide.id}
              className={`hero-bg-slide ${isCurrent ? 'is-active' : ''}`}
              style={{
                backgroundImage: `url(${slide.imageSrc})`,
                backgroundPosition: slide.focalPoint || 'center center',
              }}
              role="img"
              aria-label={slide.altText}
            />
          );
        })}
      </div>

      {/* 2. GRADIENT OVERLAY FOR TEXT READABILITY & FOREST ATMOSPHERE */}
      <div className="hero-gradient-scrim" aria-hidden="true" />

      {/* 3. SINGLE ATMOSPHERE EFFECT (DAWN, SUN-DUST, MIST, RAIN, LAMP) */}
      <SlideAtmosphere
        environment={currentSlide.environment}
        accent={currentSlide.accent}
      />

      {/* 4. MAIN HERO EDITORIAL CONTENT (LEFT-ALIGNED) */}
      <div className="hero-content-container">
        <div className="hero-quote-block">
          {/* TAG LABEL + FINE HORIZONTAL LINE */}
          <div className="hero-tag-row">
            <span className="hero-tag-text">{currentSlide.tag}</span>
            <span className="hero-tag-line" aria-hidden="true" />
          </div>

          {/* HEADLINE QUOTE (SERIF CREAM, FULL PHRASE, GENTLE FADE) */}
          <blockquote
            key={currentSlide.id}
            className={`hero-quote-text ${currentSlide.isJapanese ? 'is-japanese' : ''}`}
          >
            <span className="hero-text-reserve" aria-hidden="true">{currentSlide.quote}</span>
            <span className="hero-text-typed" aria-hidden="true">
              {typedQuote}
              {!isTypingComplete && !currentSlide.quoteSub && <span className="hero-typewriter-cursor" />}
            </span>
            <span className="hero-sr-only">{currentSlide.quote}</span>
          </blockquote>

          {/* OPTIONAL SUBQUOTE OR AUTHOR */}
          {currentSlide.quoteSub && (
            <p className="hero-quote-sub hero-typewriter-sub">
              <span className="hero-text-reserve" aria-hidden="true">{currentSlide.quoteSub}</span>
              <span className="hero-text-typed" aria-hidden="true">{typedSubquote}</span>
              <span className="hero-sr-only">{currentSlide.quoteSub}</span>
            </p>
          )}
          {currentSlide.author && (
            <div className={`hero-quote-author ${isTypingComplete ? "is-revealed" : ""}`}>{currentSlide.author}</div>
          )}

          {/* BRAND SIGNATURE SUBLINE */}
          <div className="hero-brand-subline">
            {tLanding.heroSubline}
          </div>
        </div>

        {/* 5. HORIZONTAL 5-THUMBNAIL STRIP WITH CONTROLS (MATCHING TARGET DESIGN) */}
        <div className="hero-controls-block">
          <div className="hero-thumbnails-row">
            {/* PREV BUTTON */}
            <button
              type="button"
              onClick={() => prevSlide(true)}
              className="carousel-arrow-btn prev-btn"
              aria-label={tLanding.carouselPrevious}
            >
              <LeftOutlined style={{ fontSize: 13 }} />
            </button>

            {/* 5 THUMBNAIL CARDS */}
            <div className="thumbnails-track">
              {visibleThumbnailIndices.map((idx) => {
                const item = CINEMATIC_SLIDES[idx];
                const isActive = idx === currentIndex;

                return (
                  <button
                    key={`${item.id}-${idx}`}
                    type="button"
                    onClick={() => goToSlide(idx, true)}
                    className={`thumbnail-card ${isActive ? 'is-active' : ''}`}
                    aria-label={tLanding.carouselSelect + ': ' + item.tag}
                    aria-current={isActive ? 'true' : undefined}
                  >
                    <img
                      src={item.thumbnailSrc || item.imageSrc}
                      alt={item.altText}
                      loading="lazy"
                      className="thumbnail-img"
                    />
                  </button>
                );
              })}
            </div>

            {/* NEXT BUTTON */}
            <button
              type="button"
              onClick={() => nextSlide(true)}
              className="carousel-arrow-btn next-btn"
              aria-label={tLanding.carouselNext}
            >
              <RightOutlined style={{ fontSize: 13 }} />
            </button>
          </div>

          {/* PAGINATION DOTS + COUNTER + TIMELINE */}
          <div className="hero-meta-row">
            {/* 10 DOTS */}
            <div className="hero-dots-wrap" aria-label={tLanding.carouselSelect}>
              {CINEMATIC_SLIDES.map((_, i) => (
                <button
                  type="button"
                  aria-label={CINEMATIC_SLIDES[i].tag}
                  aria-current={i === currentIndex ? 'true' : undefined}
                  key={i}
                  onClick={() => goToSlide(i, true)}
                  className={`hero-dot ${i === currentIndex ? 'is-active' : ''}`}
                />
              ))}
            </div>

            {/* SLIDE COUNTER (e.g. "05 / 10") */}
            <div className="hero-counter-label" aria-live="polite">
              {counterText}
            </div>

            {/* PLAY / PAUSE BUTTON */}
            <button
              type="button"
              onClick={togglePlayPause}
              className="hero-play-pause-btn"
              aria-label={isPlaying ? tLanding.carouselPause : tLanding.carouselPlay}
              title={isPlaying ? tLanding.carouselPause : tLanding.carouselPlay}
            >
              {isPlaying ? (
                <PauseOutlined style={{ fontSize: 12 }} />
              ) : (
                <CaretRightOutlined style={{ fontSize: 13, color: '#DCC58E' }} />
              )}
              <span className="play-pause-text">
                {isPlaying ? tLanding.carouselPause : tLanding.carouselPlay}
              </span>
            </button>
          </div>

          {/* 2PX SLIM PROGRESS TIMELINE */}
          <div className="hero-progress-track" aria-hidden="true">
            <div ref={progressBarRef} className="hero-progress-fill" />
          </div>
        </div>
      </div>
    </section>
  );
};
