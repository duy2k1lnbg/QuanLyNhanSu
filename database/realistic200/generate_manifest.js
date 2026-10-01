const fs = require('fs');
const path = require('path');

// Deterministic PRNG
class SeededRandom {
  constructor(seed = 20260929) {
    this.seed = seed;
  }
  next() {
    this.seed = (this.seed * 9301 + 49297) % 233280;
    return this.seed / 233280;
  }
  nextInt(min, max) {
    return Math.floor(min + this.next() * (max - min + 1));
  }
  choice(arr) {
    return arr[this.nextInt(0, arr.length - 1)];
  }
}

const rng = new SeededRandom(20260929);

// Data Pools
const SURNAMES = [
  "Nguyễn", "Nguyễn", "Nguyễn", "Nguyễn", "Nguyễn",
  "Trần", "Trần", "Trần",
  "Lê", "Lê", "Lê",
  "Phạm", "Phạm",
  "Hoàng", "Huỳnh", "Phan", "Vũ", "Võ", "Đặng", "Bùi", "Đỗ",
  "Hồ", "Ngô", "Dương", "Lý", "Đinh", "Đoàn", "Lâm", "Trịnh", "Mai", "Đào", "Cao"
];

const MALE_MIDDLES = [
  "Văn", "Đức", "Đình", "Quang", "Minh", "Hữu", "Tuấn", "Ngọc", "Thanh",
  "Quốc", "Công", "Duy", "Trọng", "Hoàng", "Khắc", "Bảo", "Tiến", "Trung"
];

const FEMALE_MIDDLES = [
  "Thị", "Thị", "Ngọc", "Thu", "Thanh", "Phương", "Hồng", "Kim", "Bích",
  "Lan", "Mai", "Quỳnh", "Ánh", "Thúy", "Mỹ", "Diệu", "Tuyết"
];

const MALE_NAMES = [
  "An", "Bình", "Cường", "Dũng", "Đạt", "Giang", "Hải", "Hiếu", "Hoàng", "Huy",
  "Hùng", "Khoa", "Kiên", "Lâm", "Long", "Minh", "Nam", "Nghĩa", "Phong", "Phúc",
  "Quân", "Sơn", "Thành", "Thắng", "Thịnh", "Tiến", "Trung", "Tuấn", "Tùng", "Việt", "Vũ"
];

const FEMALE_NAMES = [
  "Anh", "Ánh", "Châu", "Dung", "Duyên", "Giang", "Hà", "Hạnh", "Hằng", "Hoa",
  "Hương", "Huyền", "Lan", "Linh", "Ly", "Mai", "My", "Nga", "Ngân", "Ngọc",
  "Nhung", "Oanh", "Phương", "Quyên", "Quỳnh", "Thảo", "Thu", "Thủy", "Trang",
  "Trâm", "Trinh", "Uyên", "Vân", "Vy", "Yến"
];

const STREET_NAMES = [
  "Trần Phú", "Nguyễn Gia Thiều", "Ngô Gia Tự", "Lý Thường Kiệt", "Hai Bà Trưng",
  "Nguyễn Văn Cừ", "Hùng Vương", "Lê Lợi", "Trần Hưng Đạo", "Quang Trung",
  "Hoàng Hoa Thám", "Bà Triệu", "Võ Thị Sáu", "Phan Chu Trinh", "Nguyễn Du",
  "Đường 286", "Đường 295", "Đường Tỉnh 277", "Quốc Lộ 1A", "Quốc Lộ 18"
];

const LOCATIONS = [
  { ward: "Phường Suối Hoa", district: "Thành phố Bắc Ninh", province: "Bắc Ninh", code: "027" },
  { ward: "Phường Tiền An", district: "Thành phố Bắc Ninh", province: "Bắc Ninh", code: "027" },
  { ward: "Phường Ninh Xá", district: "Thành phố Bắc Ninh", province: "Bắc Ninh", code: "027" },
  { ward: "Phường Vũ Ninh", district: "Thành phố Bắc Ninh", province: "Bắc Ninh", code: "027" },
  { ward: "Phường Đình Bảng", district: "Thành phố Từ Sơn", province: "Bắc Ninh", code: "027" },
  { ward: "Phường Đồng Kỵ", district: "Thành phố Từ Sơn", province: "Bắc Ninh", code: "027" },
  { ward: "Xã Yên Trung", district: "Huyện Yên Phong", province: "Bắc Ninh", code: "027" },
  { ward: "Xã Long Châu", district: "Huyện Yên Phong", province: "Bắc Ninh", code: "027" },
  { ward: "Xã Đông Phong", district: "Huyện Yên Phong", province: "Bắc Ninh", code: "027" },
  { ward: "Thị trấn Chờ", district: "Huyện Yên Phong", province: "Bắc Ninh", code: "027" },
  { ward: "Thị trấn Phố Mới", district: "Thị xã Quế Võ", province: "Bắc Ninh", code: "027" },
  { ward: "Xã Phương Liễu", district: "Thị xã Quế Võ", province: "Bắc Ninh", code: "027" },
  { ward: "Thị trấn Lim", district: "Huyện Tiên Du", province: "Bắc Ninh", code: "027" },
  { ward: "Phường Bích Động", district: "Thị xã Việt Yên", province: "Bắc Giang", code: "024" },
  { ward: "Phường Nếnh", district: "Thị xã Việt Yên", province: "Bắc Giang", code: "024" },
  { ward: "Phường Tăng Tiến", district: "Thị xã Việt Yên", province: "Bắc Giang", code: "024" },
  { ward: "Phường Hoàng Văn Thụ", district: "Thành phố Bắc Giang", province: "Bắc Giang", code: "024" },
  { ward: "Thị trấn Thắng", district: "Huyện Hiệp Hòa", province: "Bắc Giang", code: "024" },
  { ward: "Phường Bồ Đề", district: "Quận Long Biên", province: "Hà Nội", code: "001" },
  { ward: "Phường Ngọc Lâm", district: "Quận Long Biên", province: "Hà Nội", code: "001" },
  { ward: "Thị trấn Trâu Quỳ", district: "Huyện Gia Lâm", province: "Hà Nội", code: "001" },
  { ward: "Xã Cổ Bi", district: "Huyện Gia Lâm", province: "Hà Nội", code: "001" },
  { ward: "Phường Dịch Vọng Hậu", district: "Quận Cầu Giấy", province: "Hà Nội", code: "001" },
  { ward: "Phường Trung Hòa", district: "Quận Cầu Giấy", province: "Hà Nội", code: "001" },
  { ward: "Phường Mỹ Đình 1", district: "Quận Nam Từ Liêm", province: "Hà Nội", code: "001" },
  { ward: "Phường Cẩm Thượng", district: "Thành phố Hải Dương", province: "Hải Dương", code: "030" },
  { ward: "Thị trấn Lai Cách", district: "Huyện Cẩm Giàng", province: "Hải Dương", code: "030" },
  { ward: "Thị trấn Như Quỳnh", district: "Huyện Văn Lâm", province: "Hưng Yên", code: "033" }
];

