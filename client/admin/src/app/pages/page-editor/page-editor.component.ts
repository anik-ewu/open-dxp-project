import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PageDetail, PageVersion } from '../../core/models/page.model';
import { PageService } from '../../core/services/page.service';

@Component({
  selector: 'app-page-editor',
  standalone: true,
  imports: [FormsModule, RouterLink, DatePipe],
  templateUrl: './page-editor.component.html',
})
export class PageEditorComponent implements OnInit {
  private readonly pageService = inject(PageService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  pageId: string | null = null;
  slug = '';
  title = '';
  blocksJson = '[]';
  status: 'Draft' | 'Published' | null = null;
  versions: PageVersion[] = [];
  saving = false;
  error: string | null = null;

  ngOnInit(): void {
    this.pageId = this.route.snapshot.paramMap.get('id');
    if (this.pageId) {
      this.pageService.getById(this.pageId).subscribe((page) => this.applyDetail(page));
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

  private applyDetail(page: PageDetail): void {
    this.slug = page.slug;
    this.title = page.title;
    this.blocksJson = page.blocksJson;
    this.status = page.status;
    this.versions = page.versions;
  }
}
