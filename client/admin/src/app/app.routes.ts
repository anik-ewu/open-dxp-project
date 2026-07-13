import { Routes } from '@angular/router';
import { PageEditorComponent } from './pages/page-editor/page-editor.component';
import { PageListComponent } from './pages/page-list/page-list.component';

export const routes: Routes = [
  { path: '', component: PageListComponent },
  { path: 'pages/new', component: PageEditorComponent },
  { path: 'pages/:id', component: PageEditorComponent },
];