const HOSPITALS = [
  "Bệnh viện Đa khoa tỉnh Bắc Ninh",
  "Bệnh viện Đa khoa huyện Yên Phong",
  "Bệnh viện Sản Nhi Bắc Ninh",
  "Bệnh viện Quân Y 110",
  "Bệnh viện Đa khoa thị xã Quế Võ",
  "Bệnh viện Đa khoa tỉnh Bắc Giang",
  "Bệnh viện Đa khoa thị xã Việt Yên",
  "Bệnh viện Đa khoa Đức Giang - Hà Nội",
  "Bệnh viện Đa khoa Gia Lâm - Hà Nội",
  "Bệnh viện Bạch Mai - Hà Nội",
  "Bệnh viện Quân Y 108 - Hà Nội",
  "Bệnh viện E - Hà Nội"
];

const PHONE_PREFIXES = ["098", "097", "096", "086", "091", "094", "088", "090", "093", "089", "070", "079", "038", "039", "036", "035"];

// Department layout mapped sequentially to MANV 1..200:
// Exactly 23 departments, exactly 200 total!
const DEPT_CONFIGS = [
  { id: 4, name: "Bộ phận hành chính văn phòng", count: 8, pb: 1, roles: [
    { manvOffset: 0, idcv: 1, title: "Tổng Giám Đốc", salary: [80000000, 85000000], isFemale: false, age: [50, 55], idtd: 24 },
    { manvOffset: 1, idcv: 23, title: "Trưởng phòng", salary: [28000000, 35000000], isFemale: false, age: [38, 45], idtd: 23 },
    { manvOffset: 2, idcv: 24, title: "Phó phòng", salary: [20000000, 26000000], isFemale: true, age: [32, 38], idtd: 23 },
    { manvOffset: 3, idcv: 25, title: "Nhân viên", salary: [11000000, 15000000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 4, idcv: 27, title: "Lễ tân", salary: [9000000, 12000000], isFemale: true, age: [22, 27], idtd: 22 }, // MANV 5
    { manvOffset: 5, idcv: 25, title: "Nhân viên", salary: [10000000, 14000000], isFemale: false, age: [25, 30], idtd: 23 }, // MANV 6
    { manvOffset: 6, idcv: 25, title: "Nhân viên", salary: [10500000, 14500000], isFemale: true, age: [25, 30], idtd: 23 }, // MANV 7
    { manvOffset: 7, idcv: 25, title: "Nhân viên", salary: [10000000, 14000000], isFemale: false, age: [26, 31], idtd: 23 } // MANV 8
  ]},
  { id: 40, name: "Bộ phận Pháp lý", count: 3, pb: 40, roles: [
    { manvOffset: 0, idcv: 23, title: "Trưởng phòng", salary: [32000000, 42000000], isFemale: false, age: [38, 45], idtd: 24 }, // MANV 9
    { manvOffset: 1, idcv: 32, title: "Chuyên viên", salary: [16000000, 22000000], isFemale: true, age: [28, 35], idtd: 23 },
    { manvOffset: 2, idcv: 32, title: "Chuyên viên", salary: [15000000, 20000000], isFemale: false, age: [27, 33], idtd: 23 }
  ]},
  { id: 23, name: "Bộ phận Nhân sự", count: 8, pb: 24, roles: [
    { manvOffset: 0, idcv: 39, title: "Trưởng phòng nhân sự", salary: [32000000, 42000000], isFemale: true, age: [38, 45], idtd: 23 }, // MANV 12
    { manvOffset: 1, idcv: 24, title: "Phó phòng", salary: [22000000, 28000000], isFemale: true, age: [32, 38], idtd: 23 },
    { manvOffset: 2, idcv: 32, title: "Chuyên viên", salary: [15000000, 20000000], isFemale: false, age: [28, 34], idtd: 23 },
    { manvOffset: 3, idcv: 32, title: "Chuyên viên", salary: [14000000, 19000000], isFemale: true, age: [27, 33], idtd: 23 },
    { manvOffset: 4, idcv: 29, title: "Nhân sự", salary: [11000000, 15000000], isFemale: false, age: [25, 30], idtd: 23 },
    { manvOffset: 5, idcv: 29, title: "Nhân sự", salary: [11500000, 15500000], isFemale: true, age: [25, 30], idtd: 23 }, // MANV 17
    { manvOffset: 6, idcv: 29, title: "Nhân sự", salary: [11000000, 15000000], isFemale: false, age: [24, 29], idtd: 23 }, // MANV 18
    { manvOffset: 7, idcv: 29, title: "Nhân sự", salary: [10500000, 14500000], isFemale: true, age: [24, 29], idtd: 23 }
  ]},
  { id: 24, name: "Bộ phận Kế toán", count: 10, pb: 25, roles: [
    { manvOffset: 0, idcv: 23, title: "Trưởng phòng", salary: [30000000, 42000000], isFemale: true, age: [38, 46], idtd: 23 }, // MANV 20
    { manvOffset: 1, idcv: 24, title: "Phó phòng", salary: [22000000, 28000000], isFemale: false, age: [33, 40], idtd: 23 },
    { manvOffset: 2, idcv: 26, title: "Kế toán", salary: [16000000, 22000000], isFemale: true, age: [29, 36], idtd: 23 },
    { manvOffset: 3, idcv: 26, title: "Kế toán", salary: [15000000, 20000000], isFemale: true, age: [28, 35], idtd: 23 },
    { manvOffset: 4, idcv: 26, title: "Kế toán", salary: [14000000, 19000000], isFemale: false, age: [27, 34], idtd: 23 },
    { manvOffset: 5, idcv: 26, title: "Kế toán", salary: [13000000, 18000000], isFemale: true, age: [26, 32], idtd: 23 },
    { manvOffset: 6, idcv: 26, title: "Kế toán", salary: [13000000, 18000000], isFemale: true, age: [26, 32], idtd: 23 },
    { manvOffset: 7, idcv: 26, title: "Kế toán", salary: [12000000, 17000000], isFemale: false, age: [25, 30], idtd: 23 },
    { manvOffset: 8, idcv: 26, title: "Kế toán", salary: [12000000, 16000000], isFemale: true, age: [24, 29], idtd: 23 },
    { manvOffset: 9, idcv: 26, title: "Kế toán", salary: [11500000, 15500000], isFemale: true, age: [24, 28], idtd: 23 }
  ]},
  { id: 33, name: "Bộ phận Tài chính", count: 5, pb: 33, roles: [
    { manvOffset: 0, idcv: 21, title: "Giám đốc", salary: [48000000, 62000000], isFemale: false, age: [42, 50], idtd: 24 }, // MANV 30
    { manvOffset: 1, idcv: 43, title: "Chuyên viên phân tích", salary: [20000000, 27000000], isFemale: true, age: [30, 37], idtd: 23 },
    { manvOffset: 2, idcv: 32, title: "Chuyên viên", salary: [16000000, 22000000], isFemale: false, age: [28, 34], idtd: 23 },
    { manvOffset: 3, idcv: 32, title: "Chuyên viên", salary: [15000000, 20000000], isFemale: true, age: [26, 32], idtd: 23 },
    { manvOffset: 4, idcv: 25, title: "Nhân viên", salary: [12000000, 16000000], isFemale: false, age: [25, 30], idtd: 23 }
  ]},
  { id: 5, name: "Bộ phận marketing", count: 6, pb: 23, roles: [
    { manvOffset: 0, idcv: 38, title: "Trưởng phòng marketing", salary: [30000000, 42000000], isFemale: false, age: [35, 42], idtd: 23 }, // MANV 35
    { manvOffset: 1, idcv: 24, title: "Phó phòng", salary: [20000000, 26000000], isFemale: true, age: [30, 36], idtd: 23 },
    { manvOffset: 2, idcv: 28, title: "Marketing", salary: [14000000, 19000000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 3, idcv: 28, title: "Marketing", salary: [13000000, 18000000], isFemale: true, age: [25, 30], idtd: 23 },
    { manvOffset: 4, idcv: 28, title: "Marketing", salary: [12000000, 17000000], isFemale: false, age: [24, 29], idtd: 23 },
    { manvOffset: 5, idcv: 28, title: "Marketing", salary: [11000000, 16000000], isFemale: true, age: [23, 28], idtd: 23 }
  ]},
  { id: 32, name: "Bộ phận Quảng cáo", count: 3, pb: 32, roles: [
    { manvOffset: 0, idcv: 31, title: "Trưởng bộ phận", salary: [24000000, 32000000], isFemale: false, age: [32, 39], idtd: 23 }, // MANV 41
    { manvOffset: 1, idcv: 53, title: "Nhân viên quảng cáo", salary: [13000000, 18000000], isFemale: true, age: [26, 31], idtd: 23 },
    { manvOffset: 2, idcv: 53, title: "Nhân viên quảng cáo", salary: [12000000, 17000000], isFemale: false, age: [24, 29], idtd: 23 }
  ]},
  { id: 31, name: "Bộ phận Thiết kế", count: 4, pb: 31, roles: [
    { manvOffset: 0, idcv: 31, title: "Trưởng bộ phận", salary: [25000000, 34000000], isFemale: false, age: [33, 40], idtd: 23 }, // MANV 44
    { manvOffset: 1, idcv: 51, title: "Nhân viên thiết kế", salary: [16000000, 22000000], isFemale: true, age: [27, 33], idtd: 23 },
    { manvOffset: 2, idcv: 51, title: "Nhân viên thiết kế", salary: [14000000, 20000000], isFemale: false, age: [25, 30], idtd: 23 },
    { manvOffset: 3, idcv: 51, title: "Nhân viên thiết kế", salary: [13000000, 18000000], isFemale: true, age: [24, 28], idtd: 23 }
  ]},
  { id: 25, name: "Bộ phận IT", count: 12, pb: 26, roles: [
    { manvOffset: 0, idcv: 40, title: "Trưởng phòng IT", salary: [36000000, 50000000], isFemale: false, age: [36, 45], idtd: 23 }, // MANV 48
    { manvOffset: 1, idcv: 24, title: "Phó phòng", salary: [26000000, 35000000], isFemale: false, age: [32, 38], idtd: 23 },
    { manvOffset: 2, idcv: 48, title: "Nhân viên lập trình", salary: [22000000, 32000000], isFemale: false, age: [28, 34], idtd: 23 },
    { manvOffset: 3, idcv: 48, title: "Nhân viên lập trình", salary: [20000000, 30000000], isFemale: true, age: [27, 33], idtd: 23 },
    { manvOffset: 4, idcv: 48, title: "Nhân viên lập trình", salary: [19000000, 28000000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 5, idcv: 48, title: "Nhân viên lập trình", salary: [18000000, 26000000], isFemale: false, age: [25, 30], idtd: 23 },
    { manvOffset: 6, idcv: 48, title: "Nhân viên lập trình", salary: [17000000, 24000000], isFemale: true, age: [24, 29], idtd: 23 },
    { manvOffset: 7, idcv: 48, title: "Nhân viên lập trình", salary: [16000000, 22000000], isFemale: false, age: [24, 28], idtd: 23 },
    { manvOffset: 8, idcv: 30, title: "IT Support", salary: [14000000, 19000000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 9, idcv: 30, title: "IT Support", salary: [13000000, 18000000], isFemale: false, age: [25, 30], idtd: 23 },
    { manvOffset: 10, idcv: 30, title: "IT Support", salary: [12000000, 17000000], isFemale: true, age: [24, 29], idtd: 23 },
    { manvOffset: 11, idcv: 30, title: "IT Support", salary: [11500000, 16000000], isFemale: false, age: [23, 27], idtd: 23 }
  ]},
  { id: 38, name: "Bộ phận Hỗ trợ kỹ thuật", count: 4, pb: 38, roles: [
    { manvOffset: 0, idcv: 31, title: "Trưởng bộ phận", salary: [22000000, 30000000], isFemale: false, age: [32, 38], idtd: 23 }, // MANV 60
    { manvOffset: 1, idcv: 30, title: "IT Support", salary: [13000000, 18000000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 2, idcv: 30, title: "IT Support", salary: [12000000, 17000000], isFemale: true, age: [25, 30], idtd: 23 },
    { manvOffset: 3, idcv: 30, title: "IT Support", salary: [11500000, 16000000], isFemale: false, age: [24, 28], idtd: 23 }
  ]},
  { id: 35, name: "Bộ phận Đào tạo và Phát triển", count: 3, pb: 35, roles: [
    { manvOffset: 0, idcv: 31, title: "Trưởng bộ phận", salary: [24000000, 32000000], isFemale: true, age: [34, 42], idtd: 23 }, // MANV 64
    { manvOffset: 1, idcv: 25, title: "Nhân viên", salary: [13000000, 18000000], isFemale: true, age: [27, 33], idtd: 23 },
    { manvOffset: 2, idcv: 25, title: "Nhân viên", salary: [12000000, 16500000], isFemale: false, age: [25, 30], idtd: 23 }
  ]},
  { id: 27, name: "Bộ phận Phát triển sản phẩm", count: 8, pb: 28, roles: [
    { manvOffset: 0, idcv: 49, title: "Trưởng phòng phát triển sản phẩm", salary: [34000000, 48000000], isFemale: false, age: [36, 44], idtd: 24 }, // MANV 67
    { manvOffset: 1, idcv: 43, title: "Chuyên viên phân tích", salary: [20000000, 28000000], isFemale: true, age: [30, 36], idtd: 23 },
    { manvOffset: 2, idcv: 48, title: "Nhân viên lập trình", salary: [20000000, 28000000], isFemale: false, age: [28, 34], idtd: 23 },
    { manvOffset: 3, idcv: 48, title: "Nhân viên lập trình", salary: [19000000, 26000000], isFemale: false, age: [27, 32], idtd: 23 },
    { manvOffset: 4, idcv: 48, title: "Nhân viên lập trình", salary: [18000000, 25000000], isFemale: true, age: [26, 31], idtd: 23 },
    { manvOffset: 5, idcv: 32, title: "Chuyên viên", salary: [16000000, 22000000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 6, idcv: 32, title: "Chuyên viên", salary: [15000000, 21000000], isFemale: true, age: [25, 30], idtd: 23 },
    { manvOffset: 7, idcv: 25, title: "Nhân viên", salary: [13000000, 18000000], isFemale: false, age: [24, 29], idtd: 23 }
  ]},
  { id: 34, name: "Bộ phận Nghiên cứu và Phát triển", count: 5, pb: 34, roles: [
    { manvOffset: 0, idcv: 23, title: "Trưởng phòng", salary: [35000000, 48000000], isFemale: false, age: [38, 46], idtd: 25 }, // MANV 75 (Tiến sĩ)
    { manvOffset: 1, idcv: 32, title: "Chuyên viên", salary: [22000000, 30000000], isFemale: false, age: [32, 38], idtd: 24 },
    { manvOffset: 2, idcv: 32, title: "Chuyên viên", salary: [20000000, 27000000], isFemale: true, age: [29, 35], idtd: 24 },
    { manvOffset: 3, idcv: 32, title: "Chuyên viên", salary: [18000000, 25000000], isFemale: false, age: [28, 33], idtd: 23 },
    { manvOffset: 4, idcv: 32, title: "Chuyên viên", salary: [17000000, 23000000], isFemale: true, age: [26, 31], idtd: 23 }
  ]},
  { id: 29, name: "Bộ phận Quản lý dự án", count: 5, pb: 29, roles: [
    { manvOffset: 0, idcv: 35, title: "Quản lý dự án", salary: [32000000, 44000000], isFemale: false, age: [35, 43], idtd: 23 }, // MANV 80
    { manvOffset: 1, idcv: 35, title: "Quản lý dự án", salary: [28000000, 38000000], isFemale: true, age: [32, 39], idtd: 23 },
    { manvOffset: 2, idcv: 35, title: "Quản lý dự án", salary: [25000000, 34000000], isFemale: false, age: [30, 36], idtd: 23 },
    { manvOffset: 3, idcv: 32, title: "Chuyên viên", salary: [16000000, 22000000], isFemale: false, age: [27, 33], idtd: 23 },
    { manvOffset: 4, idcv: 25, title: "Nhân viên", salary: [12000000, 17000000], isFemale: true, age: [25, 29], idtd: 23 }
  ]},
  { id: 28, name: "Bộ phận Dịch vụ khách hàng", count: 5, pb: 29, roles: [
    { manvOffset: 0, idcv: 23, title: "Trưởng phòng", salary: [26000000, 36000000], isFemale: true, age: [34, 42], idtd: 23 }, // MANV 85
    { manvOffset: 1, idcv: 42, title: "Chuyên viên tư vấn", salary: [15000000, 20000000], isFemale: true, age: [28, 34], idtd: 23 },
    { manvOffset: 2, idcv: 42, title: "Chuyên viên tư vấn", salary: [14000000, 19000000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 3, idcv: 44, title: "Nhân viên dịch vụ khách hàng", salary: [11000000, 15000000], isFemale: true, age: [24, 29], idtd: 23 },
    { manvOffset: 4, idcv: 44, title: "Nhân viên dịch vụ khách hàng", salary: [10500000, 14500000], isFemale: true, age: [23, 28], idtd: 23 }
  ]},
  { id: 39, name: "Bộ phận Chăm sóc khách hàng", count: 6, pb: 39, roles: [
    { manvOffset: 0, idcv: 31, title: "Trưởng bộ phận", salary: [22000000, 30000000], isFemale: true, age: [32, 38], idtd: 23 }, // MANV 90
    { manvOffset: 1, idcv: 47, title: "Nhân viên chăm sóc khách hàng", salary: [12000000, 16000000], isFemale: true, age: [26, 31], idtd: 23 },
    { manvOffset: 2, idcv: 47, title: "Nhân viên chăm sóc khách hàng", salary: [11500000, 15500000], isFemale: true, age: [25, 30], idtd: 23 },
    { manvOffset: 3, idcv: 47, title: "Nhân viên chăm sóc khách hàng", salary: [11000000, 15000000], isFemale: false, age: [25, 30], idtd: 23 },
    { manvOffset: 4, idcv: 47, title: "Nhân viên chăm sóc khách hàng", salary: [10500000, 14500000], isFemale: true, age: [24, 29], idtd: 23 },
    { manvOffset: 5, idcv: 47, title: "Nhân viên chăm sóc khách hàng", salary: [10000000, 14000000], isFemale: true, age: [23, 28], idtd: 23 }
  ]},
  { id: 36, name: "Bộ phận Bán hàng", count: 8, pb: 36, roles: [
    { manvOffset: 0, idcv: 41, title: "Trưởng phòng bán hàng", salary: [30000000, 44000000], isFemale: false, age: [36, 44], idtd: 23 }, // MANV 96
    { manvOffset: 1, idcv: 24, title: "Phó phòng", salary: [20000000, 27000000], isFemale: false, age: [32, 38], idtd: 23 },
    { manvOffset: 2, idcv: 33, title: "Nhân viên kinh doanh", salary: [13000000, 18000000], isFemale: true, age: [27, 33], idtd: 23 },
    { manvOffset: 3, idcv: 33, title: "Nhân viên kinh doanh", salary: [12500000, 17500000], isFemale: false, age: [26, 32], idtd: 23 },
    { manvOffset: 4, idcv: 33, title: "Nhân viên kinh doanh", salary: [12000000, 17000000], isFemale: false, age: [25, 30], idtd: 23 },
    { manvOffset: 5, idcv: 33, title: "Nhân viên kinh doanh", salary: [11500000, 16000000], isFemale: true, age: [24, 29], idtd: 23 },
    { manvOffset: 6, idcv: 33, title: "Nhân viên kinh doanh", salary: [11000000, 15500000], isFemale: false, age: [24, 29], idtd: 23 },
    { manvOffset: 7, idcv: 33, title: "Nhân viên kinh doanh", salary: [10500000, 15000000], isFemale: true, age: [23, 28], idtd: 23 }
  ]},
  { id: 1, name: "Bộ phận kinh doanh", count: 20, pb: 22, roles: [
    { manvOffset: 0, idcv: 22, title: "Phó giám đốc", salary: [40000000, 52000000], isFemale: false, age: [40, 48], idtd: 24 }, // MANV 104
    { manvOffset: 1, idcv: 24, title: "Phó phòng", salary: [22000000, 29000000], isFemale: false, age: [34, 40], idtd: 23 },
    { manvOffset: 2, idcv: 50, title: "Chuyên viên thị trường", salary: [16000000, 22000000], isFemale: true, age: [29, 35], idtd: 23 },
    { manvOffset: 3, idcv: 50, title: "Chuyên viên thị trường", salary: [15000000, 21000000], isFemale: false, age: [28, 34], idtd: 23 },
    // 16 nhân viên kinh doanh
    ...Array.from({ length: 16 }, (_, idx) => ({
      manvOffset: 4 + idx, idcv: 33, title: "Nhân viên kinh doanh", salary: [11000000, 17000000], isFemale: idx % 2 === 0, age: [23, 31], idtd: 23
    }))
  ]},
  { id: 2, name: "Bộ phận kỹ thuật", count: 12, pb: 34, roles: [
    { manvOffset: 0, idcv: 21, title: "Giám đốc", salary: [45000000, 60000000], isFemale: false, age: [42, 50], idtd: 24 }, // MANV 124
    { manvOffset: 1, idcv: 31, title: "Trưởng bộ phận", salary: [25000000, 34000000], isFemale: false, age: [35, 42], idtd: 23 },
    // 10 nhân viên kỹ thuật
    ...Array.from({ length: 10 }, (_, idx) => ({
      manvOffset: 2 + idx, idcv: 34, title: "Nhân viên kỹ thuật", salary: [13000000, 21000000], isFemale: idx % 4 === 0, age: [24, 34], idtd: (idx % 3 === 0 ? 22 : 23)
    }))
  ]},
  { id: 30, name: "Bộ phận Logistics", count: 8, pb: 30, roles: [
    { manvOffset: 0, idcv: 23, title: "Trưởng phòng", salary: [26000000, 36000000], isFemale: false, age: [36, 44], idtd: 23 }, // MANV 136
    { manvOffset: 1, idcv: 54, title: "Nhân viên logistics", salary: [13000000, 18000000], isFemale: false, age: [27, 33], idtd: 23 },
    { manvOffset: 2, idcv: 54, title: "Nhân viên logistics", salary: [12000000, 17000000], isFemale: true, age: [26, 31], idtd: 23 },
    { manvOffset: 3, idcv: 46, title: "Nhân viên kho", salary: [10000000, 14000000], isFemale: false, age: [25, 32], idtd: 22 },
    { manvOffset: 4, idcv: 46, title: "Nhân viên kho", salary: [9500000, 13500000], isFemale: false, age: [24, 30], idtd: 26 },
    { manvOffset: 5, idcv: 46, title: "Nhân viên kho", salary: [9500000, 13000000], isFemale: true, age: [24, 29], idtd: 26 },
    { manvOffset: 6, idcv: 46, title: "Nhân viên kho", salary: [9000000, 12500000], isFemale: false, age: [23, 28], idtd: 30 },
    { manvOffset: 7, idcv: 46, title: "Nhân viên kho", salary: [9000000, 12000000], isFemale: false, age: [22, 27], idtd: 30 }
  ]},
  { id: 37, name: "Bộ phận Vận hành", count: 7, pb: 37, roles: [
    { manvOffset: 0, idcv: 22, title: "Phó giám đốc", salary: [38000000, 50000000], isFemale: false, age: [40, 48], idtd: 23 }, // MANV 144
    { manvOffset: 1, idcv: 37, title: "Nhân viên mua hàng", salary: [15000000, 21000000], isFemale: true, age: [29, 36], idtd: 23 },
    { manvOffset: 2, idcv: 37, title: "Nhân viên mua hàng", salary: [14000000, 19000000], isFemale: false, age: [27, 33], idtd: 23 },
    { manvOffset: 3, idcv: 25, title: "Nhân viên", salary: [11000000, 15000000], isFemale: false, age: [26, 31], idtd: 22 },
    { manvOffset: 4, idcv: 25, title: "Nhân viên", salary: [10500000, 14500000], isFemale: true, age: [25, 30], idtd: 22 },
    { manvOffset: 5, idcv: 25, title: "Nhân viên", salary: [10000000, 14000000], isFemale: false, age: [24, 29], idtd: 26 },
    { manvOffset: 6, idcv: 25, title: "Nhân viên", salary: [9500000, 13500000], isFemale: false, age: [23, 28], idtd: 30 }
  ]},
  { id: 3, name: "Bộ phận sản xuất", count: 30, pb: 27, roles: [
    { manvOffset: 0, idcv: 21, title: "Giám đốc", salary: [46000000, 60000000], isFemale: false, age: [42, 50], idtd: 23 }, // MANV 151
    { manvOffset: 1, idcv: 36, title: "Giám sát sản xuất", salary: [20000000, 28000000], isFemale: false, age: [34, 42], idtd: 23 },
    { manvOffset: 2, idcv: 36, title: "Giám sát sản xuất", salary: [19000000, 26000000], isFemale: false, age: [32, 39], idtd: 22 },
    { manvOffset: 3, idcv: 52, title: "Quản lý chất lượng", salary: [20000000, 28000000], isFemale: true, age: [31, 38], idtd: 23 },
    // 26 công nhân sản xuất
    ...Array.from({ length: 26 }, (_, idx) => ({
      manvOffset: 4 + idx, idcv: 45, title: "Nhân viên sản xuất", salary: [8500000, 13500000], isFemale: idx % 3 === 0, age: [21, 32], idtd: (idx % 2 === 0 ? 26 : 30)
    }))
  ]},
  { id: 26, name: "Bộ phận Sản xuất", count: 20, pb: 27, roles: [
    { manvOffset: 0, idcv: 31, title: "Trưởng bộ phận", salary: [25000000, 35000000], isFemale: false, age: [36, 43], idtd: 23 }, // MANV 181
    { manvOffset: 1, idcv: 36, title: "Giám sát sản xuất", salary: [19000000, 26000000], isFemale: false, age: [32, 38], idtd: 22 },
    { manvOffset: 2, idcv: 36, title: "Giám sát sản xuất", salary: [18000000, 25000000], isFemale: false, age: [30, 36], idtd: 22 },
    { manvOffset: 3, idcv: 52, title: "Quản lý chất lượng", salary: [18000000, 26000000], isFemale: true, age: [29, 35], idtd: 23 },
    // 16 công nhân sản xuất (including MANV 190..194 resigned, 195..200 probation)
    ...Array.from({ length: 16 }, (_, idx) => ({
      manvOffset: 4 + idx, idcv: 45, title: "Nhân viên sản xuất", salary: [8500000, 13000000], isFemale: idx % 2 === 0, age: [20, 30], idtd: (idx % 3 === 0 ? 27 : 30)
    }))
  ]}
];

// Verify configuration counts
let totalPlanned = 0;
DEPT_CONFIGS.forEach(d => totalPlanned += d.count);
if (totalPlanned !== 200) {
  throw new Error(`Total planned employees is ${totalPlanned}, must be exactly 200!`);
}

function buildRealisticDataset() {
  const employees = [];
  const usedPhones = new Set();
  const usedCccds = new Set();
  const usedNames = new Set();

  let manvCounter = 1;

  for (const dept of DEPT_CONFIGS) {
    for (let rIdx = 0; rIdx < dept.count; rIdx++) {
      const manv = manvCounter++;
      const role = dept.roles[rIdx];

      const isFemale = role.isFemale;
      const idgt = isFemale ? 2 : 1;

      // Full Name
      let fullName = "";
      while (!fullName || usedNames.has(fullName)) {
        const sur = rng.choice(SURNAMES);
        const mid = isFemale ? rng.choice(FEMALE_MIDDLES) : rng.choice(MALE_MIDDLES);
        const given = isFemale ? rng.choice(FEMALE_NAMES) : rng.choice(MALE_NAMES);
        fullName = `${sur} ${mid} ${given}`;
      }
      usedNames.add(fullName);

      // Birth year
      const birthYear = rng.nextInt(role.age[0], role.age[1]);
      const actualBirthYear = 2026 - birthYear;
      const birthMonth = rng.nextInt(1, 12);
      const birthDay = rng.nextInt(1, 28);
      const dobStr = `${actualBirthYear}-${String(birthMonth).padStart(2, '0')}-${String(birthDay).padStart(2, '0')}`;

      // Address & Phone & CCCD
      const loc = rng.choice(LOCATIONS);
      const street = rng.choice(STREET_NAMES);
      const houseNo = rng.nextInt(1, 180);
      const address = `Số ${houseNo}, Phố ${street}, ${loc.ward}, ${loc.district}, Tỉnh ${loc.province}`;

      let phone = "";
      while (!phone || usedPhones.has(phone)) {
        const pfx = rng.choice(PHONE_PREFIXES);
        const sfx = String(rng.nextInt(1000000, 9999999));
        phone = `${pfx}${sfx}`;
      }
      usedPhones.add(phone);

      let cccd = "";
      while (!cccd || usedCccds.has(cccd)) {
        const provCode = loc.code;
        const centGender = (actualBirthYear < 2000) ? (idgt === 1 ? '0' : '1') : (idgt === 1 ? '2' : '3');
        const yearSuffix = String(actualBirthYear).substring(2);
        const random6 = String(rng.nextInt(100000, 999999));
        cccd = `${provCode}${centGender}${yearSuffix}${random6}`;
      }
      usedCccds.add(cccd);

      // Salary
      let salary = Math.round(rng.nextInt(role.salary[0], role.salary[1]) / 500000) * 500000;
      if (manv === 1) salary = 82000000;

      // Allowances: 1 to 4 allowances
      const allowances = [];

      // 1. Phụ cấp chức vụ (5)
      if (role.idcv === 1) allowances.push({ idpc: 5, amount: 8000000, reason: "Phụ cấp chức vụ Tổng Giám Đốc" });
      else if ([21, 22].includes(role.idcv)) allowances.push({ idpc: 5, amount: 5000000, reason: "Phụ cấp chức vụ Ban Giám đốc" });
      else if ([23, 31, 35, 36, 38, 39, 40, 41, 49, 52].includes(role.idcv)) allowances.push({ idpc: 5, amount: 3000000, reason: "Phụ cấp trách nhiệm quản lý" });
      else if (role.idcv === 24) allowances.push({ idpc: 5, amount: 1500000, reason: "Phụ cấp trách nhiệm phó phòng" });

      // 2. Phụ cấp đặc biệt (12) - Expat / Foreign Experts
      if ([30, 75, 80].includes(manv)) {
        allowances.push({ idpc: 12, amount: 6000000, reason: "Phụ cấp chuyên gia nước ngoài" });
      }

      // 3. Phụ cấp chứng chỉ nghề nghiệp (6) - Kế toán kiểm toán CPA, IT Security, PMP
      if ([20, 22, 48, 50, 81].includes(manv)) {
        allowances.push({ idpc: 6, amount: rng.choice([1000000, 1500000, 2000000]), reason: "Phụ cấp chứng chỉ chuyên môn quốc tế" });
      }

      // 4. Phụ cấp đi lại (2)
      if (rng.next() < 0.70 && allowances.length < 4) {
        allowances.push({ idpc: 2, amount: rng.choice([300000, 500000, 800000, 1000000]), reason: "Phụ cấp xăng xe và đi lại" });
      }

      // 5. Phụ cấp nhà ở (1)
      if ((loc.province !== "Bắc Ninh" || rng.next() < 0.35) && allowances.length < 4) {
        allowances.push({ idpc: 1, amount: rng.choice([800000, 1200000, 1500000, 2000000]), reason: "Hỗ trợ nhà ở ngoại tỉnh" });
      }

      // 6. IT / Technical / WFH (7, 11)
      if ((dept.id === 25 || dept.id === 27) && allowances.length < 4) {
        allowances.push({ idpc: 7, amount: rng.choice([800000, 1200000, 1500000]), reason: "Phụ cấp kỹ năng công nghệ cao" });
        if (rng.next() < 0.40 && allowances.length < 4) allowances.push({ idpc: 11, amount: 500000, reason: "Hỗ trợ làm việc từ xa (WFH)" });
      }

      // 7. Production / Quality / Worker: Chuyên cần (9), Khu vực (8)
      if ([3, 26, 30, 37].includes(dept.id) && allowances.length < 4) {
        allowances.push({ idpc: 9, amount: rng.choice([300000, 400000, 500000]), reason: "Thưởng chuyên cần sản xuất" });
        if (dept.id === 3 && manv % 4 === 0 && allowances.length < 4) {
          allowances.push({ idpc: 8, amount: 500000, reason: "Phụ cấp khu vực nhà xưởng nóng hại" });
        }
      }

      // 8. Sales / Kinh doanh: Phụ cấp khác (13 - cước điện thoại)
      if ([1, 36].includes(dept.id) && rng.next() < 0.40 && allowances.length < 4) {
        allowances.push({ idpc: 13, amount: rng.choice([300000, 500000]), reason: "Phụ cấp cước viễn thông liên lạc khách hàng" });
      }

      // 9. Seniority allowance (10) for tenure > 3 years
      if (actualBirthYear <= 1988 && rng.next() < 0.40 && allowances.length < 4) {
        allowances.push({ idpc: 10, amount: rng.choice([500000, 1000000, 1500000]), reason: "Phụ cấp thâm niên công tác" });
      }

      // 10. Phụ cấp gia đình (3) & Người phụ thuộc (4)
      if (actualBirthYear <= 1992 && allowances.length < 4) {
        if (manv % 15 === 0) {
          allowances.push({ idpc: 4, amount: 500000, reason: "Trợ cấp phụng dưỡng người phụ thuộc đặc biệt" });
        } else if (manv % 12 === 0) {
          allowances.push({ idpc: 3, amount: 600000, reason: "Hỗ trợ nuôi con nhỏ dưới 6 tuổi" });
        }
      }

      while (allowances.length > 4) allowances.pop();
      if (allowances.length === 0) {
        allowances.push({ idpc: 2, amount: 500000, reason: "Phụ cấp đi lại tiêu chuẩn" });
      }

      // Special groups
      const isResigned = (manv >= 190 && manv <= 194);
      const isNewbie = (manv >= 195 && manv <= 200);

      let contractStart = "2026-01-01";
      let contractEnd = "2027-12-31";
      let contractType = 2; // Xác định thời hạn

      if (isResigned) {
        contractStart = "2024-01-01";
        contractEnd = "2026-09-15";
      } else if (isNewbie) {
        contractStart = "2026-09-15";
        contractEnd = "2026-11-15";
        contractType = 1; // Thử việc
      } else {
        if (actualBirthYear <= 1985 && rng.next() < 0.40) {
          contractType = 3; // Không xác định thời hạn
          contractEnd = "2029-12-31";
        }
      }

      const contractNo = `${String(manv).padStart(5, '0')}/2026/HDLD`;
      const insuranceNo = `BH27${String(manv).padStart(6, '0')}`;
      const hospital = rng.choice(HOSPITALS);

      // Dependents
      let dependentCount = 0;
      if (actualBirthYear <= 1993 && !isNewbie) {
        const p = rng.next();
        if (p < 0.35) dependentCount = 1;
        else if (p < 0.60) dependentCount = 2;
      }

      // Demographics
      let iddt = 1; // Kinh
      if (rng.next() < 0.10) iddt = rng.choice([2, 3, 4, 21, 22, 23, 25]);

      let idtg = 1; // Khong
      const relP = rng.next();
      if (relP < 0.10) idtg = 3; // Phat Giao
      else if (relP < 0.15) idtg = 21; // Cong Giao

      let idqt = 191; // Viet Nam
      if (manv === 30) idqt = 91;  // Korea (CFO)
      else if (manv === 75) idqt = 85;  // Japan (R&D Head)
      else if (manv === 80) idqt = 9;   // Australia (Project Director)
      else if (manv === 44) idqt = 156; // Singapore (Design Lead)

      employees.push({
        manv,
        code: `NV${String(manv).padStart(4, '0')}`,
        hoten: fullName,
        idgt,
        gender: isFemale ? "Nữ" : "Nam",
        dob: dobStr,
        birthYear: actualBirthYear,
        phone,
        cccd,
        address,
        idbp: dept.id,
        deptName: dept.name,
        idpb: dept.pb,
        idcv: role.idcv,
        positionTitle: role.title,
        idtd: role.idtd,
        iddt,
        idtg,
        idqt,
        salary,
        allowances,
        dependentCount,
        contractNo,
        contractStart,
        contractEnd,
        contractType,
        insuranceNo,
        hospital,
        isResigned,
        isNewbie
      });
    }
  }

  return employees;
}

const manifest = buildRealisticDataset();

// Verify all 35 positions
const presentPositions = new Set(manifest.map(e => e.idcv));
const allRequiredPositions = [
  1, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37,
  38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54
];
const missing = allRequiredPositions.filter(id => !presentPositions.has(id));
if (missing.length > 0) {
  console.error("FATAL: Missing positions:", missing);
  process.exit(1);
} else {
  console.log("✓ SUCCESS: All 35 positions are 100% covered across the 200 employees!");
}

// Verify department counts
console.log("\n--- Department Distribution Verification ---");
DEPT_CONFIGS.forEach(dept => {
  const actual = manifest.filter(e => e.idbp === dept.id).length;
  console.log(`IDBP ${dept.id.toString().padStart(2, ' ')} (${dept.name}): ${actual}/${dept.count} [${actual === dept.count ? 'OK' : 'FAIL'}]`);
});

// Output 20 preview profiles
const previewSampleIds = [1, 5, 6, 7, 8, 9, 12, 17, 18, 20, 30, 44, 48, 75, 80, 96, 104, 124, 151, 192];
const previewData = previewSampleIds.map(id => {
  const e = manifest.find(x => x.manv === id);
  return {
    MANV: e.manv,
    CODE: e.code,
    HOTEN: e.hoten,
    GENDER: e.gender,
    DOB: e.dob,
    PHONE: e.phone,
    CCCD: e.cccd,
    ADDRESS: e.address,
    DEPT: `${e.idbp} - ${e.deptName}`,
    POSITION: `${e.idcv} - ${e.positionTitle}`,
    SALARY: new Intl.NumberFormat('vi-VN').format(e.salary) + ' đ',
    ALLOWANCES: e.allowances.map(a => `IDPC ${a.idpc}: ${new Intl.NumberFormat('vi-VN').format(a.amount)} đ`),
    CONTRACT: `${e.contractNo} (${e.contractStart} -> ${e.contractEnd})`,
    INSURANCE: `${e.insuranceNo} (${e.hospital})`
  };
});

fs.writeFileSync('D:/QL_NS/QuanLyNhanSu/database/realistic200/realistic200_manifest.json', JSON.stringify(manifest, null, 2), 'utf8');
fs.writeFileSync('D:/QL_NS/QuanLyNhanSu/database/realistic200/preview_20_employees.json', JSON.stringify(previewData, null, 2), 'utf8');

console.log("\nSaved realistic200_manifest.json and preview_20_employees.json successfully!");
