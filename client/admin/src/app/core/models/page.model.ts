export interface PageSummary {
  id: string;
  slug: string;
  title: string;
  status: 'Draft' | 'Published';
  updatedAt: string;
}

export interface PageVersion {
  id: string;
  versionNumber: number;
  title: string;
  publishedAt: string;
}

export interface PageDetail {
  id: string;
  slug: string;
  title: string;
  blocksJson: string;
  status: 'Draft' | 'Published';
  latestVersionNumber: number;
  tags: string[];
  versions: PageVersion[];
}

export interface RelatedPage {
  pageId: string;
  slug: string;
  title: string;
  snippet: string;
}

export interface CreatePageRequest {
  slug: string;
  title: string;
  blocksJson: string;
}

export interface UpdatePageDraftRequest {
  title: string;
  blocksJson: string;
}
