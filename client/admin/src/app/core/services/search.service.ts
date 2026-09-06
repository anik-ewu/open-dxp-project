import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { RelatedPage } from '../models/page.model';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/search`;

  getRelated(pageId: string, take = 5): Observable<RelatedPage[]> {
    return this.http.get<RelatedPage[]>(`${this.baseUrl}/related/${pageId}`, { params: { take } });
  }
}
