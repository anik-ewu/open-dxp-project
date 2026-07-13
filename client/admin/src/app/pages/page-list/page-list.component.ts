import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageSummary } from '../../core/models/page.model';
import { PageService } from '../../core/services/page.service';

@Component({
  selector: 'app-page-list',
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './page-list.component.html',
})
export class PageListComponent implements OnInit {
  private readonly pageService = inject(PageService);

  pages: PageSummary[] = [];
  loading = true;

  ngOnInit(): void {
    this.pageService.getAll().subscribe((pages) => {
      this.pages = pages;
      this.loading = false;
    });
  }
}
