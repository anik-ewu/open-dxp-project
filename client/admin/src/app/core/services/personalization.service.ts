import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreatePageVariantRequest, PageVariant, VariantAnalytics } from '../models/page-variant.model';

@Injectable({ providedIn: 'root' })
export class PersonalizationService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl;

  getVariants(pageId: string): Observable<PageVariant[]> {
    return this.http.get<PageVariant[]>(`${this.apiBaseUrl}/api/pages/${pageId}/variants`);
  }

  createVariant(pageId: string, request: CreatePageVariantRequest): Observable<PageVariant> {
    return this.http.post<PageVariant>(`${this.apiBaseUrl}/api/pages/${pageId}/variants`, request);
  }

  deleteVariant(pageId: string, variantId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiBaseUrl}/api/pages/${pageId}/variants/${variantId}`);
  }

  getAnalytics(pageId: string): Observable<VariantAnalytics[]> {
    return this.http.get<VariantAnalytics[]>(`${this.apiBaseUrl}/api/analytics/pages/${pageId}/variants`);
  }
}
