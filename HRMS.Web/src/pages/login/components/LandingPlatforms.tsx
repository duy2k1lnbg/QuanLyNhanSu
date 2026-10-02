import { GlobalOutlined, MobileOutlined, WindowsOutlined, ArrowRightOutlined } from '@ant-design/icons';
import { ScrollReveal } from '../../../components/ScrollReveal';
import type { LocaleType } from '../../../locales/vi';
import '../LandingPlatforms.css';

interface LandingPlatformsProps {
  tLanding: LocaleType['landing'];
  onOpenLogin: () => void;
  onOpenDownload: (type?: 'windows' | 'mobile' | 'general') => void;
}

export function LandingPlatforms({ tLanding, onOpenLogin, onOpenDownload }: LandingPlatformsProps) {
  const platforms = [
    { id: 'web', icon: <GlobalOutlined />, title: tLanding.platformWebTitle,
      description: tLanding.platformWebDesc, action: tLanding.platformWebAction,
      position: '-100%', onClick: onOpenLogin },
    { id: 'mobile', icon: <MobileOutlined />, title: tLanding.platformMobileTitle,
      description: tLanding.platformMobileDesc, action: tLanding.platformMobileAction,
      position: '-200%', onClick: () => onOpenDownload('mobile') },
    { id: 'desktop', icon: <WindowsOutlined />, title: tLanding.platformDesktopTitle,
      description: tLanding.platformDesktopDesc, action: tLanding.platformDesktopAction,
      position: '0%', onClick: () => onOpenDownload('windows') },
  ];
  return (
    <section id="platforms" className="landing-section-platforms" aria-labelledby="platforms-title">
      <div className="platforms-container">
        <ScrollReveal direction="up" duration={1500} delay={200}>
          <div className="platforms-heading">
            <h2 id="platforms-title">{tLanding.platformsTitle}</h2>
            <p>{tLanding.platformsSubtitle}</p>
          </div>
        </ScrollReveal>
        <div className="platforms-grid">
          {platforms.map((platform, index) => (
            <ScrollReveal key={platform.id} direction="up" duration={1500} delay={250 + index * 250}>
              <article className={'platform-card platform-card-' + platform.id}>
                <div className="platform-image">
                  <img src="/images/landing/platforms-forest.png" alt={platform.title}
                    loading="lazy" decoding="async" width={2164} height={727}
                    style={{ left: platform.position }} />
                </div>
                <div className="platform-card-content">
                  <span className="platform-card-icon" aria-hidden="true">{platform.icon}</span>
                  <h3>{platform.title}</h3>
                  <p>{platform.description}</p>
                  <button type="button" onClick={platform.onClick}>
                    <span>{platform.action}</span><ArrowRightOutlined />
                  </button>
                </div>
              </article>
            </ScrollReveal>
          ))}
        </div>
      </div>
    </section>
  );
}
