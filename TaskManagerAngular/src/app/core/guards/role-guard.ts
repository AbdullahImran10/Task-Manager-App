import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../services/auth';

export const roleGuard = (allowedRoles: string[]): CanActivateFn => {

  return () => {

    const auth = inject(Auth);
    const router = inject(Router);

    const user = auth.getUser();

    if (!user) {
      return router.createUrlTree(['/login']);
    }

    if (allowedRoles.includes(user.role)) {
      return true;
    }

    return router.createUrlTree(['/employee']);
  };
};