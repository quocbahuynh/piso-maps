'use client';
import SearchDialog from '@/components/search';
import { SearchProvider } from 'fumadocs-ui/contexts/search';
import { I18nProvider } from 'fumadocs-ui/contexts/i18n';
import { DirectionProvider } from '@radix-ui/react-direction';
import { NextProvider } from 'fumadocs-core/framework/next';
import { type ReactNode } from 'react';
import type { I18nProviderProps } from 'fumadocs-ui/contexts/i18n';

export function Provider({
  children,
  i18n,
}: {
  children: ReactNode;
  i18n?: I18nProviderProps;
}) {
  return (
    <NextProvider>
      <DirectionProvider dir="ltr">
        <SearchProvider SearchDialog={SearchDialog}>
          {i18n ? <I18nProvider {...i18n}>{children}</I18nProvider> : children}
        </SearchProvider>
      </DirectionProvider>
    </NextProvider>
  );
}
