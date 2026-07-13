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
  versions: PageVersion[];
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
