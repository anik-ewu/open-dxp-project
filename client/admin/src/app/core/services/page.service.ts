import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreatePageRequest,
  PageDetail,
  PageSummary,
  PageVersion,
  UpdatePageDraftRequest,
} from '../models/page.model';

@Injectable({ providedIn: 'root' })
export class PageService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/pages`;

  getAll(): Observable<PageSummary[]> {
    return this.http.get<PageSummary[]>(this.baseUrl);
  }

  getById(id: string): Observable<PageDetail> {
    return this.http.get<PageDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreatePageRequest): Observable<PageDetail> {
    return this.http.post<PageDetail>(this.baseUrl, request);
  }

  updateDraft(id: string, request: UpdatePageDraftRequest): Observable<PageDetail> {
    return this.http.put<PageDetail>(`${this.baseUrl}/${id}`, request);
  }

  publish(id: string): Observable<PageVersion> {
    return this.http.post<PageVersion>(`${this.baseUrl}/${id}/publish`, {});
  }
}
