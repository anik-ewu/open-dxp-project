export interface PageVariant {
  id: string;
  pageId: string;
  name: string;
  blocksJson: string;
  targetSegment: string | null;
  trafficPercentage: number | null;
  priority: number;
  createdAt: string;
}

export interface CreatePageVariantRequest {
  name: string;
  blocksJson: string;
  targetSegment: string | null;
  trafficPercentage: number | null;
  priority: number;
}

export interface VariantAnalytics {
  variantId: string | null;
  variantLabel: string;
  impressions: number;
  conversions: number;
  conversionRate: number;
}
