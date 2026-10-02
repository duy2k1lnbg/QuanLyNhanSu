export type SlideEnvironment =
  | 'dawn'
  | 'lamp'
  | 'sun-dust'
  | 'mist'
  | 'clouds'
  | 'rain'
  | 'horizon';

export interface CinematicSlideData {
  id: string;
  index: number;
  tag: string;
  imageSrc: string;
  thumbnailSrc?: string;
  altText: string;
  quote: string;
  quoteSub?: string;
  author?: string;
  isJapanese?: boolean;
  environment: SlideEnvironment;
  accent: string;
  accentStrong: string;
  focalPoint?: string;
  durationMs: number;
  intensity?: number;
}

export const CINEMATIC_SLIDES: CinematicSlideData[] = [
  {
    id: 'hero-01',
    index: 0,
    tag: 'COMEBACK',
    imageSrc: '/images/inspirational/hero-01.jpg',
    thumbnailSrc: '/images/inspirational/hero-01.jpg',
    altText: 'Đường đua vắng lặng lúc bình minh',
    quote: 'Dear ông trời, con là đứa trẻ ưu tú năm ấy. Xin hãy mang con trở lại đường đua!!!'.normalize('NFC'),
    environment: 'dawn',
    accent: '#CCA776',
    accentStrong: '#E2BD8A',
    focalPoint: '70% 50%',
    durationMs: 8000,
  },
  {
    id: 'hero-02',
    index: 1,
    tag: 'START AGAIN',
    imageSrc: '/images/inspirational/hero-02.jpg',
    thumbnailSrc: '/images/inspirational/hero-02.jpg',
    altText: 'Bàn học đêm khuya với laptop và đèn bàn',
    quote: 'Nó không khó, nó chỉ mới thôi. Cái gì không làm được, thì vừa khóc vừa làm.'.normalize('NFC'),
    environment: 'lamp',
    accent: '#CBA36A',
    accentStrong: '#E5BA7D',
    focalPoint: '65% 50%',
    durationMs: 8000,
  },
  {
    id: 'hero-03',
    index: 2,
    tag: 'SELF ACCEPTANCE',
    imageSrc: '/images/inspirational/hero-03.jpg',
    thumbnailSrc: '/images/inspirational/hero-03.jpg',
    altText: 'Căn phòng tĩnh lặng với ánh sáng ban mai xuyên qua cửa sổ',
    quote: 'Đừng ghét bản thân vì những ngày chưa giỏi giang.'.normalize('NFC'),
    environment: 'sun-dust',
    accent: '#D5C19A',
    accentStrong: '#EBD4AA',
    focalPoint: '75% 45%',
    durationMs: 8000,
  },
  {
    id: 'hero-04',
    index: 3,
    tag: 'FORGIVE THE PAST',
    imageSrc: '/images/inspirational/hero-04.jpg',
    thumbnailSrc: '/images/inspirational/hero-04.jpg',
    altText: 'Con đường vắng chìm trong sương mù dày đặc',
    quote: 'Đừng tự trách bản thân trong quá khứ nữa, khi đó cậu ấy đứng một mình trong sương cũng rất hoang mang mờ mịt.'.normalize('NFC'),
    environment: 'mist',
    accent: '#AAC0B0',
    accentStrong: '#C1D9C8',
    focalPoint: '60% 50%',
    durationMs: 10000,
  },
  {
    id: 'hero-05',
    index: 4,
    tag: 'PATIENCE',
    imageSrc: '/images/inspirational/hero-05.jpg',
    thumbnailSrc: '/images/inspirational/hero-05.jpg',
    altText: 'Bông hoa hồng nở rộ giữa rừng xanh ngập tràn ánh nắng ấm',
    quote: 'Hãy kiên nhẫn với chính mình, hoa nào cũng cần thời gian để nở.'.normalize('NFC'),
    environment: 'sun-dust',
    accent: '#DCC58E',
    accentStrong: '#F4DD9E',
    focalPoint: '78% 55%',
    durationMs: 8000,
  },
  {
    id: 'hero-06',
    index: 5,
    tag: 'THE CHOSEN PATH',
    imageSrc: '/images/inspirational/hero-06.jpg',
    thumbnailSrc: '/images/inspirational/hero-06.jpg',
    altText: 'Con đường núi gồ ghề hiểm trở kéo dài vô tận về phía chân trời',
    quote: 'Con đường mình chọn có quỳ cũng phải đi cho hết.'.normalize('NFC'),
    environment: 'clouds',
    accent: '#AFB793',
    accentStrong: '#C7D1A7',
    focalPoint: '70% 50%',
    durationMs: 8000,
  },
  {
    id: 'hero-07',
    index: 6,
    tag: 'DECISION',
    imageSrc: '/images/inspirational/hero-07.jpg',
    thumbnailSrc: '/images/inspirational/hero-07.jpg',
    altText: 'Đường ray chia đôi hướng trước thành phố về đêm',
    quote: '始めようぜ。この間の続きをよ。また負ける？俺はまだ負けてねえぞ。覚えとけよ、俺が負ける時は、俺が死ぬ時だぜ。何度も立ち上がれ。勝利は忍耐する者に訪れる。',
    isJapanese: true,
    environment: 'lamp',
    accent: '#C3AE86',
    accentStrong: '#DEC699',
    focalPoint: '65% 55%',
    durationMs: 12000,
  },
  {
    id: 'hero-08',
    index: 7,
    tag: 'MOVE FORWARD',
    imageSrc: '/images/inspirational/hero-08.jpg',
    thumbnailSrc: '/images/inspirational/hero-08.jpg',
    altText: 'Cây cầu cao tốc kéo dài trong mưa bão',
    quote: '弱い！未熟！そんなものは男ではない！進め！男なら！男に生まれたなら、進む以外の道などない！',
    isJapanese: true,
    environment: 'rain',
    accent: '#A8BAC2',
    accentStrong: '#C0D4DC',
    focalPoint: '60% 50%',
    durationMs: 10000,
  },
  {
    id: 'hero-09',
    index: 8,
    tag: 'WHY NOT YOU?',
    imageSrc: '/images/inspirational/hero-09.jpg',
    thumbnailSrc: '/images/inspirational/hero-09.jpg',
    altText: 'Sân thượng nhìn ra chân trời thành phố lúc bình minh rạng rỡ',
    quote: 'But I want you to take it personal, and my personal question to you is: Why not you?',
    quoteSub: "You've got the brains, you can make decisions, you can study the plan, you can change your life, you can make your dreams come true. Why not you?",
    environment: 'horizon',
    accent: '#D9B486',
    accentStrong: '#F0C998',
    focalPoint: '75% 50%',
    durationMs: 14000,
  },
  {
    id: 'hero-10',
    index: 9,
    tag: 'TRY HARD AGAIN',
    imageSrc: '/images/inspirational/hero-10.jpg',
    thumbnailSrc: '/images/inspirational/hero-10.jpg',
    altText: 'Con đường uốn lượn qua thung lũng sương mù hướng về phía bình minh rạng rỡ',
    quote: 'Ever tried. Ever failed. No matter. Try again. Fail again. Fail better. The world is yours. Treat everyone kindly and light up the night.',
    author: '— Peter Dinklage',
    environment: 'dawn',
    accent: '#D9C28A',
    accentStrong: '#EED79D',
    focalPoint: '70% 50%',
    durationMs: 12000,
  },
];

// Mặc định mở ở slide 05 (hero-05 - PATIENCE)
export const DEFAULT_SLIDE_INDEX = 4;
