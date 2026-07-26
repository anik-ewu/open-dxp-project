import { Routes } from '@angular/router';
import { AuthCallbackComponent } from './core/auth/auth-callback.component';
import { authGuard } from './core/auth/auth.guard';
import { PageEditorComponent } from './pages/page-editor/page-editor.component';
import { PageListComponent } from './pages/page-list/page-list.component';

export const routes: Routes = [
  { path: 'auth-callback', component: AuthCallbackComponent },
  { path: '', component: PageListComponent, canActivate: [authGuard] },
  { path: 'pages/new', component: PageEditorComponent, canActivate: [authGuard] },
  { path: 'pages/:id', component: PageEditorComponent, canActivate: [authGuard] },
];
