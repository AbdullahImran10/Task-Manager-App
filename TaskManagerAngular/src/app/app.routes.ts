import { Routes } from '@angular/router';
import { Login } from './auth/login/login';
import { Employee } from './employee/employee';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
    {
        path: 'login',
        component: Login
    },
    {
        path : '',
        redirectTo: 'login',
        pathMatch: 'full'
    },
    {
    path: 'employee',
    component: Employee,
    canActivate: [authGuard]
  }
];
