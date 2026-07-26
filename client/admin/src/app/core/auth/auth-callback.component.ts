import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth-callback',
  standalone: true,
  template: `<p>Signing you in...</p>`,
})
export class AuthCallbackComponent implements OnInit {
  private readonly router = inject(Router);

  ngOnInit(): void {
    // By the time this component renders, the app-wide APP_INITIALIZER has already
    // run AuthService.init() (which calls tryLoginCodeFlow()), so the code exchange is done.
    this.router.navigateByUrl('/');
  }
}
