import { Container } from '@/components/ui/Container';
import { CodeWindow } from '@/components/ui/CodeWindow';
import styles from './FeaturesSection.module.css';

const EDITORIAL_ROWS = [
  {
    id: 'autocomplete',
    title: 'API Gợi ý địa chỉ tự động',
    description:
      'Trả về danh sách gợi ý địa chỉ trực tiếp từ Google Maps ngay lập tức mỗi khi người dùng nhập từng ký tự.',

    imagePosition: 'left' as const,
    code: `{
  "status": "OK",
  "results": [
    {
      "formatted_address": "1600 Amphitheatre Parkway",
      "geometry": {
        "location": {
          "lat": 37.4224764,
          "lng": -122.0842499
        }
      }
    }
  ]
}`,
    language: 'json',
    codeTitle: '',
  },
  {
    id: 'place-details',
    title: 'API Truy xuất chi tiết địa điểm',
    description:
      'Lấy thông tin chi tiết của một địa điểm (tọa độ, giờ mở cửa, hình ảnh, đánh giá) từ Google Maps.',

    imagePosition: 'right' as const,
    code: `{
  "status": "OK",
  "routes": [
    {
      "distance_meters": 1450,
      "duration_seconds": 320,
      "legs": [
        {
          "start_location": { "lat": 37.422, "lng": -122.084 },
          "end_location": { "lat": 37.422, "lng": -122.085 }
        }
      ]
    }
  ]
}`,
    language: 'json',
    codeTitle: '',
  },
  {
    id: 'search',
    title: 'API Tìm kiếm địa điểm bản đồ',
    description:
      'Tìm kiếm và phân loại mọi địa điểm (nhà hàng, khách sạn, trạm xăng...) thông qua các từ khóa.',

    imagePosition: 'left' as const,
    code: `{
  "status": "OK",
  "results": [
    {
      "name": "Blue Bottle Coffee",
      "rating": 4.6,
      "geometry": {
        "location": {
          "lat": 37.7749,
          "lng": -122.4194
        }
      }
    }
  ]
}`,
    language: 'json',
    codeTitle: '',
  },
];

export function FeaturesSection() {
  return (
    <section className={styles.section} id="features">
      <Container>
        {/* Editorial Lockup */}
        <header className={styles.header}>
          <p className={`text-eyebrow ${styles.eyebrow}`}>Bộ 3 API Cốt Lõi</p>
          <h2 className={`text-heading-md ${styles.heading}`}>
            Tối Giản Dữ Liệu Địa Điểm
          </h2>
        </header>

        {/* 12-Column Editorial Rows */}
        <div className={styles.rowsContainer}>
          {EDITORIAL_ROWS.map((row) => (
            <article
              key={row.id}
              className={`${styles.row} ${row.imagePosition === 'right' ? styles.rowReverse : ''
                }`}
            >
              {/* Media Thumbnail (5/12 columns on desktop) */}
              <div className={styles.mediaCol}>
                <div className={styles.codeWrapper}>
                  <CodeWindow
                    code={row.code}
                    language={row.language}
                    title={row.codeTitle}
                  />
                </div>
              </div>

              {/* Text Block (7/12 columns on desktop) */}
              <div className={styles.textCol}>
                <h3 className={`text-heading-sm ${styles.title}`}>{row.title}</h3>
                <p className={styles.description}>{row.description}</p>

              </div>
            </article>
          ))}
        </div>
      </Container>
    </section>
  );
}
