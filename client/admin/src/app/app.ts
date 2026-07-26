import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService, UserProfile } from './core/auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly authService = inject(AuthService);

  protected readonly title = signal('admin');

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  get userProfile(): UserProfile | null {
    return this.authService.getUserProfile();
  }

  login(): void {
    this.authService.login();
  }

  logout(): void {
    this.authService.logout();
  }
}
