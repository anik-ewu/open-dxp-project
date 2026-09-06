import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PageDetail, PageVersion, RelatedPage } from '../../core/models/page.model';
import { PageVariant, VariantAnalytics } from '../../core/models/page-variant.model';
import { PageService } from '../../core/services/page.service';
import { PersonalizationService } from '../../core/services/personalization.service';
import { SearchService } from '../../core/services/search.service';

@Component({
  selector: 'app-page-editor',
  standalone: true,
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './page-editor.component.html',
})
export class PageEditorComponent implements OnInit {
  private readonly pageService = inject(PageService);
  private readonly personalizationService = inject(PersonalizationService);
  private readonly searchService = inject(SearchService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  pageId: string | null = null;
  slug = '';
  title = '';
  blocksJson = '[]';
  status: 'Draft' | 'Published' | null = null;
  versions: PageVersion[] = [];
  tags: string[] = [];
  relatedPages: RelatedPage[] = [];
  saving = false;
  error: string | null = null;

  variants: PageVariant[] = [];
  analytics: VariantAnalytics[] = [];
  variantError: string | null = null;

  newVariantName = '';
  newVariantBlocksJson = '[]';
  newVariantSegment = '';
  newVariantTrafficPercentage: number | null = null;
  newVariantPriority = 0;

  ngOnInit(): void {
    this.pageId = this.route.snapshot.paramMap.get('id');
    if (this.pageId) {
      this.pageService.getById(this.pageId).subscribe((page) => this.applyDetail(page));
      this.loadVariants();
      this.loadAnalytics();
      this.loadRelatedPages();
    }
  }

  save(): void {
    this.error = null;
    this.saving = true;

    if (this.pageId) {
      this.pageService.updateDraft(this.pageId, { title: this.title, blocksJson: this.blocksJson }).subscribe({
        next: (page) => {
          this.applyDetail(page);
          this.saving = false;
        },
        error: (err) => {
          this.error = err.error?.detail ?? 'Failed to save draft.';
          this.saving = false;
        },
      });
    } else {
      this.pageService.create({ slug: this.slug, title: this.title, blocksJson: this.blocksJson }).subscribe({
        next: (page) => this.router.navigate(['/pages', page.id]),
        error: (err) => {
          this.error = err.error?.detail ?? 'Failed to create page.';
          this.saving = false;
        },
      });
    }
  }

  publish(): void {
    if (!this.pageId) {
      return;
    }

    this.error = null;
    this.pageService.publish(this.pageId).subscribe({
      next: () => this.pageService.getById(this.pageId!).subscribe((page) => this.applyDetail(page)),
      error: (err) => {
        this.error = err.error?.detail ?? 'Failed to publish.';
      },
    });
  }

  loadVariants(): void {
    if (!this.pageId) {
      return;
    }
    this.personalizationService.getVariants(this.pageId).subscribe((variants) => (this.variants = variants));
  }

  loadAnalytics(): void {
    if (!this.pageId) {
      return;
    }
    this.personalizationService.getAnalytics(this.pageId).subscribe((analytics) => (this.analytics = analytics));
  }

  loadRelatedPages(): void {
    if (!this.pageId) {
      return;
    }
    this.searchService.getRelated(this.pageId).subscribe((related) => (this.relatedPages = related));
  }

  /** Tags and related pages are computed a few seconds after publish by Kafka consumers. */
  refreshEnrichment(): void {
    if (!this.pageId) {
      return;
    }
    this.pageService.getById(this.pageId).subscribe((page) => this.applyDetail(page));
    this.loadRelatedPages();
  }

  createVariant(): void {
    if (!this.pageId) {
      return;
    }

    this.variantError = null;
    this.personalizationService
      .createVariant(this.pageId, {
        name: this.newVariantName,
        blocksJson: this.newVariantBlocksJson,
        targetSegment: this.newVariantSegment.trim() ? this.newVariantSegment.trim() : null,
        trafficPercentage: this.newVariantTrafficPercentage,
        priority: this.newVariantPriority,
      })
      .subscribe({
        next: () => {
          this.newVariantName = '';
          this.newVariantBlocksJson = '[]';
          this.newVariantSegment = '';
          this.newVariantTrafficPercentage = null;
          this.newVariantPriority = 0;
          this.loadVariants();
        },
        error: (err) => {
          this.variantError = err.error?.detail ?? 'Failed to create variant.';
        },
      });
  }

  deleteVariant(variantId: string): void {
    if (!this.pageId) {
      return;
    }
    this.personalizationService.deleteVariant(this.pageId, variantId).subscribe(() => this.loadVariants());
  }

  private applyDetail(page: PageDetail): void {
    this.slug = page.slug;
    this.title = page.title;
    this.blocksJson = page.blocksJson;
    this.status = page.status;
    this.versions = page.versions;
    this.tags = page.tags;
  }
}
