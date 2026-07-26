import { AuthConfig } from 'angular-oauth2-oidc';
import { environment } from '../../../environments/environment';

export const authConfig: AuthConfig = {
  // Must match the OpenIddict server's issuer exactly, including the trailing slash.
  issuer: `${environment.apiBaseUrl}/`,
  redirectUri: window.location.origin + '/auth-callback',
  postLogoutRedirectUri: window.location.origin,
  clientId: 'angular-admin',
  responseType: 'code',
  scope: 'openid profile email roles offline_access opendxp-api',
  useSilentRefresh: false,
  requireHttps: false,
  strictDiscoveryDocumentValidation: false,
};
