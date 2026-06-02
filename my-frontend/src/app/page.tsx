import { Navbar } from '@/components/layout/Navbar';
import { Footer } from '@/components/layout/Footer';
import { HeroSection } from '@/components/landing/HeroSection';
import { FeaturesSection } from '@/components/landing/FeaturesSection';
import { QuickStartSection } from '@/components/landing/QuickStartSection';
import { PricingSection } from '@/components/landing/PricingSection';
import { FinalCtaSection } from '@/components/landing/FinalCtaSection';

export default function LandingPage() {
  return (
    <>
      <Navbar />
      <main>
        <HeroSection />
        <FeaturesSection />
        <QuickStartSection />
        <PricingSection />
        <FinalCtaSection />
      </main>
      <Footer />
    </>
  );
}
