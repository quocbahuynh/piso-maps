import { i18n } from '@/lib/i18n';
import { i18nProvider, uiTranslations } from 'fumadocs-ui/i18n';
import { Provider } from '@/components/provider';
import type { ReactNode } from 'react';

const translations = i18n
  .translations()
  .extend(uiTranslations())
  .add('ui', {
    en: { displayName: 'English' },
    vi: {
      displayName: 'Tiếng Việt',
      search: 'Tìm kiếm',
      searchNoResult: 'Không tìm thấy kết quả',
      toc: 'Trong trang này',
      tocNoHeadings: 'Không có tiêu đề',
      lastUpdate: 'Cập nhật lần cuối',
      chooseLanguage: 'Chọn ngôn ngữ',
      nextPage: 'Trang tiếp',
      previousPage: 'Trang trước',
      chooseTheme: 'Giao diện',
      editOnGithub: 'Chỉnh sửa trên GitHub',
      themeLight: 'Sáng',
      themeDark: 'Tối',
      themeSystem: 'Hệ thống',
      codeBlockCopy: 'Sao chép',
      codeBlockCopied: 'Đã sao chép',
      accordionCopyAnchor: 'Sao chép liên kết',
      headingCopyAnchor: 'Sao chép liên kết',
      pageActionsCopyMarkdown: 'Sao chép Markdown',
      pageActionsOpen: 'Mở',
      pageActionsOpenGitHub: 'Mở trong GitHub',
      pageActionsViewMarkdown: 'Xem dạng Markdown',
      pageActionsOpenScira: 'Mở trong Scira AI',
      pageActionsOpenChatGPT: 'Mở trong ChatGPT',
      pageActionsOpenClaude: 'Mở trong Claude',
      pageActionsOpenCursor: 'Mở trong Cursor',
      pageActionsOpenInLLMPrompt: 'Đọc {url}, tôi muốn hỏi về nó.',
      bannerClose: 'Đóng banner',
      searchOpen: 'Mở tìm kiếm',
      searchClose: 'Đóng tìm kiếm',
      menuToggle: 'Chuyển menu',
      themeToggle: 'Chuyển giao diện',
      sidebarOpen: 'Mở sidebar',
      sidebarCollapse: 'Thu gọn sidebar',
      tocInline: 'Mục lục',
      typeTableProp: 'Tham số',
      typeTableType: 'Kiểu',
      typeTableDefault: 'Mặc định',
      typeTableParameters: 'Tham số',
      typeTableReturns: 'Kết quả',
      notFoundTitle: 'Không tìm thấy trang',
      notFoundDescription: 'Trang bạn đang tìm có thể đã bị xóa, đổi tên hoặc tạm thời không khả dụng.',
      notFoundLink: 'Về trang chủ',
    },
  });

export default async function LangLayout({
  params,
  children,
}: {
  params: Promise<{ lang: string }>;
  children: ReactNode;
}) {
  const { lang } = await params;

  return <Provider i18n={i18nProvider(translations, lang)}>{children}</Provider>;
}

export async function generateStaticParams() {
  return i18n.languages.map((lang) => ({ lang }));
}
