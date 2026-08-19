import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Auth } from '../../core/services/auth';
import { LoginRequest } from './login.model';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private auth = inject(Auth);
  email = '';
  password = '';
  errorMessage = '';
  login(){
      const credentials : LoginRequest = {
    email : this.email,
    password : this.password
  };
  this.auth.login(credentials).subscribe({
    next: (response) => {
      console.log('Login Successful');
      console.log(response);
    },
    error: (error)=> {
      console.log(error);
      this.errorMessage = error.error?.message || 'Login Failed';
    }
  });
  }
  
}
