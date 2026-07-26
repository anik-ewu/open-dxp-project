import { Injectable, inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { authConfig } from './auth.config';

export interface UserProfile {
  name: string;
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oauthService = inject(OAuthService);

  async init(): Promise<void> {
    this.oauthService.configure(authConfig);
    await this.oauthService.loadDiscoveryDocument();
    await this.oauthService.tryLoginCodeFlow();

    if (this.oauthService.hasValidAccessToken()) {
      this.oauthService.setupAutomaticSilentRefresh();
    }
  }

  login(): void {
    this.oauthService.initCodeFlow();
  }

  logout(): void {
    this.oauthService.logOut();
  }

  isAuthenticated(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  getUserProfile(): UserProfile | null {
    if (!this.isAuthenticated()) {
      return null;
    }

    const claims = this.oauthService.getIdentityClaims() as Record<string, unknown> | undefined;
    if (!claims) {
      return null;
    }

    const roleClaim = claims['role'];
    const roles = Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim as string] : [];

    return {
      name: (claims['name'] as string) ?? '',
      email: (claims['email'] as string) ?? '',
      roles,
    };
  }
}
